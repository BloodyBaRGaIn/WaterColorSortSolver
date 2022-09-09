using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WaterColorSort.Classes
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal sealed class PixelComparer : IEqualityComparer<PixelData>
    {
        public bool Equals([DisallowNull] PixelData p1, [DisallowNull] PixelData p2)
        {
            return Math.Abs(p1.X - p2.X) < PixelData.PixelSize
                && Math.Abs(p1.Y - p2.Y) < PixelData.PixelSize
                && p1.Colorsimilar(p2);
        }

        /// <summary>
        /// Dummy redefinition for only <see cref="Equals(PixelData, PixelData)"/> method comparing correctly
        /// </summary>
        /// <param name="obj"><see cref="PixelData"/> object to get the hash of</param>
        /// <returns>Zero</returns>
        public int GetHashCode([DisallowNull] PixelData obj)
        {
            return 0;
        }
    }
}
