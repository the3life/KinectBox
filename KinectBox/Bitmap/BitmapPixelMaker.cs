using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KinectBox.Bitmap
{
    public class BitmapPixelMaker
    {
        public BitmapPixelMaker(int width, int height, byte[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
            Stride = width * 4;
            Bitmap = new WriteableBitmap(width, width, 96, 96, PixelFormats.Bgr32, null);
        }
        
        public BitmapPixelMaker(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new byte[width * height * 4];
            Stride = width * 4;
            Bitmap = new WriteableBitmap(width, width, 96, 96, PixelFormats.Bgr32, null);
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Pixels { get; private set; }
        public int Stride { get; }
        public WriteableBitmap Bitmap { get; }

        public Color GetPixel(int x, int y)
        {
            var index = y * Stride + x * 4;
            var blue = Pixels[index++];
            var green = Pixels[index++];
            var red = Pixels[index++];
            var alpha = Pixels[index];

            return Color.FromArgb(alpha, red, green, blue);
        }

        public void SetPixel(int index, Color color)
        {
            Pixels[index + 2] = color.R;
            Pixels[index + 1] = color.G;
            Pixels[index + 0] = color.B;
            Pixels[index + 3] = color.A;
        }

        public void SetPixel(int x, int y, Color color)
        {
            var index = y * Stride + x * 4;

            Pixels[index++] = color.B;
            Pixels[index++] = color.G;
            Pixels[index++] = color.R;
            Pixels[index] = color.A;
        }
        
        public void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            var index = y * Stride + x * 4;

            Pixels[index++] = b;
            Pixels[index++] = g;
            Pixels[index++] = r;
        }
        
        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a)
        {
            var index = y * Stride + x * 4;

            Pixels[index++] = b;
            Pixels[index++] = g;
            Pixels[index++] = r;
            Pixels[index] = a;
        }

        public void SetPixels(byte[] pixels)
        {
            Pixels = pixels;
        }

        public void Draw()
        {
            Bitmap.WritePixels(new Int32Rect(0, 0, Width, Height), Pixels, Stride, 0);
        }
    }
}