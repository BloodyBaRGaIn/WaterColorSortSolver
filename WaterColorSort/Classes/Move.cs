using System;

namespace WaterColorSort.Classes
{
    internal struct Move
    {
        internal readonly int from, to;
        internal readonly System.Drawing.Color color;

        internal Move(int from, int to, System.Drawing.Color color)
        {
            this.from = from;
            this.to = to;
            this.color = color;
            gen_prev = gen_next = 0;
        }

        internal Move Opposite => new(to, from, color);

        internal System.Numerics.BigInteger gen_prev, gen_next;

        public override bool Equals(object obj)
        {
            return obj is Move move
                   && move.from == from
                   && move.to == to
                   && move.color == color;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(from, to, color);
        }

        public override string ToString()
        {
            return $"{from} -> {to} ({color.Name})";
        }
    }
}
