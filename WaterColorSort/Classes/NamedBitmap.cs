
using System;
using System.Collections.Generic;
using System.Drawing;

namespace WaterColorSort.Classes
{
    internal readonly struct NamedBitmap
    {
        internal readonly Bitmap img;
        internal readonly string name;

        internal NamedBitmap(Bitmap img, string name)
        {
            this.img = img;
            this.name = name;
        }

        public override bool Equals(object obj) => obj != null &&
                                                   obj is NamedBitmap other &&
                                                   EqualityComparer<Bitmap>.Default.Equals(img, other.img) &&
                                                   name == other.name;

        public override int GetHashCode() => HashCode.Combine(img, name);

        public void Deconstruct(out Bitmap img, out string name)
        {
            img = this.img;
            name = this.name;
        }

        public static implicit operator (Bitmap img, string name)(NamedBitmap value) => (value.img, value.name);

        public static implicit operator NamedBitmap((Bitmap img, string name) value) => new(value.img, value.name);
    }
}
