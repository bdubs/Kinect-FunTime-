using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;


namespace kinect_fun
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor;
        MediaPlayer sp = new MediaPlayer();
        MediaPlayer sp2 = new MediaPlayer();
        bool closeToChangeSong = true;
        public MainWindow()
        {
            sp2.Open(new Uri("C:\\Users\\vost3550\\Documents\\Visual Studio 2010\\Projects\\kinect fun\\kinect fun\\53576_newgrounds_guitar.mp3"));
            sp.Open(new Uri("C:\\Users\\vost3550\\Documents\\Visual Studio 2010\\Projects\\kinect fun\\kinect fun\\Nyan_Cat.mp3"));
            InitializeComponent();
        }
           
        private void kinectSensorChooser1_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];
            }

            if (_sensor.Status == KinectStatus.Connected)
            {           
                _sensor.ColorStream.Enable();
                _sensor.DepthStream.Enable();
                _sensor.SkeletonStream.Enable();
                _sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(_sensor_AllFramesReady);
                _sensor.Start();
            }
        }

        void _sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //throw new NotImplementedException();
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame == null || colorFrame == null)
                    {
                        return;
                    }

                    sp.Play();//start our music!!!
                    sp.Volume = 0.5;
                    //sp2.Play();


                    byte[] depthPixels = danceSilhouette(depthFrame);//after messing with depthdata
                    byte[] pixels = new byte[colorFrame.PixelDataLength];//original thingy

                    //***copy data out into our byte array***
                    colorFrame.CopyPixelDataTo(pixels);

                    byte[] pixels2 = new byte[pixels.Length];//pixels after modifying the color image
                    pixels2 = messWithImage(pixels);
                    int stride = colorFrame.Width * 4; //because RGB + alpha

                    image1.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96,
                        PixelFormats.Bgr32, null, depthPixels, stride);
                    image2.Source = BitmapSource.Create(colorFrame.Width, colorFrame.Height, 96, 96,
                        PixelFormats.Bgr32, null, pixels2, stride);
                  // BitmapSource.Create();
                }//end depth frame
            }//end color frame

        }//end allFramesReady

        public byte[] messWithImage(byte[] pixels)
        {
            byte[] pixelResult = new byte[pixels.Length];
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            for (int i = 0; i < pixels.Length; i += 4)
            {

                //Normal
                pixelResult[i + BlueIndex] = pixels[i + BlueIndex];
                pixelResult[i + GreenIndex] = pixels[i + GreenIndex];
                pixelResult[i + RedIndex] = pixels[i + RedIndex];
                //Terminator Vision
                //pixelResult[i + BlueIndex] = 0;
                //pixelResult[i + GreenIndex] = 0;
                //pixelResult[i + RedIndex] = pixels[i+RedIndex];

                //Night Vision
                //pixelResult[i + BlueIndex] = 0;
                //pixelResult[i + GreenIndex] = pixels[i+GreenIndex];
                //pixelResult[i + RedIndex] = 0;

                //Blue Vision
                //pixelResult[i + BlueIndex] = pixels[i+BlueIndex];
                //pixelResult[i + GreenIndex] = 0;
                //pixelResult[i + RedIndex] = 0;
            }
            return pixelResult;
        }

        public byte[] danceSilhouette(DepthImageFrame depthFrame)
        {
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //Height * Width * 4 pixels
            //(Blue, Red, Green, empty pixels per unit of height
            Byte[] pixelies = new Byte[depthFrame.Height * depthFrame.Width * 4];

            //hardcoded locations to the various pixels
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            for(int depthIndex = 0, colorIndex = 0;
                depthIndex <rawDepthData.Length && colorIndex < pixelies.Length;
                depthIndex++, colorIndex+=4){
                //get the player
                    int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                //get the depth value
                    int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;
                    //Volume needs to be between 1 and 0, we want the volume to vary with player distance
                /* If the player is between 0 and 1.1m (0-3ft) we make the player appear as a sillhoutte
                 *We also make the volume very loud at this distance. Note that volume is between 0 (quiet) and 1 (loud)
                 */
                /*if (player != 0 && depth <=1100)
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 0;
                        pixelies[colorIndex + RedIndex] = 0;                       
                    }*/
                
                
                     if (player != 0 && depth < 1100)
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 0;
                        pixelies[colorIndex + RedIndex] = 0;
                        if (sp.Volume != 0.8)
                        {
                            sp.Volume = 0.8;
                            image2.Opacity = 0.0;
                        }
                    }
                    else if (player != 0 && depth > 1100 && depth < 1400)
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 0;
                        pixelies[colorIndex + RedIndex] = 0;
                        if (sp.Volume != 0.6)
                        {
                            sp.Volume = 0.6;
                            image2.Opacity = 0;
                        }
                       
                    }
                    else if (player != 0 && depth > 1400 && depth < 1700)
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 0;
                        pixelies[colorIndex + RedIndex] = 0;
                        if (sp.Volume != 0.4)
                        {
                            sp.Volume = 0.4;
                            image2.Opacity = 0.0;
                        }
                    }
                    else if (player != 0 && depth > 1700)
                    {
                        pixelies[colorIndex + BlueIndex] = 255;
                        pixelies[colorIndex + GreenIndex] = 255;
                        pixelies[colorIndex + RedIndex] = 255;
                        if (sp.Volume != 0.3)
                        {
                            sp.Volume = 0.3;
                            image2.Opacity = 0.6;
                        }
                    }
                    else
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 255;
                        pixelies[colorIndex + RedIndex] = 0;
                    }
                    
                    /*else if (depth <= 100)
                    {
                        pixelies[colorIndex + BlueIndex] = 255;
                        pixelies[colorIndex + GreenIndex] = 0;
                        pixelies[colorIndex + RedIndex] = 0;
                    }
                    else if (depth > 100 && depth <= 200)
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 255;
                        pixelies[colorIndex + RedIndex] = 0;
                    }
                    else
                    {
                        pixelies[colorIndex + BlueIndex] = 0;
                        pixelies[colorIndex + GreenIndex] = 0;
                        pixelies[colorIndex + RedIndex] = 255;
                    }*/
                }//end loopy
            return pixelies;
        }


        public void volumeByDistance(DepthImageFrame depthFrame, MediaPlayer ourSongPlayer)
        {

        }
    }//public class MainWindow
}
