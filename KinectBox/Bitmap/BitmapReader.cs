using System.Windows.Media;

namespace KinectBox.Bitmap
{
    public class BitmapReader
    {
        private const int AlphaIndex = 3;
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        private int _width;
        private int _height;
        private byte[] _pixels;
        private int _stride;

        public BitmapReader(int width, int height, byte[] pixels)
        {
            _width = width;
            _height = height;
            _pixels = pixels;
            _stride = width * 4;
        }

        public bool IsFill(int x, int y)
        {
            var index = y * _stride + x * 4;

            var color = (_pixels[index + AlphaIndex] << 24) | (_pixels[index + RedIndex] << 16) |
                        (_pixels[index + GreenIndex] << 8) | (_pixels[index + BlueIndex] << 0);

            return color != 16777215;
        }
    }
}