
using System;
using System.Drawing;

namespace WaterColorSort.Classes
{
    internal struct UserColor : IComparable
    {
        internal readonly Color color;
        internal readonly string name;

        internal UserColor(Color color, string name = "")
        {
            this.color = color;
            this.name = name;
        }

        internal static System.Collections.Generic.List<string> Names = new();
        private static readonly ConsoleColor[] consoleColors = new ConsoleColor[]
        {
            ConsoleColor.Blue,
            ConsoleColor.DarkRed,
            ConsoleColor.Cyan,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGray,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.White,
            ConsoleColor.DarkMagenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow,
            ConsoleColor.Black
        };

        internal ConsoleColor GetNearestColor() => Names.Count == 0 || Names.IndexOf(name) == -1 ? ConsoleColor.Black : consoleColors[Names.IndexOf(name)];

        public static implicit operator Color(UserColor color) => color.color;

        public static explicit operator UserColor(Color color) => new(color);

        public override bool Equals(object obj) => obj is UserColor color && color.color == this.color;

        public static bool operator ==(UserColor color1, UserColor color2) => color1.Equals(color2);

        public static bool operator !=(UserColor color1, UserColor color2) => !color1.Equals(color2);

        public override int GetHashCode() => HashCode.Combine(color, name);

        int IComparable.CompareTo(object obj)
        {
            return name.CompareTo(((UserColor)obj).name);
        }
    }
}
