using IronOcr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
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
            Size offsetSize = new Size(BitmapWork.Location) + new Size(offset, offset);
            List<Point> coordsData = bottle_pixel_list.Select(p => p.Average() + offsetSize).ToList();
            foreach (Move move in final)
            {
                Console.Clear();
                Bottle.PrintColoredBottles(bottles, del);
                move.PrintColored();
                _ = Bottle.TransferColors(bottles, move);

                _ = ProcessWork.Click(new(coordsData[move.from], 0.1), new(coordsData[move.to], 0.1), new(coordsData[move.to], 0)).Wait(250);
            }
            return true;
        }

        internal static void GotoNext()
        {
            const int max_iterations = 10;
            while (true)
            {
                #region find next button

                bool find = false;
                Point click = Point.Empty;
                Bitmap image = null;
                Installation.LoggingMode = Installation.LoggingModes.None;
                Console.WriteLine("Searching for \"NEXT\" button...");
                using (new OutputSink())
                {
                    for (int i = 0; i < max_iterations; i++)
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
                            ProcessWork.Click(new ClickData(click + new Size(BitmapWork.Location), 0)).Wait();
                            return;
                        }
                    }
                }

                #endregion

                #region find X button

                List<Point> points = null;
                for (int i = 0; i < max_iterations; i++)
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
                    ProcessWork.Click(
                        new ClickData(points.Average() + (new Size(BitmapWork.Location) + (bitmap.Size / 2)),
                            0)).Wait();
                    return;
                }

                #endregion
            }

        }

        public override bool Equals(object obj)
        {
            return obj is Move move
              && move.from == from
              && move.to == to
              && move.color.Equals(color);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(from, to, color, Win);
        }

        public override string ToString()
        {
            return $"{from} -> {to} ({color.name})";
        }
    }
}
