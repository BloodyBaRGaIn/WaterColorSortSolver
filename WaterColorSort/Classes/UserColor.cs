using System;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal readonly struct UserColor : IComparable
    {
        internal readonly Color color;
        internal readonly string name;

        internal UserColor(Color color, string name = "")
        {
            this.color = color;
            this.name = name;
        }

        internal ConsoleColor GetColorByName()
        {
            string this_name = name;
            return BitmapWork.named_resources.FirstOrDefault(r => r.namedBitmap.name == this_name).consoleColor;
        }

        public static implicit operator Color(UserColor color)
        {
            return color.color;
        }

        public static explicit operator UserColor(Color color)
        {
            return new(color);
        }

        public override bool Equals(object obj)
        {
            return obj is UserColor color && color.color == this.color;
        }

        public static bool operator ==(UserColor color1, UserColor color2)
        {
            return color1.Equals(color2);
        }

        public static bool operator !=(UserColor color1, UserColor color2)
        {
            return !color1.Equals(color2);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(color, name);
        }

        int IComparable.CompareTo(object obj)
        {
            return name.CompareTo(((UserColor)obj).name);
        }

        public override string ToString()
        {
            return name;
        }
    }
}
