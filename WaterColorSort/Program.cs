using AutoItX3Lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using WaterColorSort.Classes;

namespace WaterColorSort
{
    internal static class Program
    {
        private const int Offset = 10;
        
        private static int Main(string[] args)
        {
            #region Init
            List<Bottle> Bottles = new();
            List<PixelData> pixelDatas = new();
            List<List<PixelData>> bottle_pixel_list = new();
            List<int> y_layers = new();
            List<Tree> trees = new();
            List<Move> final = new();
            AutoItX3 autoItX3 = new();
            #endregion

            string android = ProcessWork.GetProcess();
            if (string.IsNullOrWhiteSpace(android))
            {
                return 1;
            }
            _ = autoItX3.MouseMove(0, 0, 0);
            autoItX3.WinActivate(android);
        _main:
            Bottle.Solution_Found = false;
            Bottles.Clear();
            pixelDatas.Clear();
            bottle_pixel_list.Clear();
            y_layers.Clear();
            trees.Clear();
            final.Clear();
            Thread.Sleep(1000);

            Rectangle bounds = ProcessWork.DetectWindow(autoItX3, android, 10, 150, -10, -300);
            pixelDatas.AddRange(BitmapWork.GetPixels(bounds).OrderBy(d => d.y).ThenBy(d => d.x));
            PixelData.DataReduction(pixelDatas);
            if (!PixelData.FillYLayers(y_layers, pixelDatas)
                || !PixelData.MakeDataSets(y_layers, pixelDatas, bottle_pixel_list))
            {
                goto _main;
            }
            BitmapWork.SaveColorImage(bottle_pixel_list, bounds, new Size(1, 1) * PixelData.PixelSize);
            Console.Clear();
            if (!Bottle.FillBottles(Bottles, BitmapWork.empty, bottle_pixel_list))
            {
                goto _main;
            }

            Console.WriteLine("\nINPUT\n");
            Bottle.PrintBottles(Bottles);
            Tree temp;
            foreach (Move move in Bottle.GetMoves(Bottles))
            {
                temp = new();
                Bottle.MakeMove(Bottles, temp, move);
                temp.ClearTree();
                if (Bottle.Solution_Found)
                {
                    Bottle.Solution_Found = false;
                    trees.Add(temp);
                    continue;
                }
                temp.Clear();
            }

            if (trees.Count == 0)
            {
                goto _main;
            }
            temp = trees.OrderBy(t => t.Root().TotalCount()).FirstOrDefault();
            while (!temp.Any(t => t.Value.Win))
            {
                temp.FindSolution();
            }
            temp.FillMoves(final);
            
            if (final.Count == 0)
            {
                goto _main;
            }

            Move.ClearMoves(final);

            Console.WriteLine($"\nTOTAL MOVES COUNT: {final.Count}");

            int done = Bottle.ApplyMoves(Bottles, final);
            bool failed = false;
            if (done != final.Count)
            {
                final = final.Take(done).ToList();
                failed = true;
            }

            Console.WriteLine(failed ? $"\n{final.Count}/{done} APPLIED" : "\nAPPLIED SUCCESSFULLY");

            Console.WriteLine("\nRESULT\n");
            Bottle.PrintBottles(Bottles);

            Console.WriteLine("\nMOVES\n");
            Move.PerformMoves(bottle_pixel_list, final, bounds, autoItX3, Offset);
            if (!Bottles.All(b => b.IsCompleted) || failed)
            {
                Console.WriteLine("\nFAILED\n");
                goto _main;
            }
            Console.WriteLine($"\n{final.Count} MOVES PERFORMED\n");
            Thread.Sleep(1500);

            Move.GotoNext(bounds, autoItX3);
            Console.WriteLine("DONE");
            goto _main;
        }
    }
}
