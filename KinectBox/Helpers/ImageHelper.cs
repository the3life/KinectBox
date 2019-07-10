using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace KinectBox.Helpers
{
    public static class ImageHelper
    {
        public static System.Drawing.Bitmap ToBitmap(this BitmapSource bitmapSource)
        {
            using (var stream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);

                return new System.Drawing.Bitmap(stream);
            }
        }
        
        public static BitmapSource ToBitmapSource(this IImage image)
        {
            using (var bitmap = image.Bitmap)
            {
                var pointer = bitmap.GetHbitmap();

                var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(pointer, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(pointer);

                return bitmapSource;
            }
        }
        
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr obj);
    }
}