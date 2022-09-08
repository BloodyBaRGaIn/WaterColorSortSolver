using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using WaterColorSort.Classes;

[assembly: System.Reflection.AssemblyVersion("1.0")]
[assembly: System.Resources.NeutralResourcesLanguage("en")]
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
            ProcessWork.KillADB();
            #endregion
            while (true)
            {
                Console.Clear();
#if DEBUG
                Console.WriteLine("START");
#endif
                ProcessWork.StartApp().Wait();
#if DEBUG
                Console.WriteLine("APP STARTED");
#endif
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
                if (pixelDatas.Count == 0)
                {
                    continue;
                }
#if DEBUG
                Console.WriteLine("GOT PIXELS");
#endif
                if (!PixelData.FillYLayers(y_layers, pixelDatas)
                    || !PixelData.MakeDataSets(y_layers, pixelDatas, bottle_pixel_list, out List<int> del))
                {
                    continue;
                }
#if DEBUG
                Console.WriteLine("DATA STRUCTURED");
#endif
                BitmapWork.SaveColorImage(bottle_pixel_list, new(Point.Empty, new Size(BitmapWork.W, BitmapWork.H)), new Size(1, 1) * PixelData.PixelSize);
                bottle_pixel_list.RemoveAll(p => p.Count <= 10);
                Bottle.Solution_Found = false;
                if (!Bottle.FillAndSolve(Bottles, bottle_pixel_list, trees, del))
                {
                    continue;
                }
#if DEBUG
                Console.WriteLine($"SOLVED FOR BOTTLES CAPACITY OF {Bottle.CURR_SIZE}");
#endif
                if (!Tree.TraceSolution(trees, final))
                {
                    continue;
                }
#if DEBUG
                Console.WriteLine("MINIMAL SOLUTION TRACED");
#endif
                Move.ClearMoves(final);
#if DEBUG
                Console.WriteLine($"\nTOTAL MOVES COUNT: {final.Count}");
#endif
                int done = Bottle.ApplyMoves(Bottle.CopyBottles(Bottles), final);
                bool failed = false;
                if (done != final.Count)
                {
                    final = final.Take(done).ToList();
                    failed = true;
                }
#if DEBUG
                Console.WriteLine(failed ? $"{final.Count}/{done} APPLIED" : "APPLIED SUCCESSFULLY");
#endif
                if (!Move.PerformMoves(bottle_pixel_list, Bottles, del, final, Offset))
                {
                    continue;
                }
                
                if (!Bottles.All(b => b.IsCompleted) || failed)
                {
#if DEBUG
                    Console.WriteLine("\nFAILED\n");
#endif
                    continue;
                }

                Task.Delay(500).Wait();
                Console.Clear();
                Bottle.PrintColoredBottles(Bottles, del);
                Console.WriteLine($"{final.Count} MOVES PERFORMED");

                del.Clear();
                Task.Delay(1500).Wait();
                Move.GotoNext();
            }
        }
    }
}
