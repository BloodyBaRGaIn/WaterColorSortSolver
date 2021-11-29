
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WaterColorSort.Classes;

namespace WaterColorSort
{
    internal static class Program
    {
        private const int Offset = 20;

        private static void Main()
        {
            #region Init
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            List<Bottle> Bottles = new();
            List<PixelData> pixelDatas = new();
            List<List<PixelData>> bottle_pixel_list = new();
            List<int> y_layers = new();
            List<Tree> trees = new();
            List<Move> final = new();
            Rectangle bounds = new(Point.Empty, new Size(BitmapWork.W, BitmapWork.H));
            UserColor.InitDict();
            ProcessWork.KillADB();
            #endregion
            while (true)
            {
                Console.Clear();
                Console.WriteLine("START");
                ProcessWork.StartApp().Wait();
                Console.WriteLine("APP STARTED");
                #region Clear
                Bottle.CURR_SIZE = Bottle.MIN_SIZE;
                Bottles.Clear();
                pixelDatas.Clear();
                bottle_pixel_list.Clear();
                y_layers.Clear();
                trees.Clear();
                final.Clear();
                #endregion

                pixelDatas.AddRange(BitmapWork.GetPixels().Distinct(new PixelComparer()).OrderBy(d => d.y).ThenBy(d => d.x));
                Console.WriteLine("GOT PIXELS");
                if (!PixelData.FillYLayers(y_layers, pixelDatas)
                    || !PixelData.MakeDataSets(y_layers, pixelDatas, bottle_pixel_list, out List<int> del))
                {
                    continue;
                }
                Console.WriteLine("DATA STRUCTURED");
                BitmapWork.SaveColorImage(bottle_pixel_list, bounds, new Size(1, 1) * PixelData.PixelSize);
                bottle_pixel_list.RemoveAll(p => p.Count <= 10);
                Bottle.Solution_Found = false;
            fill:
                if (!Bottle.FillBottles(Bottles, bottle_pixel_list))
                {
                    continue;
                }
                if (Bottle.FilledCorrectly(Bottles))
                {
                    Console.WriteLine("\nINPUT\n");
                    Bottle.PrintColoredBottles(Bottles, del);
                    Bottle.Solve(Bottles, trees);
                }

                if (trees.Count == 0)
                {
                    if (Bottle.CURR_SIZE < Bottle.MAX_SIZE)
                    {
                        Bottle.CURR_SIZE++;
                    }
                    else
                    {
                        continue;
                    }
                    goto fill;
                }

                Console.WriteLine($"SOLVED FOR BOTTLES CAPACITY OF {Bottle.CURR_SIZE}");

                if (!Tree.TraceSolution(trees, final))
                {
                    continue;
                }

                Console.WriteLine("MINIMAL SOLUTION TRACED");

                Move.ClearMoves(final);

                Console.WriteLine($"\nTOTAL MOVES COUNT: {final.Count}");

                int done = Bottle.ApplyMoves(Bottles, final);
                bool failed = false;
                if (done != final.Count)
                {
                    final = final.Take(done).ToList();
                    failed = true;
                }

                Console.WriteLine(failed ? $"{final.Count}/{done} APPLIED" : "APPLIED SUCCESSFULLY");

                Console.WriteLine("\nRESULT\n");
                Bottle.PrintColoredBottles(Bottles, del);
                del.Clear();

                Console.WriteLine("\nMOVES\n");
                Move.PerformMoves(bottle_pixel_list, final, Offset);
                if (!Bottles.All(b => b.IsCompleted) || failed)
                {
                    Console.WriteLine("\nFAILED\n");
                    continue;
                }
                Console.WriteLine($"\n{final.Count} MOVES PERFORMED\n");
                Thread.Sleep(1000);

                Move.GotoNext();

                continue;
            }
        }
    }
}
