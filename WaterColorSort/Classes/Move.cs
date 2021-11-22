using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace WaterColorSort.Classes
{
    internal struct Move
    {
        private const string StrButton = "LEFT";
        private const int Width = 720;
        private const int Height = 1520;
        private const int ClientWidth = 720; //460
        private const int ClientHeight = 1520; //980
        private const int X = 10;
        private const int Y = 335; //150
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

        internal static void ClearMoves(List<Move> final)
        {
            for (int i = 0; i < final.Count - 1; i++)
            {
                if (final[i].Equals(final[i + 1].Opposite))
                {
                    final.RemoveAt(i--);
                    continue;
                }
                if (final[i].to == final[i + 1].from && final[i].color == final[i + 1].color)
                {
                    Move move = new(final[i].from, final[i + 1].to, final[i].color);
                    final.RemoveAt(i);
                    final.Insert(i, move);
                }
            }
        }

        internal static void PerformMoves(List<List<PixelData>> bottle_pixel_list, IEnumerable<Move> final,
                                          Rectangle bounds, AutoItX3Lib.AutoItX3 autoItX3, int offset, int speed = 5)
        {
            //_ = autoItX3.MouseMove(bounds.X, bounds.Y, 0);
            foreach (Move move in final)
            {
                PixelData from = bottle_pixel_list[move.from][0];
                PixelData to = bottle_pixel_list[move.to][0];
                //_ = autoItX3.MouseClick(StrButton, bounds.X + from.x + offset, bounds.Y + from.y + (offset * 2), 1, speed);
                //_ = autoItX3.MouseClick(StrButton, bounds.X + to.x + offset, bounds.Y + to.y + (offset * 2), 2, speed);
                ProcessWork.Click((from.x + offset + X) * Width / ClientWidth, (from.y + (offset * 10) + Y) * Height / ClientHeight);
                System.Threading.Thread.Sleep(10);
                ProcessWork.Click((to.x + offset + X) * Width / ClientWidth, (to.y + (offset * 10) + Y) * Height / ClientHeight);
                System.Threading.Thread.Sleep(10);
                ProcessWork.Click((to.x + offset + X) * Width / ClientWidth, (to.y + (offset * 10) + Y) * Height / ClientHeight);
                System.Threading.Thread.Sleep(10);
                System.Console.WriteLine(move);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static void GotoNext(Rectangle bounds, AutoItX3Lib.AutoItX3 autoItX3)
        {
            //using Bitmap image = new(bounds.Width, bounds.Height);
            //using Graphics g = Graphics.FromImage(image);
            //g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            using Bitmap image = ProcessWork.GetImage().Clone(new Rectangle(0, 335, 720, 800), PixelFormat.Format32bppArgb);
            using Bitmap tofind = new(Image.FromFile("InterfaceResources/complete.png"));
            List<Point> Pts = BitmapWork.FindBitmapsEntry(image, tofind, 10);
            if (Pts.Count > 0)
            {
                ProcessWork.Click(((int)Pts.Average(p => p.X) + X) * Width / ClientWidth, ((int)Pts.Average(p => p.Y) + Y) * Height / ClientHeight);
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
