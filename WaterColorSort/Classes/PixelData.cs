using System;
using System.Drawing;

namespace WaterColorSort.Classes
{
    internal struct PixelData
    {
        internal int x, y;
        internal Color c;

        internal PixelData(int x, int y, Color c)
        {
            this.x = x;
            this.y = y;
            this.c = c;
        }

        internal bool Colorsimilar(PixelData other)
        {
            return (Math.Abs(c.R - other.c.R) <= 10 && Math.Abs(c.G - other.c.G) <= 10 && Math.Abs(c.B - other.c.B) <= 10);
        }

        internal bool Colorsimilar(Color other)
        {
            return Colorsimilar(new PixelData(0, 0, other));
        }
    }
}
