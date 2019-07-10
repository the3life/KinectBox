using System;
using System.Windows;
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
    public class ImageViewerViewModel : ViewModel<ImageViewer>
    {
        private readonly KinectManager _kinectManager;

        private KinectImageProcess _process;

        private ColorImageFormat _lastImageFormat;
        private byte[] _rawPixelData;
        private byte[] _pixelData;
        private WriteableBitmap _bitmap;

        public ImageViewerViewModel(KinectManager kinectManager)
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
            var imageWith = 0;
            var imageHeight = 0;
            var bytesPerPixel = 0;
            var convertToRgb = false;
            var haveNewFormat = false;

            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null) return;

                imageWith = frame.Width;
                imageHeight = frame.Height;
                bytesPerPixel = frame.BytesPerPixel;
                haveNewFormat = _lastImageFormat != frame.Format;
                
                if (frame.Format == ColorImageFormat.RawBayerResolution640x480Fps30 ||
                    frame.Format == ColorImageFormat.RawBayerResolution1280x960Fps12)
                {
                    convertToRgb = true;
                    bytesPerPixel = 4;
                }

                if (haveNewFormat)
                {
                    _lastImageFormat = frame.Format;
                    _rawPixelData = new byte[frame.PixelDataLength];
                    _pixelData = new byte[frame.Width * frame.Height * bytesPerPixel];
                }

                if (convertToRgb)
                {
                    frame.CopyPixelDataTo(_rawPixelData);

                    ConvertBayerToRgb32(imageWith, imageHeight, _rawPixelData, _pixelData);
                }
                else
                {
                    frame.CopyPixelDataTo(_pixelData);
                }

                View.Dispatcher.Invoke(() =>
                {
                    if (haveNewFormat)
                    {
                        _bitmap = new WriteableBitmap(imageWith, imageHeight, 96, 96, PixelFormats.Bgr32,
                            null);

                        //View.ColorImage.Source = _bitmap;
                    }

                    _bitmap.WritePixels(new Int32Rect(0, 0, imageWith, imageHeight), _pixelData, imageWith * bytesPerPixel, 0);
                    
                    var bitmapSource = BitmapSource.Create(imageWith, imageHeight, 96, 96, PixelFormats.Bgr32, null, _pixelData, imageWith * 4);

                    var b = bitmapSource.ToBitmap();
                    var image = new Image<Bgr, byte>(b);
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

                    View.ColorImage.Source = image.ToBitmapSource();
                });

                /*for (var y = 0; y < frame.Height; y++)
                {
                    for (var x = 0; x < frame.Width; x++)
                    {
                        var index = y * frame.Width + x;
                        var depth = _depthPixels[index].Depth;
    
                        if (depth > 600 && depth < 900)
                        {
                            PixelMaker.SetPixel(x, y, Colors.Gold);
                        }
                        else
                        {
                            PixelMaker.SetPixel(x, y, Colors.Transparent);
                        }
                    }
                }*/

                //PixelMaker.Draw();
            }
        }
        
        private void ConvertBayerToRgb32(int width, int height, byte[] rawPixelData, byte[] pixelData)
        {
            for (var y = 0; y < height; y += 2)
            {
                for (var x = 0; x < width; x += 2)
                {
                    var firstRowOffset = (y * width) + x;
                    var secondRowOffset = firstRowOffset + width;

                    var red = rawPixelData[firstRowOffset + 1];
                    var green1 = rawPixelData[firstRowOffset];
                    var green2 = rawPixelData[secondRowOffset + 1];
                    var blue = rawPixelData[secondRowOffset];

                    firstRowOffset *= 4;
                    secondRowOffset *= 4;

                    pixelData[firstRowOffset]     = blue;
                    pixelData[firstRowOffset + 1] = green1;
                    pixelData[firstRowOffset + 2] = red;

                    pixelData[firstRowOffset + 4] = blue;
                    pixelData[firstRowOffset + 5] = green1;
                    pixelData[firstRowOffset + 6] = red;

                    pixelData[secondRowOffset]     = blue;
                    pixelData[secondRowOffset + 1] = green2;
                    pixelData[secondRowOffset + 2] = red;

                    pixelData[secondRowOffset + 4] = blue;
                    pixelData[secondRowOffset + 5] = green2;
                    pixelData[secondRowOffset + 6] = red;
                }
            }
        }

        private void ResetOutput()
        {
        }
    }
}