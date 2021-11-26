
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WaterColorSort.Classes
{
    internal sealed class PixelComparer : IEqualityComparer<PixelData>
    {
        public bool Equals(PixelData p1, PixelData p2) => Math.Abs(p1.x - p2.x) < PixelData.PixelSize
                                                          && Math.Abs(p1.y - p2.y) < PixelData.PixelSize
                                                          && p1.Colorsimilar(p2);

        public int GetHashCode([DisallowNull] PixelData obj) => 0;
    }
}
