using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KinectBox.Kinect;
using KinectBox.Views;
using Microsoft.Kinect;

namespace KinectBox.ViewModels
{
    public class DepthViewerViewModel : ViewModel<DepthViewer>
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
        private WriteableBitmap _depthBitmap;

        public DepthViewerViewModel(KinectManager kinectManager)
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

                for (int i = 0, colorIndex = 0; i < _rawPixelData.Length; i++, colorIndex += 4)
                {
                    var depth = _rawPixelData[i];
                    var color = (depth.Depth >= 600 && depth.Depth <= 900) ? NormalDepthColor : InvalidDepthColor;

                    _pixelData[colorIndex + RedIndex] = color.R;
                    _pixelData[colorIndex + GreenIndex] = color.G;
                    _pixelData[colorIndex + BlueIndex] = color.B;
                }

                View.Dispatcher.Invoke(() =>
                {
                    if (haveNewFormat)
                    {
                        _depthBitmap = new WriteableBitmap(imageWidth, imageHeight, 96, 96, PixelFormats.Bgr32,
                            null);

                        View.DepthImage.Source = _depthBitmap;
                    }

                    _depthBitmap.WritePixels(new Int32Rect(0, 0, imageWidth, imageHeight), _pixelData,
                        imageWidth * 4,
                        0);
                });
            }
        }

        private void ResetOutput()
        {
        }
    }
}