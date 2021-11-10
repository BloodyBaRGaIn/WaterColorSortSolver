using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    internal sealed class Bottle : Stack<Color>
    {
        internal static bool f = false;
        internal Bottle(params Color[] colors)
        {
            foreach (Color c in colors)
            {
                Push(c);
            }
        }
        internal bool IsCompleted => Count == 0 || (this.Distinct().Count() == 1 && Count == 4);
        internal static bool TransferColors(Bottle from, Bottle to)
        {
            if (!IsPossibleTransfer(from, to))
            {
                return false;
            }
            Color transf = from.Peek();
            while (from.Count > 0 && to.Count < 4)
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
        internal static bool IsPossibleTransfer(Bottle from, Bottle to)
        {
            return (from.Distinct().Count() > 1 || to.Count != 0)
                   && !from.Equals(to)
                   && from.Count > 0
                   && to.Count < 4
                   && (to.Count == 0 || (from.Peek() == to.Peek()));
        }
        internal static IEnumerable<Move> GetMoves(List<Bottle> bottles)
        {
            foreach (Bottle from in bottles)
            {
                foreach (Bottle to in bottles)
                {
                    if (IsPossibleTransfer(from, to))
                    {
                        yield return new Move(bottles.IndexOf(from), bottles.IndexOf(to), from.Peek());
                    }
                }
            }
        }
        internal static System.Numerics.BigInteger GetState(List<Bottle> bottles)
        {
            System.Numerics.BigInteger xor = 0;
            foreach (Bottle b in bottles)
            {
                xor += HashCode.Combine(b.Count, b.TryPeek(out Color c), c);
            }
            return xor;
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
        internal static void MakeMove(List<Bottle> bottles, List<Move> prev, Move move)
        {
            if (f) return;
            List<Bottle> new_bottles = new();
            foreach (Bottle b in bottles)
            {
                new_bottles.Add(new(b.Reverse().ToArray()));
                if (!new_bottles.Last().SequenceEqual(b))
                {
                    throw new Exception("");
                }
            }
            if (!TransferColors(new_bottles[move.from], new_bottles[move.to]))
            {
                return;
            }
            move.gen_prev = GetState(bottles);
            move.gen_next = GetState(new_bottles);
            prev.Add(move);
            if (new_bottles.All(b => b.IsCompleted))
            {
                Console.WriteLine("Found");
                f = true;
                return;
            }
            foreach (Move new_move in GetMoves(new_bottles).OrderByDescending(b => new_bottles[b.to].Count))
            {
                MakeMove(new_bottles, prev, new_move);
            }
        }
        public new void Push(Color item)
        {
            if (Count >= 4)
            {
                throw new Exception("Overflow");
            }
            base.Push(item);
        }
        public override bool Equals(object obj)
        {
            return obj is Bottle bottle && base.Equals(bottle);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            string res = "(";
            foreach (Color c in this)
            {
                res += $"{c.Name},";
            }
            return $"{res[Count > 0 ? ..^1 : ..]})";
        }
    }
}
