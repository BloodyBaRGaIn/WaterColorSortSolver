using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace WaterColorSort.Classes
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class PointExtensions
    {
        internal static Point Average(this IEnumerable<Point> points)
        {
            return new Point((int)points.Average(GetX_Point), (int)points.Average(GetY_Point));
        }

        internal static Point Average(this IEnumerable<PixelData> pixels)
        {
            return pixels.Select(d => (Point)d).Average();
        }

        private static Func<Point, int> GetX_Point => p => p.X;
        private static Func<Point, int> GetY_Point => p => p.Y;
    }
}
