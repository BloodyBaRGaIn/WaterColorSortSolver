using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WaterColorSort.Classes
{
    internal struct PixelData
    {
        internal const int PixelSize = 5;
        internal readonly int x, y;
        internal readonly UserColor c;

        internal PixelData(int x, int y, UserColor c)
        {
            this.x = x;
            this.y = y;
            this.c = c;
        }

        internal bool Colorsimilar(PixelData other)
        {
            Color _c = c.color;
            return (Math.Abs(_c.R - other.c.color.R) <= 10 && Math.Abs(_c.G - other.c.color.G) <= 10 && Math.Abs(_c.B - other.c.color.B) <= 10);
        }

        internal static bool MakeDataSets(List<int> y_layers, List<PixelData> pixelDatas, List<List<PixelData>> bottle_pixel_list)
        {
            List<int> cols_base = new();
            List<int> cols = new();
            for (int i = 0; i <= y_layers.Count - 2; i++)
            {
                IEnumerable<PixelData> layer = pixelDatas.Where(d => d.y >= y_layers[i] && d.y < y_layers[i + 1]).ToList();
                cols_base.Clear();
                cols_base.AddRange(layer.Where(d => d.c == BitmapWork.empty).Select(d => d.x).Distinct().OrderBy(x => x));
                if (cols_base.Count == 0)
                {
                    return false;
                }
                cols.Clear();
                cols.Add(cols_base[0]);
                for (int idx = 0; idx < cols_base.Count - 1; idx++)
                {
                    if (cols_base[idx + 1] - cols_base[idx] >= PixelSize * 4)
                    {
                        cols.Add(cols_base[idx + 1]);
                    }
                }
                cols.Add(cols_base.Last() + PixelSize * 20);
                for (int idx = 0; idx <= cols.Count - 2; idx++)
                {
                    bottle_pixel_list.Add(layer.Where(d => d.x >= cols[idx] && d.x < cols[idx + 1]).ToList());
                }
            }
            return true;
        }

        internal static bool FillYLayers(List<int> y_layers, List<PixelData> pixelDatas)
        {
            y_layers.Clear();
            List<int> dist_y = new();
            dist_y.AddRange(pixelDatas.Where(d => d.c == BitmapWork.empty).Select(d => d.y).Distinct().OrderBy(y => y));
            if (dist_y.Count == 0)
            {
                return false;
            }
            y_layers.Add(dist_y[0]);
            for (int i = 0; i < dist_y.Count - 1; i++)
            {
                List<PixelData> find_l = pixelDatas.Where(d => Math.Abs(d.y - dist_y[i + 1]) <= PixelSize && d.c.color == BitmapWork.empty)
                                                   .OrderBy(d => d.x).ToList();
                if (dist_y[i + 1] - dist_y[i] >= 240 && find_l.Count >= 10 && find_l[1].x - find_l[0].x < PixelSize * 2)
                {
                    y_layers.Add(dist_y[i + 1]);
                }
                else
                {
                    dist_y.RemoveAt(i + 1);
                    i--;
                }
            }
            y_layers.Add(pixelDatas.Max(d => d.y));
            if (Math.Abs(y_layers[^1] - y_layers[^2]) <= 30)
            {
                y_layers.Remove(Math.Min(y_layers[^1], y_layers[^2]));
            }
            return true;
        }

        internal static void DataReduction(List<PixelData> pixelDatas)
        {
            for (int i = 0; i < pixelDatas.Count - 1; i++)
            {
                for (int j = i + 1; j < pixelDatas.Count; j++)
                {
                    if (Math.Abs(pixelDatas[i].x - pixelDatas[j].x) < PixelSize
                        && Math.Abs(pixelDatas[i].y - pixelDatas[j].y) < PixelSize
                        && pixelDatas[i].Colorsimilar(pixelDatas[j]))
                    {
                        pixelDatas.RemoveAt(j--);
                    }
                }
            }
        }
    }
}
