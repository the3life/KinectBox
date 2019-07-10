using System.Collections.Generic;
using System.Linq;
using AForge;
using Point = System.Windows.Point;

namespace KinectBox.Extensions
{
    public static class IntPointExtensions
    {
        public static Point ToPoint(this IntPoint point)
        {
            return new Point(point.X, point.Y);
        }
        
        public static List<Point> ToPoints(this List<IntPoint> point)
        {
            return point.Select(intPoint => intPoint.ToPoint()).ToList();
        }
    }
}