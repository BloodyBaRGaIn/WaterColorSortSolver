using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    internal sealed class Tree : List<Tree>
    {
        internal Move Value;
        internal Tree Parent;
        internal int iteration = 0;

        private Tree(Move _value)
        {
            Value = _value;
        }

        internal Tree(Move _value, Tree Parent) : this(_value)
        {
            this.Parent = Parent;
            iteration = Parent.iteration + 1;
        }

        internal Tree() : this(default) { }

        private Tree Root()
        {
            Tree root = this;
            while (root.Parent?.Parent != null)
            {
                root = root.Parent;
            }
            return root;
        }

        private int TotalCount()
        {
            int count = Count;
            foreach (Tree tree in this)
            {
                count += tree.TotalCount();
            }
            return count;
        }

        internal static bool TraceSolution(List<Tree> trees, List<Move> final)
        {
            Bottle.Solution_Found = true;
            List<Move> f_list = new();
            final.Clear();
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
            return final.Count > 0;
        }

        public new void Clear()
        {
            foreach (Tree tree in this)
            {
                tree.Clear();
            }
            base.Clear();
        }

        internal void Print()
        {
            if (Value.Win)
            {
                Console.WriteLine(Value);
            }
            foreach (Tree tree in this)
            {
                tree.Print();
            }
        }

        private void FindSolution()
        {
            foreach (Tree tree in this)
            {
                if (tree.Any(t => t.Value.Win))
                {
                    tree.Value.Win = true;
                }
                tree.FindSolution();
            }
        }

        private void FillMoves(List<Move> moves)
        {
            if (Value.Win)
            {
                moves.Add(Value);
            }
            foreach (Tree tree in this)
            {
                tree.FillMoves(moves);
            }
        }

        internal void ClearTree()
        {
            for (int i = 0; i < Count; i++)
            {
                Tree tree = this[i];
                if (tree.Count == 0 && !tree.Value.Win)
                {
                    Remove(tree);
                    i--;
                }
                else
                {
                    tree.ClearTree();
                }
            }
        }
    }
}
