using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WaterColorSort.Classes;

[assembly: System.Reflection.AssemblyVersion("1.0")]
[assembly: System.Resources.NeutralResourcesLanguage("en")]
namespace WaterColorSort
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class Program
    {
        private const int Offset = 20;

        private static readonly List<Bottle> Bottles = new();

        private static readonly List<PixelData> pixelDatas = new();

        private static readonly List<List<PixelData>> bottle_pixel_list = new();

        private static readonly List<int> y_layers = new();

        private static readonly List<Tree> trees = new();

        private static readonly List<Move> final = new();

        private static void Main()
        {
            #region Init
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            ProcessWork.KillADB();

            #endregion
            while (true)
            {
                Thread.Sleep(1000);

                Console.Clear();

                ConsoleWorkaround.WriteLine("START");

                ProcessWork.StartApp();

                ConsoleWorkaround.WriteLine("APP STARTED");

                ProcessWork.StartLiveConnectionCheck();

                #region Clear
                Bottle.CURR_SIZE = Bottle.MIN_SIZE;
                Bottles.Clear();
                pixelDatas.Clear();
                bottle_pixel_list.Clear();
                y_layers.Clear();
                trees.Clear();
                final.Clear();
                #endregion
                pixelDatas.AddRange(BitmapWork.GetPixels().Distinct(new PixelComparer()).OrderBy(d => d.Y).ThenBy(d => d.X));
                if (pixelDatas.Count == 0)
                {
                    continue;
                }

                ConsoleWorkaround.WriteLine("GOT PIXELS");

                if (!PixelData.FillYLayers(y_layers, pixelDatas)
                    || !PixelData.MakeDataSets(y_layers, pixelDatas, bottle_pixel_list, out List<int> del))
                {
                    continue;
                }

                ConsoleWorkaround.WriteLine("DATA STRUCTURED");

                BitmapWork.SaveColorImage(bottle_pixel_list, new(Point.Empty, BitmapWork.Size), new Size(1, 1) * PixelData.PixelSize);

                bottle_pixel_list.RemoveAll(p => p.Count <= 10);
                Bottle.Solution_Found = false;
                if (!Bottle.FillAndSolve(Bottles, bottle_pixel_list, trees, del))
                {
                    continue;
                }

                ConsoleWorkaround.WriteLine($"SOLVED FOR BOTTLES CAPACITY OF {Bottle.CURR_SIZE}");

                if (!Tree.TraceSolution(trees, final))
                {
                    continue;
                }

                ConsoleWorkaround.WriteLine("MINIMAL SOLUTION TRACED");

                Move.ClearMoves(final);

                ConsoleWorkaround.WriteLine($"\nTOTAL MOVES COUNT: {final.Count}");

                int done = Bottle.ApplyMoves(Bottle.CopyBottles(Bottles), final);
                bool failed = false;
                if (done != final.Count)
                {
                    final.RemoveRange(done, final.Count - done);
                    failed = true;
                }

                ConsoleWorkaround.WriteLine(failed ? $"{final.Count}/{done} APPLIED" : "APPLIED SUCCESSFULLY");

                if (!Move.PerformMoves(bottle_pixel_list, Bottles, del, final, Offset))
                {
                    continue;
                }

                if (!Bottles.All(b => b.IsCompleted) || failed)
                {
                    ConsoleWorkaround.WriteLine("\nFAILED\n");
                    continue;
                }

                Console.Clear();
                Bottle.PrintColoredBottles(Bottles, del);
                Console.WriteLine($"{final.Count} MOVES PERFORMED");

                del.Clear();
                Task.Delay(3000).Wait();
                Move.GotoNext();
            }
        }
    }
}
