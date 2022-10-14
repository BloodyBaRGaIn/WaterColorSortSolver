using System;

namespace WaterColorSort.Classes
{
    internal readonly struct ColoredBitmap
    {
        internal readonly NamedBitmap namedBitmap;
        internal readonly ConsoleColor consoleColor;

        internal ColoredBitmap(NamedBitmap namedBitmap, ConsoleColor consoleColor)
        {
            this.namedBitmap = namedBitmap;
            this.consoleColor = consoleColor;
        }
    }
}
