using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using IronOcr;

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

        private void PrintColored()
        {
            Console.Write($"{from} \u2192 {to} (");
            Console.ForegroundColor = color.GetColorByName();
            Console.Write('\u25A0');
            Console.ResetColor();
            Console.WriteLine(')');
        }

        internal static bool PerformMoves(List<List<PixelData>> bottle_pixel_list, List<Bottle> bottles, List<int> del, IEnumerable<Move> final, int offset)
        {
            List<(int x, int y)> coords = bottle_pixel_list.Select(p => ((int)p.Average(p => p.x) + BitmapWork.X, (int)p.Average(p => p.y) + offset + BitmapWork.Y)).ToList();
            foreach (Move move in final)
            {
                Console.Clear();
                Bottle.PrintColoredBottles(bottles, del);
                move.PrintColored();
                Bottle.TransferColors(bottles, move);
                _ = ProcessWork.Click(new List<((int, int), double)>(3) { (coords[move.from], 0.1), (coords[move.to], 0.1), (coords[move.to], 0) }).Wait(200);
            }
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static void GotoNext()
        {
            #region find next button

            bool find = false;
            Point click = Point.Empty;
            Bitmap image = null;
            for (int i = 0; i < 10; i++)
            {
                IronTesseract ocr = new();
                image?.Dispose();
                image = BitmapWork.GetBitmap();
                using (OcrInput Input = new(image))
                {
                    Input.DeNoise();
                    Input.ToGrayScale();
                    Input.Invert();
                    Input.ReplaceColor(Color.Gray, Color.White, 100);

                    OcrResult Result;
                    _ = Input.SaveAsImages();

                    try
                    {
                        Result = ocr.Read(Input);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (OcrResult.Word word in Result.Words)
                    {
                        if (word.Text.ToUpper().Contains("NEXT"))
                        {
                            find = true;
                            click = word.Location.Location;
                            break;
                        }
                    }
                }
                GC.Collect();
                if (find)
                {
                    ProcessWork.Click(click.X + BitmapWork.X, click.Y + BitmapWork.Y, 0).Wait();
                    return;
                }
            }

            #endregion

            #region find X button

            List<Point> points = null;
            while (true)
            {
                GC.Collect();
                using Bitmap bitmap = InterfaceResources.Interface.X;
                image?.Dispose();
                image = BitmapWork.GetBitmap();
                points = BitmapWork.FindBitmapsEntry(image, bitmap, 50);
                if (points.Count == 0)
                {
                    continue;
                }
                ProcessWork.Click((int)points.Average(p => p.X) + BitmapWork.X + bitmap.Width / 2, (int)points.Average(p => p.Y) + BitmapWork.Y + bitmap.Height / 2, 0).Wait();
                return;
            }

            #endregion
            
        }

        public override bool Equals(object obj) => obj is Move move
                                                   && move.from == from
                                                   && move.to == to
                                                   && move.color.Equals(color);

        public override int GetHashCode() => HashCode.Combine(from, to, color, Win);

        public override string ToString() => $"{from} -> {to} ({color.name})";
    }
}
