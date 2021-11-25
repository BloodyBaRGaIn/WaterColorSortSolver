
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
            List<Bottle> Bottles = new();
            List<PixelData> pixelDatas = new();
            List<List<PixelData>> bottle_pixel_list = new();
            List<int> y_layers = new();
            List<Tree> trees = new();
            List<Move> final = new();
            Rectangle bounds = new(Point.Empty, new Size(BitmapWork.W, BitmapWork.H));
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
                if (!Bottle.FillBottles(Bottles, BitmapWork.empty, bottle_pixel_list))
                {
                    continue;
                }
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
                    //Bottle.PrintBottles(Bottles);
                    //Console.WriteLine();
                    Bottle.PrintColoredBottles(Bottles, del);
                    foreach (Move move in Bottle.GetMoves(Bottles))
                    {
                        temp = new();
                        using (Task SolveTask = Task.Run(() => Bottle.MakeMove(Bottles, temp, move)))
                        {
                            if (SolveTask.Wait(2000) && Bottle.Solution_Found)
                            {
                                temp.ClearTree();
                                Bottle.Solution_Found = false;
                                if (temp.Count > 0)
                                {
                                    trees.Add(temp);
                                }
                            }
                            try
                            {
                                SolveTask.Dispose();
                            }
                            catch
                            {
                                Bottle.Solution_Found = true;
                                SolveTask.Wait();
                                SolveTask.Dispose();
                                Bottle.Solution_Found = false;
                            }
                        }
                        GC.Collect();
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
                        continue;
                    }
                    goto fill;
                }

                Console.WriteLine($"SOLVED FOR BOTTLES CAPACITY OF {Bottle.CURR_SIZE}");
                Bottle.Solution_Found = true;
                List<Move> f_list = new();
                foreach (Tree tree in trees.Where(t => t.Root().TotalCount() > 0).OrderBy(t => t.Root().TotalCount()))
                {
                    using (Task SolveTask = Task.Run(() =>
                    {
                        while (!tree.Any(t => t.Value.Win))
                        {
                            tree.FindSolution();
                        }
                    }))
                    {
                        if (SolveTask.Wait(2000))
                        {
                            f_list.Clear();
                            tree.FillMoves(f_list);
                            if (f_list.Count > 0 && (f_list.Count < final.Count || final.Count == 0))
                            {
                                final.Clear();
                                final.AddRange(f_list);
                            }
                        }
                    }
                    GC.Collect();
                }

                if (final.Count == 0)
                {
                    continue;
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

                Console.WriteLine(failed ? $"{final.Count}/{done} APPLIED" : "APPLIED SUCCESSFULLY");

                Console.WriteLine("\nRESULT\n");
                //Bottle.PrintBottles(Bottles);
                //Console.WriteLine();
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
