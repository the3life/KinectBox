using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using KinectBox.Helpers;
using KinectBox.Kinect;
using KinectBox.Views;
using Microsoft.Kinect;

namespace KinectBox.ViewModels
{
    public class BoxViewerViewModel:ViewModel<BoxViewer>
    {
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        private static readonly Color NormalDepthColor = Colors.Gold;
        private static readonly Color InvalidDepthColor = Colors.Black;

        private readonly KinectManager _kinectManager;

        private KinectImageProcess _process;

        private DepthImageFormat _lastImageFormat;
        private DepthImagePixel[] _rawPixelData;
        private byte[] _pixelData;
        
        private DepthColorizer _colorizer = new DepthColorizer();
        
        static double fx_d = 1.0 / 5.9421434211923247e+02;
        static double fy_d = 1.0 / 5.9104053696870778e+02;
        static double cx_d = 3.3930780975300314e+02;
        static double cy_d = 2.4273913761751615e+02;
        
        private int _frameCount = 20;
        
        public BoxViewerViewModel(KinectManager kinectManager)
        {
            _kinectManager = kinectManager;
        }
        
        public override void OnLoaded()
        {
            base.OnLoaded();
            
            _process = new KinectImageProcess(_kinectManager.ActiveSensor, OnFrameReady, ResetOutput);
        }
        
        private void OnFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;

                var imageWidth = frame.Width;
                var imageHeight = frame.Height;
                var haveNewFormat = _lastImageFormat != frame.Format;

                if (haveNewFormat)
                {
                    _lastImageFormat = frame.Format;
                    _rawPixelData = new DepthImagePixel[frame.PixelDataLength];
                    _pixelData = new byte[frame.Width * frame.Height * 4];
                }

                frame.CopyDepthImagePixelDataTo(_rawPixelData);
                
                //_colorizer.ConvertDepthFrame(_rawPixelData, frame.MinDepth, frame.MaxDepth, KinectDepthTreatment.ClampUnreliableDepths, _pixelData);

                for (int i = 0, colorIndex = 0; i < _rawPixelData.Length; i++, colorIndex += 4)
                {
                    var depth = _rawPixelData[i];
                    var color = (depth.Depth >= 600 && depth.Depth <= 950) ? NormalDepthColor : InvalidDepthColor;

                    if (color == NormalDepthColor)
                    {
                        //_depth = depth.Depth;
                    }

                    _pixelData[colorIndex + RedIndex] = color.R;
                    _pixelData[colorIndex + GreenIndex] = color.G;
                    _pixelData[colorIndex + BlueIndex] = color.B;
                }
                
                View.Dispatcher.Invoke(() =>
                {
                    var bitmapSource = BitmapSource.Create(imageWidth, imageHeight, 96, 96, PixelFormats.Bgr32, null, _pixelData, imageWidth * 4);

                    var image = new Image<Bgr, byte>(bitmapSource.ToBitmap());
                    var grayImage = image.Convert<Gray, byte>();

                    using (var storage = new MemStorage())
                    {
                        var contours = grayImage.FindContours(
                            Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                            Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL,
                            storage);

                        for (var i = 0; contours != null; contours = contours.HNext)
                        {
                            i++;
                            
                            if ((contours.Area > Math.Pow(10, 2)) && (contours.Area < Math.Pow(1000, 2)))
                            {
                                var box = contours.GetMinAreaRect();
                                image.Draw(box, new Bgr(System.Drawing.Color.Red), 2);
                            }
                        }
                    }

                    View.DepthImage.Source = image.ToBitmapSource();
                });
                
                if (_frameCount == 20)
                {
                    _frameCount = 0;

                    
                }

                _frameCount++;
            }
        }

        private void ResetOutput()
        {
        }
    }
}