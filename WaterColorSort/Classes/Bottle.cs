using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    internal sealed class Bottle : Stack<UserColor>
    {
        private const int PixelGroupMinSize = 20;

        internal const int MAX_SIZE = 10;
        internal const int MIN_SIZE = 4;
        internal static int CURR_SIZE = 4;

        internal static bool Solution_Found = false;

        private delegate void Print(int idx, params object[] param);

        private Bottle(params Color[] colors) => PushColors(from Color c in colors select (UserColor)c);

        private Bottle(in UserColor[] colors) => PushColors(colors);

        private void PushColors(IEnumerable<UserColor> colors)
        {
            foreach (UserColor c in colors)
            {
                Push(c);
            }
        }

        internal bool IsCompleted => Count == 0 || (this.Distinct().Count() == 1 && Count == CURR_SIZE);

        private static readonly Print Body = new((b, param) =>
        {
            Bottle bot = (param[0] as List<Bottle>)[b];
            int idx = (int)param[1] + bot.Count - CURR_SIZE - 1;
            Console.Write('\u2502');
            Console.ForegroundColor = (idx >= 0 && idx < bot.Count ? bot.ElementAt(idx) : default).GetColorByName();
            Console.Write("\u2588\u2588");
            Console.ResetColor();
            Console.Write("\u2502 ");
        });

        private static readonly Print Base = new((_, _) => Console.Write("\u2514\u2500\u2500\u2518 "));

        private static readonly Print Num = new((b, param) => Console.Write($"{((param[0] as List<Bottle>)[b].IsCompleted ? "#" : "")}{b}".PadLeft(3).PadRight(5)));

        private static bool TransferColors(in Bottle from, in Bottle to)
        {
            if (!IsPossibleTransfer(from, to))
            {
                return false;
            }
            UserColor transf = from.Peek();
            while (from.Count > 0 && to.Count < CURR_SIZE)
            {
                if (from.Peek() != transf)
                {
                    break;
                }
                _ = from.Pop();
                to.Push(transf);
            }
            if (from.TryPeek(out transf))
            {
                if (to.Peek() == transf)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsPossibleTransfer(in List<Bottle> bottles, in int from, in int to) => IsPossibleTransfer(bottles[from], bottles[to]);

        private static bool IsPossibleTransfer(in Bottle from, in Bottle to) => (from.Distinct().Count() > 1 || to.Count != 0)
                                                                                 && !from.Equals(to)
                                                                                 && from.Count > 0
                                                                                 && to.Count < CURR_SIZE
                                                                                 && (to.Count == 0 || (from.Peek() == to.Peek()));

        private static IEnumerable<Move> GetMoves(List<Bottle> bottles) => NonOpt(bottles).OrderByDescending(b => bottles[b.to].Count)
                                                                                          .ThenBy(b => bottles[b.to].Distinct().Count());

        private static IEnumerable<Move> NonOpt(List<Bottle> bottles)
        {
            for (int i = 0; i < bottles.Count; i++)
            {
                for (int j = 0; j < bottles.Count; j++)
                {
                    if (IsPossibleTransfer(bottles, i, j))
                    {
                        yield return new Move(i, j, bottles[i].Peek());
                    }
                }
            }
        }

        private static void MakeMove(in List<Bottle> bottles, in Tree prev, in Move move)
        {
            if (Solution_Found || prev.iteration >= (CURR_SIZE + 1) * (bottles.Count + 1))
            {
                return;
            }
            List<Bottle> new_bottles = CopyBottles(bottles);
            if (!TransferColors(new_bottles, move))
            {
                return;
            }
            Tree child = new(move, prev);
            prev.Add(child);
            if (new_bottles.All(b => b.IsCompleted))
            {
                child.Value.Win = true;
                Solution_Found = true;
                return;
            }
            IEnumerable<Move> new_moves = GetMoves(new_bottles);
            if (!new_moves.Any())
            {
                _ = prev.Remove(child);
            }
            foreach (Move new_move in new_moves)
            {
                if (!prev.Select(t => t.Value).Contains(new_move.Opposite))
                {
                    child.Add(new(new_move, child));
                }
                MakeMove(new_bottles, child, new_move);
            }
        }

        private static bool FillBottles(in List<Bottle> bottles, in List<List<PixelData>> bottle_pixel_list)
        {
            bottles.Clear();
            int segment_len = -1;
            foreach (List<PixelData> b in bottle_pixel_list)
            {
                if (!b.Any(d => d.userColor != BitmapWork.empty))
                {
                    bottles.Add(new());
                    continue;
                }
                int min_y = 0;
                var gr_empty = b.Where(d => d.userColor == BitmapWork.empty).GroupBy(PixelData.DataY).Where(g => g.Count() >= PixelGroupMinSize);
                if (gr_empty.Any())
                {
                    min_y = gr_empty.OrderBy(g => g.Key).First().Key;
                }
                else
                {
                    min_y = b.Where(d => d.userColor == BitmapWork.empty).Min(PixelData.DataY);
                }
                int max_y = b.Where(d => d.userColor != BitmapWork.empty).Max(PixelData.DataY);
                min_y += (max_y - min_y) / 8;
                if (segment_len == -1) segment_len = ((max_y - min_y) / CURR_SIZE) + 1;
                Bottle new_b = new();
                for (int seg = 1; seg <= CURR_SIZE; seg++)
                {
                    int y_lim_min = max_y - (seg * segment_len);
                    IEnumerable<IGrouping<UserColor, PixelData>> groups = b.Where(d => d.y >= y_lim_min && d.y < y_lim_min + segment_len)
                                                                           .GroupBy(d => d.userColor)
                                                                           .OrderByDescending(gr => gr.Count());
                    if (groups.Any() && groups.First().Count() > PixelGroupMinSize)
                    {
                        UserColor color = groups.First().Key;
                        if (color == BitmapWork.empty)
                        {
                            //return false;
                        }
                        else new_b.Push(color);
                    }
                }
                bottles.Add(new_b);
            }
            return true;
        }

        private static bool FilledCorrectly(in IEnumerable<Bottle> Bottles)
        {
            IEnumerable<UserColor> bottle_content = Bottles.SelectMany(b => b);
            foreach (UserColor color in bottle_content.Distinct())
            {
                if (bottle_content.Count(b => b.Equals(color)) != CURR_SIZE)
                {
                    return false;
                }
            }
            return true;
        }

        private static void Solve(List<Bottle> Bottles, in List<Tree> trees)
        {
            Tree temp;
            foreach (Move move in GetMoves(Bottles))
            {
                temp = new();
                using (Task SolveTask = Task.Run(() => MakeMove(Bottles, temp, move)))
                {
                    if (SolveTask.Wait(5000) && Solution_Found)
                    {
                        temp.ClearTree();
                        Solution_Found = false;
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
                        Solution_Found = true;
                        SolveTask.Wait();
                        SolveTask.Dispose();
                        Solution_Found = false;
                    }
                }
                GC.Collect();
            }
        }

        private static void UniversalPrint(in int idx, in List<int> del, in Print print, params object[] param)
        {
            if (idx > 0 && del[idx + 1] - del[idx] != del[idx] - del[idx - 1])
            {
                Console.Write("  ");
            }
            for (int b = del[idx]; b < del[idx + 1]; b++)
            {
                print?.Invoke(b, param);
            }
            Console.WriteLine();
        }

        internal static List<Bottle> CopyBottles(in List<Bottle> bottles) => bottles.Select(b => new Bottle(b.Reverse().ToArray())).ToList();

        internal static bool TransferColors(List<Bottle> bottles, Move move) => TransferColors(bottles[move.from], bottles[move.to]);

        internal static bool FillAndSolve(List<Bottle> Bottles, List<List<PixelData>> bottle_pixel_list, List<Tree> trees, List<int> del)
        {
            while (true)
            {
                if (!FillBottles(Bottles, bottle_pixel_list))
                {
                    return false;
                }
                if (FilledCorrectly(Bottles))
                {
                    Console.Clear();
                    PrintColoredBottles(Bottles, del);
                    Console.WriteLine("SOLVING...");
                    Solve(Bottles, trees);
                }
                if (trees.Count == 0)
                {
                    if (CURR_SIZE < MAX_SIZE)
                    {
                        CURR_SIZE++;
                        continue;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    break;
                }
            }
            return true;
        }

        internal static int ApplyMoves(in List<Bottle> bottles, in List<Move> moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                if (!TransferColors(bottles, move))
                {
                    return i;
                }
            }
            return moves.Count;
        }

        internal static void PrintBottles(in List<Bottle> Bottles)
        {
            for (int i = 0; i < Bottles.Count; i++)
            {
                Console.WriteLine($"{i} => {Bottles[i]}");
            }
        }

        internal static void PrintColoredBottles(in List<Bottle> Bottles, in List<int> del)
        {
            Console.WriteLine();
            if (del[^1] != Bottles.Count)
            {
                del.Add(Bottles.Count);
            }
            for (int idx = 0; idx < del.Count - 1; idx++)
            {
                for (int i = 0; i < CURR_SIZE + 1; i++)
                {
                    UniversalPrint(idx, del, Body, Bottles, i);
                }
                UniversalPrint(idx, del, Base);
                UniversalPrint(idx, del, Num, Bottles);
                Console.WriteLine();
            }
        }

        public new void Push(UserColor item)
        {
            if (Count >= CURR_SIZE)
            {
                throw new Exception("Overflow");
            }
            base.Push(item);
        }

        public override bool Equals(object obj) => obj is Bottle bottle && base.Equals(bottle);

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            string res = "(";
            foreach (UserColor c in this)
            {
                res += $"{c.name},";
            }
            return $"{res[Count > 0 ? ..^1 : ..]})";
        }
    }
}
