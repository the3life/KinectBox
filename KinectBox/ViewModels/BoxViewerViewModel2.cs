using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using KinectBox.Extensions;
using KinectBox.Kinect;
using KinectBox.Views;
using Microsoft.Kinect;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace KinectBox.ViewModels
{
    public class BoxViewerViewModel2 : ViewModel<BoxViewer>
    {
        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        private static readonly Color NormalDepthColor = Colors.Gold;
        private static readonly Color InvalidDepthColor = Colors.Black;

        private readonly KinectManager _kinectManager;

        private KinectImageProcess _process;

        private DepthImageFormat _lastDepthImageFormat;

        private DepthImagePixel[] _rawDepthPixelData;
        private byte[] _depthPixelData;
        private WriteableBitmap _depthBitmap;

        private BitmapData _bitmapData;
        
        private int _frameCount = 50;
        
        private BlobCounter _blobCounter = new BlobCounter();
        private SimpleShapeChecker _shapeChecker = new SimpleShapeChecker();
        
        static double fx_d = 1.0 / 5.9421434211923247e+02;
        static double fy_d = 1.0 / 5.9104053696870778e+02;
        static double cx_d = 3.3930780975300314e+02;
        static double cy_d = 2.4273913761751615e+02;

        public BoxViewerViewModel2(KinectManager kinectManager)
        {
            _kinectManager = kinectManager;
            
            _blobCounter.FilterBlobs = true;
            _blobCounter.MinWidth = 50;
            _blobCounter.MinHeight = 50;
        }

        public override void OnLoaded()
        {
            base.OnLoaded();

            _process = new KinectImageProcess(_kinectManager.ActiveSensor, OnFrameReady, ResetOutput);
        }

        private short _depth = 0;

        private void OnFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;

                var imageWidth = frame.Width;
                var imageHeight = frame.Height;
                var haveNewFormat = _lastDepthImageFormat != frame.Format;

                if (haveNewFormat)
                {
                    _lastDepthImageFormat = frame.Format;
                    _rawDepthPixelData = new DepthImagePixel[frame.PixelDataLength];
                    _depthPixelData = new byte[frame.Width * frame.Height * 4];

                    if (_bitmapData != null)
                    {
                        Marshal.FreeHGlobal(_bitmapData.Scan0);

                        _bitmapData = null;
                    }

                    _bitmapData = new BitmapData
                    {
                        Width = imageWidth,
                        Height = imageHeight,
                        Stride = imageWidth * 4,
                        PixelFormat = PixelFormat.Format32bppRgb,
                        Scan0 = Marshal.AllocHGlobal(_depthPixelData.Length)
                    };
                }

                frame.CopyDepthImagePixelDataTo(_rawDepthPixelData);

                for (int i = 0, colorIndex = 0; i < _rawDepthPixelData.Length; i++, colorIndex += 4)
                {
                    var depth = _rawDepthPixelData[i];
                    var color = (depth.Depth >= 600 && depth.Depth <= 900) ? NormalDepthColor : InvalidDepthColor;

                    if (color == NormalDepthColor)
                    {
                        _depth = depth.Depth;
                    }

                    _depthPixelData[colorIndex + RedIndex] = color.R;
                    _depthPixelData[colorIndex + GreenIndex] = color.G;
                    _depthPixelData[colorIndex + BlueIndex] = color.B;
                }
                
                if (_frameCount == 50)
                {
                    _frameCount = 0;

                    Marshal.Copy(_depthPixelData, 0, _bitmapData.Scan0, _depthPixelData.Length);

                    View.Dispatcher.Invoke(DrawRectangle);
                }

                _frameCount++;
            }
        }

        private void ResetOutput()
        {
        }
        
        /*static PVector depthToWorld(int x, int y, float [] depthLookUp, int depthValue) {
            PVector result = new PVector();
            double depth =  depthLookUp[depthValue];
            result.x = (float)((x - cx_d) * depth * fx_d);
            result.y = (float)((y - cy_d) * depth * fy_d);
            result.z = (float)(depth);
            return result;
        }*/

        private double DepthToWorld(int x, int z)
        {
            return (x - cx_d) * z * fx_d;
        }

        private void DrawRectangle()
        {
            _blobCounter.ProcessImage(_bitmapData);

            var rectangles = _blobCounter.GetObjectsRectangles();
            var blobs = _blobCounter.GetObjectsInformation();
            
            //View.Canvas.Children.Clear();

            foreach (var blob in blobs)
            {
                var edges = _blobCounter.GetBlobsEdgePoints(blob);
                List<IntPoint> corners;

                if (_shapeChecker.IsConvexPolygon(edges, out corners))
                {
                    var line = new System.Windows.Shapes.Line
                    {
                        X1 = corners[1].X,
                        Y1 = corners[1].Y,
                        X2 = corners[2].X,
                        Y2 = corners[2].Y,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2
                    };

                    _depth = 850;

                    var pixelWidth = Math.Abs(corners[1].X - corners[2].X);
                    var realWidth = Math.Abs(DepthToWorld(corners[1].X, _depth) - DepthToWorld(corners[2].X, _depth));
                    var realWidth2 = Math.Abs(DepthToWorld(pixelWidth, _depth));

                    Debug.WriteLine("Real Width {0}, Real Width 2 {1}, Pixel Width: {2}, Depth: {3}", realWidth, realWidth2, pixelWidth, _depth);
                    
                    /*var mmWidth = (pixelWidth * 25.4) / 96;
                    var realMMWidth = Math.Tan(28.5) * 2 * 650 * pixelWidth;
                    
                    var zMeters = 650.0 / 1000.0;
                    var a = 6 - 2;
                    var b = corners[1].X - (639 / 2);
                    var c = Math.Tan(57 / 2);
                    var d = zMeters / 640;
                    var x1Meters = a * (corners[1].X - (639 / 2)) * c * d;
                    var x2Meters = a * (corners[2].X - (639 / 2)) * c * d;
                    var diff = Math.Abs(x1Meters - x2Meters);*/

                    var polygon = new Polygon
                    {
                        Stroke = Brushes.Red, StrokeThickness = 2, Points = new PointCollection(corners.ToPoints())
                    };

                    /*View.Canvas.Children.Add(polygon);
                    View.Canvas.Children.Add(line);*/
                }
            }

            /*View.Canvas.Children.Clear();

            foreach (var rectangle in rectangles)
            {
                var rect = new Rectangle
                    {Width = rectangle.Width, Height = rectangle.Height, Stroke = Brushes.Red, StrokeThickness = 2};

                Canvas.SetLeft(rect, rectangle.X);
                Canvas.SetTop(rect, rectangle.Y);

                View.Canvas.Children.Add(rect);
            }

            Console.WriteLine();*/

            /*var bitmap = new BitmapReader(imageWidth, imageHeight, pixelData);
            var edges = new List<Point>();*/

            /*for (var y = 0; y < imageHeight; y++)
            {
                for (var x = 0; x < imageWidth; x++)
                {
                    if (bitmap.IsFill(x, y))
                    {
                        edges.Add(new Point(x, y));
                    }
                }
            }*/

            /*var bitmap = new System.Drawing.Bitmap(imageWidth, imageHeight, PixelFormat.Format32bppRgb);

            for (var y = 0; y < imageHeight; y++)
            {
                for (var x = 0; x < imageWidth; x++)
                {
                    var index = y * imageWidth + x;

                    var r = pixelData[index + RedIndex];
                    var g = pixelData[index + GreenIndex];
                    var b = pixelData[index + BlueIndex];
                    
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(r, g, b));
                }
            }

            _blobCounter.ProcessImage(bitmap);

            var blobs = _blobCounter.GetObjectsInformation();
            
            var streamGeometry = new StreamGeometry();

            using (var geometryContext = streamGeometry.Open())
            {
                foreach (var blob in blobs)
                {
                    var edgePoint = _blobCounter.GetBlobsEdgePoints(blob);
                    List<IntPoint> corners;

                    if (_shapeChecker.IsConvexPolygon(edgePoint, out corners))
                    {
                        var subType = _shapeChecker.CheckPolygonSubType(corners);
                        
                        geometryContext.BeginFigure(corners[0].ToPoint(), false, true);
                        geometryContext.PolyLineTo(corners.ToPoints(), true, true);
                    }
                }

                Console.WriteLine();
            }
            
            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawGeometry(Brushes.Red, new Pen(Brushes.Gold, 2), streamGeometry);
            }
            
            View.Graphics.Add(drawingVisual);*/
        }
    }
}