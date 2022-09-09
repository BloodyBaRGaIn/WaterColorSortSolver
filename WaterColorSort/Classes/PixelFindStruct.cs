using System;
using System.Collections.Generic;
using System.Drawing;

namespace WaterColorSort.Classes
{
    internal readonly struct PixelFindStruct
    {
        internal readonly NamedBitmap namedBitmap;
        internal readonly Bitmap img_cpy;
        internal readonly List<PixelData> result;

        internal PixelFindStruct(NamedBitmap namedBitmap, Bitmap img_cpy, List<PixelData> result)
        {
            this.namedBitmap = namedBitmap;
            this.img_cpy = img_cpy;
            this.result = result;
        }

        public override bool Equals(object obj)
        {
            return obj != null
              && obj is PixelFindStruct other
              && EqualityComparer<NamedBitmap>.Default.Equals(namedBitmap, other.namedBitmap)
              && EqualityComparer<Bitmap>.Default.Equals(img_cpy, other.img_cpy)
              && EqualityComparer<List<PixelData>>.Default.Equals(result, other.result);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(namedBitmap, img_cpy, result);
        }

        public void Deconstruct(out NamedBitmap namedBitmap, out Bitmap img_cpy, out List<PixelData> result)
        {
            namedBitmap = this.namedBitmap;
            img_cpy = this.img_cpy;
            result = this.result;
        }

        public static implicit operator (NamedBitmap namedBitmap, Bitmap img_cpy, List<PixelData> result)(PixelFindStruct value)
        {
            return (value.namedBitmap, value.img_cpy, value.result);
        }

        public static implicit operator PixelFindStruct((NamedBitmap namedBitmap, Bitmap img_cpy, List<PixelData> result) value)
        {
            return new(value.namedBitmap, value.img_cpy, value.result);
        }
    }
}
