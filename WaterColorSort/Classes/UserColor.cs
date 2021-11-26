
using System;
using System.Collections.Generic;
using System.Drawing;

namespace WaterColorSort.Classes
{
    internal readonly struct UserColor : IComparable
    {
        internal readonly Color color;
        internal readonly string name;

        internal UserColor(Color color, string name = "")
        {
            this.color = color;
            this.name = name;
        }

        private static readonly Dictionary<string, ConsoleColor> consoleColors = new();

        internal static void InitDict()
        {
            consoleColors.Clear();
            consoleColors.Add(BitmapWork.named_resources[0].name, ConsoleColor.Blue);
            consoleColors.Add(BitmapWork.named_resources[1].name, ConsoleColor.DarkRed);
            consoleColors.Add(BitmapWork.named_resources[2].name, ConsoleColor.Cyan);
            consoleColors.Add(BitmapWork.named_resources[3].name, ConsoleColor.DarkCyan);
            consoleColors.Add(BitmapWork.named_resources[4].name, ConsoleColor.Black);
            consoleColors.Add(BitmapWork.named_resources[5].name, ConsoleColor.DarkGray);
            consoleColors.Add(BitmapWork.named_resources[6].name, ConsoleColor.Green);
            consoleColors.Add(BitmapWork.named_resources[7].name, ConsoleColor.Magenta);
            consoleColors.Add(BitmapWork.named_resources[8].name, ConsoleColor.DarkYellow);
            consoleColors.Add(BitmapWork.named_resources[9].name, ConsoleColor.White);
            consoleColors.Add(BitmapWork.named_resources[10].name, ConsoleColor.DarkMagenta);
            consoleColors.Add(BitmapWork.named_resources[11].name, ConsoleColor.Red);
            consoleColors.Add(BitmapWork.named_resources[12].name, ConsoleColor.Yellow);
        }

        internal ConsoleColor GetNearestColor() => consoleColors.TryGetValue(name ?? "", out ConsoleColor consoleColor) ? consoleColor : ConsoleColor.Black;

        public static implicit operator Color(UserColor color) => color.color;

        public static explicit operator UserColor(Color color) => new(color);

        public override bool Equals(object obj) => obj is UserColor color && color.color == this.color;

        public static bool operator ==(UserColor color1, UserColor color2) => color1.Equals(color2);

        public static bool operator !=(UserColor color1, UserColor color2) => !color1.Equals(color2);

        public override int GetHashCode() => HashCode.Combine(color, name);

        int IComparable.CompareTo(object obj) => name.CompareTo(((UserColor)obj).name);
    }
}
