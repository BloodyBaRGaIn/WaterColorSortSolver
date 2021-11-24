
using System;
using System.Collections.Generic;
using System.Linq;

namespace WaterColorSort.Classes
{
    internal sealed class Tree : List<Tree>
    {
        internal Move Value;
        internal Tree Parent;
        internal int iteration = 0;

        internal Tree(Move _value)
        {
            Value = _value;
        }

        internal Tree(Move _value, Tree Parent) : this(_value)
        {
            this.Parent = Parent;
            iteration = Parent.iteration + 1;
        }

        internal Tree() : this(default) { }

        internal Tree Root()
        {
            Tree root = this;
            while (root.Parent?.Parent != null)
            {
                root = root.Parent;
            }
            return root;
        }

        internal int TotalCount()
        {
            int count = Count;
            foreach (Tree tree in this)
            {
                count += tree.TotalCount();
            }
            return count;
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

        internal void FindSolution()
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

        internal void FillMoves(List<Move> moves)
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
