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
            while (true)
            {
                Console.Clear();
                Console.WriteLine("START");
                #region Clear
                Bottle.Solution_Found = false;
                Bottles.Clear();
                pixelDatas.Clear();
                bottle_pixel_list.Clear();
                y_layers.Clear();
                trees.Clear();
                final.Clear();
                #endregion
                Thread.Sleep(1500); /* new level await */
                //string android = ProcessWork.GetProcess();
                //if (string.IsNullOrWhiteSpace(android))
                //{
                    //return 1;
                //}
                //autoItX3.WinActivate(android);
                //Rectangle bounds = ProcessWork.DetectWindow(autoItX3, android, 10, 150, -10, -300);
                Rectangle bounds = new(Point.Empty, new Size(720, 800));
                if (bounds.Height < 0 || bounds.Width < 0) 
                {
                    return 1;
                }
                pixelDatas.AddRange(BitmapWork.GetPixels(bounds).OrderBy(d => d.y).ThenBy(d => d.x));
                Console.WriteLine("GOT PIXELS\n");
                PixelData.DataReduction(pixelDatas);
                Console.WriteLine("DATA REDUCED\n");
                if (!PixelData.FillYLayers(y_layers, pixelDatas)
                    || !PixelData.MakeDataSets(y_layers, pixelDatas, bottle_pixel_list))
                {
                    continue;
                }
                Console.WriteLine("DATA STRUCTURED\n");
                BitmapWork.SaveColorImage(bottle_pixel_list, bounds, new Size(1, 1) * PixelData.PixelSize);
                bottle_pixel_list.RemoveAll(p => p.Count < 5);
            fill:
                if (!Bottle.FillBottles(Bottles, BitmapWork.empty, bottle_pixel_list))
                {
                    continue;
                }
                Console.WriteLine($"TRYING TO SOLVE WITH CAPACITY OF {Bottle.CURR_SIZE}\n");
                bool wrong_size = false;
                IEnumerable<UserColor> bottle_content = Bottles.SelectMany(b => b);
                foreach (UserColor color in bottle_content)
                {
                    if (bottle_content.Count(b => b.Equals(color)) != Bottle.CURR_SIZE)
                    {
                        wrong_size = true;
                        break;
                    }
                }
                Tree temp;
                if (!wrong_size)
                {
                    Console.WriteLine("\nINPUT\n");
                    Bottle.PrintBottles(Bottles);
                    foreach (Move move in Bottle.GetMoves(Bottles))
                    {
                        temp = new();
                        System.Threading.Tasks.Task SolveTask = System.Threading.Tasks.Task.Run(() =>
                        {
                            Bottle.MakeMove(Bottles, temp, move);
                        });
                        SolveTask.Wait(1000);
                        temp.ClearTree();
                        if (Bottle.Solution_Found)
                        {
                            Bottle.Solution_Found = false;
                            trees.Add(temp);
                            continue;
                        }
                        temp.Clear();
                    }
                }

                if (trees.Count == 0)
                {
                    if (Bottle.CURR_SIZE < Bottle.MAX_SIZE)
                    {
                        Bottle.CURR_SIZE++;
                    }
                    else
                    {
                        Bottle.CURR_SIZE = Bottle.MIN_SIZE;
                    }
                    goto fill;
                }

                Console.WriteLine($"\nSOLVED FOR BOTTLES CAPACITY OF {Bottle.CURR_SIZE}\n");

                temp = trees.OrderBy(t => t.Root().TotalCount()).FirstOrDefault();
                while (!temp.Any(t => t.Value.Win))
                {
                    temp.FindSolution();
                }
                temp.FillMoves(final);

                if (final.Count == 0)
                {
                    continue;
                }

                //Move.ClearMoves(final);

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
                    continue;
                }
                Console.WriteLine($"\n{final.Count} MOVES PERFORMED\n");
                Thread.Sleep(1500);

                Move.GotoNext(bounds, autoItX3);
                Console.WriteLine("DONE");

                Bottle.CURR_SIZE = Bottle.MIN_SIZE;
                continue;
            }
        }
    }
}
