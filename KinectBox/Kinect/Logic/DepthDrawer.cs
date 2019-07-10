using System.Windows.Media;
using KinectBox.Bitmap;
using Microsoft.Kinect;

namespace KinectBox.Kinect.Logic
{
    public class DepthDrawer
    {
        private DepthImageFormat _format;
        private DepthImagePixel[] _depthPixels;
        
        private DepthColorizer _colorizer = new DepthColorizer();

        public void Draw(DepthImageFrame frame, BitmapPixelMaker pixelMaker)
        {
            if (frame == null) return;

            if (_format != frame.Format)
            {
                _format = frame.Format;
                _depthPixels = new DepthImagePixel[frame.PixelDataLength];
            }

            /*frame.CopyDepthImagePixelDataTo(_depthPixels);
            
            _colorizer.ConvertDepthFrame(_depthPixels, frame.MinDepth, frame.MaxDepth, KinectDepthTreatment.ClampUnreliableDepths, pixelMaker.Pixels);*/

            for (var y = 0; y < frame.Height; y++)
            {
                for (var x = 0; x < frame.Width; x++)
                {
                    var index = y * frame.Width + x;
                    var depth = _depthPixels[index].Depth;

                    if (depth > 600 && depth < 900)
                    {
                        pixelMaker.SetPixel(x, y, Colors.Gold);
                    }
                    else
                    {
                        pixelMaker.SetPixel(x, y, Colors.Transparent);
                    }
                }
            }
        }
    }
}