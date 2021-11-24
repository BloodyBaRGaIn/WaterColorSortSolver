
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    internal struct Move
    {
        internal readonly int from, to;
        internal readonly UserColor color;
        internal bool Win;

        internal Move(int from, int to, UserColor color)
        {
            this.from = from;
            this.to = to;
            this.color = color;
            Win = false;
        }

        internal Move Opposite => new(to, from, color);

        internal static void ClearMoves(List<Move> moves)
        {
            for (int i = 0; i < moves.Count - 1; i++)
            {
                if (moves[i].Equals(moves[i + 1].Opposite))
                {
                    moves.RemoveAt(i--);
                }
            }
        }

        internal static void PerformMoves(List<List<PixelData>> bottle_pixel_list, IEnumerable<Move> final, int offset)
        {
            List<(int x, int y)> coords = bottle_pixel_list.Select(p => ((int)p.Average(p => p.x) + BitmapWork.X, (int)p.Average(p => p.y) + offset + BitmapWork.Y)).ToList();
            foreach (Move move in final)
            {
                ProcessWork.Click(new List<(int, int)>(3) { coords[move.from], coords[move.to], coords[move.to] }).Wait();
                System.Console.WriteLine(move);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static void GotoNext()
        {
            using Bitmap image = BitmapWork.GetBitmap();
            using Bitmap tofind = new(Image.FromFile("InterfaceResources/complete.png"));
            List<Point> Pts = BitmapWork.FindBitmapsEntry(image, tofind, 10);
            if (Pts.Count > 0)
            {
                ProcessWork.Click((int)Pts.Average(p => p.X) + BitmapWork.X, (int)Pts.Average(p => p.Y) + BitmapWork.Y).Wait();
            }
            image.Dispose();
            System.GC.Collect();
        }

        public override bool Equals(object obj) => obj is Move move
                                                   && move.from == from
                                                   && move.to == to
                                                   && move.color.Equals(color);

        public override int GetHashCode() => System.HashCode.Combine(from, to, color, Win);

        public override string ToString() => $"{from} -> {to} ({color.name})";
    }
}
