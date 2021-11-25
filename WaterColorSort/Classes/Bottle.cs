
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    internal sealed class Bottle : Stack<UserColor>
    {
        internal const int MAX_SIZE = 10;
        internal const int MIN_SIZE = 2;
        internal static int CURR_SIZE = 2;

        private const int PixelGroupMinSize = 20;
        internal static bool Solution_Found = false;

        internal Bottle(params Color[] colors)
        {
            foreach (UserColor c in colors)
            {
                Push(c);
            }
        }

        internal Bottle(UserColor[] colors)
        {
            foreach (UserColor c in colors)
            {
                Push(c);
            }
        }

        internal bool IsCompleted => Count == 0 || (this.Distinct().Count() == 1 && Count == CURR_SIZE);

        internal static bool TransferColors(Bottle from, Bottle to)
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

        internal static bool IsPossibleTransfer(List<Bottle> bottles, int from, int to) => IsPossibleTransfer(bottles[from], bottles[to]);

        internal static bool IsPossibleTransfer(Bottle from, Bottle to) => (from.Distinct().Count() > 1 || to.Count != 0)
                                                                           && !from.Equals(to)
                                                                           && from.Count > 0
                                                                           && to.Count < CURR_SIZE
                                                                           && (to.Count == 0 || (from.Peek() == to.Peek()));

        internal static IEnumerable<Move> GetMoves(List<Bottle> bottles)
        {
            static IEnumerable<Move> NonOpt(List<Bottle> bottles)
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
            return NonOpt(bottles).OrderByDescending(b => bottles[b.to].Count)
                                  .ThenBy(b => bottles[b.to].Distinct().Count());
        }

        internal static int ApplyMoves(List<Bottle> bottles, List<Move> moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                Move move = moves[i];
                if (!TransferColors(bottles[move.from], bottles[move.to]))
                {
                    return i;
                }
            }
            return moves.Count;
        }

        internal static void MakeMove(List<Bottle> bottles, Tree prev, Move move)
        {
            if (Solution_Found || prev.iteration >= (CURR_SIZE + 1) * (bottles.Count + 1))
            {
                return;
            }
            List<Bottle> new_bottles = new();
            foreach (Bottle b in bottles)
            {
                new_bottles.Add(new(b.Reverse().ToArray()));
            }
            if (!TransferColors(new_bottles[move.from], new_bottles[move.to]))
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

        internal static void PrintBottles(List<Bottle> Bottles)
        {
            for (int i = 0; i < Bottles.Count; i++)
            {
                Console.WriteLine($"{i} => {Bottles[i]}");
            }
        }

        internal static bool FillBottles(List<Bottle> bottles, Color empty, List<List<PixelData>> bottle_pixel_list)
        {
            bottles.Clear();
            foreach (List<PixelData> b in bottle_pixel_list)
            {
                if (!b.Any(d => d.c != empty))
                {
                    bottles.Add(new());
                    continue;
                }
                int min_y = b.Min(d => d.y);
                int max_y = b.Where(d => d.c != empty).Max(d => d.y);
                if (Math.Abs(max_y - b.Where(d => d.c == empty).Max(d => d.y)) > 20)
                {
                    min_y = b.Where(d => d.c == empty).Max(d => d.y);
                    b.RemoveAll(d => Math.Abs(min_y - d.y) > 20 && d.c == empty);
                }
                int segment_len = (int)(((max_y - min_y) / (CURR_SIZE + 0.5f)) + 1);
                int segment_len_4 = (int)(((max_y - min_y) / 4.5f) + 1);
                min_y += segment_len_4 / 2;
                Bottle new_b = new();
                for (int seg = CURR_SIZE - 1; seg >= 0; seg--)
                {
                    int y_lim_min = min_y + (seg * segment_len);
                    IEnumerable<IGrouping<UserColor, PixelData>> groups = b.Where(d => d.y >= y_lim_min && d.y < y_lim_min + segment_len)
                                                                           .GroupBy(d => d.c)
                                                                           .OrderByDescending(gr => gr.Count());
                    if (groups.Any() && groups.First().Count() > PixelGroupMinSize)
                    {
                        UserColor color = groups.First().Key;
                        if (color == empty)
                        {
                            return false;
                        }
                        new_b.Push(color);
                    }
                }
                bottles.Add(new_b);
            }
            return true;
        }


        internal static void PrintColoredBottles(List<Bottle> Bottles, List<int> del)
        {
            if (del[^1] != Bottles.Count)
            {
                del.Add(Bottles.Count);
            }
            for (int idx = 0; idx < del.Count - 1; idx++)
            {
                for (int i = 0; i < CURR_SIZE; i++)
                {
                    if (idx > 0 && del[idx + 1] - del[idx] != del[idx] - del[idx - 1])
                    {
                        Console.Write("  ");
                    }
                    for (int b = del[idx]; b < del[idx + 1]; b++)
                    {
                        Console.Write('|');
                        Console.ForegroundColor = Bottles[b].ElementAtOrDefault(i + (Bottles[b].Count - CURR_SIZE)).GetNearestColor();
                        Console.Write("##");
                        Console.ResetColor();
                        Console.Write('|');
                        Console.Write(' ');
                    }
                    Console.WriteLine();
                }
                if (idx > 0 && del[idx + 1] - del[idx] != del[idx] - del[idx - 1])
                {
                    Console.Write("  ");
                }
                for (int b = del[idx]; b < del[idx + 1]; b++)
                {
                    Console.Write(b.ToString().PadLeft(3).PadRight(5));
                }
                Console.WriteLine();
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
