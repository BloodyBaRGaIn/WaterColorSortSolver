using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal readonly struct PixelData
    {
        internal const int PixelSize = 5;
        internal readonly int X, Y;
        internal readonly UserColor userColor;

        private static int x_dist = 0;

        internal PixelData(int x, int y, UserColor userColor)
        {
            X = x;
            Y = y;
            this.userColor = userColor;
        }

        internal bool Colorsimilar(PixelData other, int tolerance = 10)
        {
            Color _c = userColor.color;
            return Math.Abs(_c.R - other.userColor.color.R) <= tolerance
                && Math.Abs(_c.G - other.userColor.color.G) <= tolerance
                && Math.Abs(_c.B - other.userColor.color.B) <= tolerance;
        }

        public static implicit operator Point(PixelData data)
        {
            return new(data.X, data.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is PixelData data
              && data.X == X
              && data.Y == Y
              && data.userColor == userColor;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, userColor);
        }

        internal static bool MakeDataSets(List<int> y_layers, List<PixelData> pixelDatas,
                                          List<List<PixelData>> bottle_pixel_list, out List<int> del)
        {
            del = new();
            if (y_layers == null
                || pixelDatas == null
                || bottle_pixel_list == null)
            {
                return false;
            }

            List<int> cols_base = new();
            List<int> cols = new();
            int max_dist = (int)(BitmapWork.Size.Width * 3f / x_dist);
            for (int i = 0; i <= y_layers.Count - 2; i++)
            {
                del.Add(cols.Count + del.ElementAtOrDefault(del.Count - 1) - (del.Count > 0 ? 1 : 0));
                IEnumerable<PixelData> layer = pixelDatas.Where(d => d.Y >= y_layers[i] && d.Y < y_layers[i + 1]).ToList();
                cols_base.Clear();
                cols_base.AddRange(layer.Where(d => d.userColor == BitmapWork.empty).Select(d => d.X).Distinct().OrderBy(x => x));
                if (cols_base.Count == 0)
                {
                    return false;
                }
                cols.Clear();
                cols.Add(cols_base[0]);
                for (int idx = 0; idx < cols_base.Count - 1; idx++)
                {
                    if (cols_base[idx + 1] - cols_base[idx] >= max_dist)
                    {
                        cols.Add(cols_base[idx + 1]);
                    }
                }
                cols.Add(cols_base.Last() + (PixelSize * max_dist));
                for (int idx = 0; idx <= cols.Count - 2; idx++)
                {
                    bottle_pixel_list.Add(layer.Where(d => d.X >= cols[idx] && d.X < cols[idx + 1]).ToList());
                }
            }
            return true;
        }

        internal static bool FillYLayers(List<int> y_layers, List<PixelData> pixelDatas)
        {
            y_layers.Clear();
            List<int> dist_y = new();
            dist_y.AddRange(pixelDatas.Where(d => d.userColor == BitmapWork.empty).Select(DataY).Distinct().OrderBy(y => y));
            if (dist_y.Count == 0)
            {
                return false;
            }
            y_layers.Add(dist_y[0]);
            x_dist = 0;
            for (int i = 0; i < dist_y.Count; i++)
            {
                List<int> find_x = pixelDatas.Where(d => d.userColor.color == BitmapWork.empty && Math.Abs(d.Y - dist_y[i]) <= PixelSize).Select(d => d.X).Distinct().OrderBy(x => x).ToList();
                for (int c = 0; c < find_x.Count - 1; c++)
                {
                    int dist = find_x[c + 1] - find_x[c];
                    if (x_dist < dist)
                    {
                        x_dist = dist;
                    }
                }
            }
            float amp = 0.6169f - 42.1184f / x_dist;
            //134 150 160 164
            //0.3 0.34 0.3625 0.35
            for (int i = 0; i < dist_y.Count - 1; i++)
            {
                List<PixelData> find_l = pixelDatas.Where(d => Math.Abs(d.Y - dist_y[i + 1]) <= PixelSize && d.userColor.color == BitmapWork.empty)
                                                   .OrderBy(d => d.X).ToList();
                int y_diff = dist_y[i + 1] - dist_y[i];
                if (y_diff >= (int)(BitmapWork.Size.Height * amp) && find_l.Count >= 10 && find_l[1].X - find_l[0].X < PixelSize * 2)
                {
                    y_layers.Add(dist_y[i + 1]);
                }
                else
                {
                    dist_y.RemoveAt(i + 1);
                    i--;
                }
            }
            y_layers.Add(pixelDatas.Max(d => d.Y));
            if (Math.Abs(y_layers[^1] - y_layers[^2]) <= BitmapWork.Size.Height / 10)
            {
                y_layers.Remove(Math.Min(y_layers[^1], y_layers[^2]));
            }
            return true;
        }

        internal static readonly Func<PixelData, int> DataY = d => d.Y;

        public override string ToString()
        {
            return $"{userColor} {X} {Y}";
        }
    }
}
