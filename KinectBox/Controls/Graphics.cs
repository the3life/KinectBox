using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace KinectBox.Controls
{
    public class Graphics : FrameworkElement
    {
        private readonly VisualCollection _visualCollection;

        public Graphics()
        {
            _visualCollection = new VisualCollection(this);
        }

        protected override int VisualChildrenCount => _visualCollection.Count;

        public void Add(Visual visual)
        {
            _visualCollection.Add(visual);
        }

        public DrawingVisual Create()
        {
            return _visualCollection[_visualCollection.Add(new DrawingVisual())] as DrawingVisual;
        }
        
        public DrawingVisual CreateGeometry()
        {
            return _visualCollection[_visualCollection.Add(new DrawingVisual())] as DrawingVisual;
        }

        public StreamGeometryContext RenderGeometry()
        {
            var streamGeometry = new StreamGeometry();

            return streamGeometry.Open();
        }

        /*public void Render(Action<DrawingContext> action)
        {
            var drawingVisual = new DrawingVisual();
            
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                action.Invoke(drawingContext);
            }
        }

        public void RenderGeometry(Brush brush, Pen pen, Action<StreamGeometryContext> action)
        {
            Render(drawingContext =>
            {
                var streamGeometry = new StreamGeometry();

                using (var geometryContext = streamGeometry.Open())
                {
                    action.Invoke(geometryContext);
                }
                
                drawingContext.DrawGeometry(brush, pen, streamGeometry);
            });
        }

        public void DrawText(FormattedText formattedText, Point origin)
        {
            Render(context => context.DrawText(formattedText, origin));
        }

        public void DrawLine(Pen pen, Point point0, Point point1)
        {
            Render(context => context.DrawLine(pen, point0, point1));
        }

        public void DrawRectangle(Brush brush, Pen pen, Rect rectangle)
        {
            Render(context => context.DrawRectangle(brush, pen, rectangle));
        }

        public void DrawRoundedRectangle(
            Brush brush,
            Pen pen,
            Rect rectangle,
            double radiusX,
            double radiusY)
        {
            Render(context => context.DrawRoundedRectangle(brush, pen, rectangle, radiusX, radiusY));
        }

        public void DrawEllipse(
            Brush brush,
            Pen pen,
            Point center,
            double radiusX,
            double radiusY)
        {
            Render(context => context.DrawEllipse(brush, pen, center, radiusX, radiusY));
        }

        public void DrawImage(ImageSource imageSource, Rect rectangle)
        {
            Render(context => context.DrawImage(imageSource, rectangle));
        }

        public void LineTo(Brush brush, Pen pen, Point point, bool isStroked, bool isSmoothJoin)
        {
            RenderGeometry(brush, pen,context => context.LineTo(point, isStroked, isSmoothJoin));
        }

        public void QuadraticBezierTo(
            Brush brush, Pen pen,
            Point point1,
            Point point2,
            bool isStroked,
            bool isSmoothJoin)
        {
            RenderGeometry(brush, pen,context => context.QuadraticBezierTo(point1,point2, isStroked, isSmoothJoin));
        }

        public void BezierTo(
            Brush brush, Pen pen,
            Point point1,
            Point point2,
            Point point3,
            bool isStroked,
            bool isSmoothJoin)
        {
            RenderGeometry(brush, pen,context => context.BezierTo(point1, point2, point3, isStroked, isSmoothJoin));
        }

        public void PolyLineTo(Brush brush, Pen pen, IList<Point> points, bool isStroked, bool isSmoothJoin)
        {
            RenderGeometry(brush, pen,context => context.PolyLineTo(points, isStroked, isSmoothJoin));
        }

        public void PolyQuadraticBezierTo(
            Brush brush, Pen pen,
            IList<Point> points,
            bool isStroked,
            bool isSmoothJoin)
        {
            RenderGeometry(brush, pen,context => context.PolyQuadraticBezierTo(points, isStroked, isSmoothJoin));
        }

        public void ArcTo(
            Brush brush, Pen pen,
            Point point,
            Size size,
            double rotationAngle,
            bool isLargeArc,
            SweepDirection sweepDirection,
            bool isStroked,
            bool isSmoothJoin)
        {
            RenderGeometry(brush, pen,context => context.ArcTo(point, size, rotationAngle, isLargeArc, sweepDirection, isStroked, isSmoothJoin));
        }*/

        protected override Visual GetVisualChild(int index)
        {
            return _visualCollection[index];
        }
    }
}