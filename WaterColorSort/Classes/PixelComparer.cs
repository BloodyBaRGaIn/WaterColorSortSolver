using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WaterColorSort.Classes
{
    internal sealed class PixelComparer : IEqualityComparer<PixelData>
    {
        public bool Equals([DisallowNull] PixelData p1, [DisallowNull] PixelData p2) => Math.Abs(p1.x - p2.x) < PixelData.PixelSize
                                                                                     && Math.Abs(p1.y - p2.y) < PixelData.PixelSize
                                                                                     && p1.Colorsimilar(p2);

        /// <summary>
        /// Dummy redefinition for only <see cref="Equals(PixelData, PixelData)"/> method comparing
        /// </summary>
        /// <param name="obj"><see cref="PixelData"/> object to get the hash of</param>
        /// <returns>Zero</returns>
        public int GetHashCode([DisallowNull] PixelData obj) => 0; 
    }
}
