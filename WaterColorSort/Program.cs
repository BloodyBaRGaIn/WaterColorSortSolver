using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using WaterColorSort.Classes;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

using AutoItX3Lib;
using System.Threading;

namespace WaterColorSort
{
    static class Program
    {
        private const string StrButton = "LEFT";
        private const string ResPath = "Resources";

        static void Main(string[] args)
        {
            List<Bottle> Bottles = new();
            List<PixelData> pixelDatas = new();
            List<List<PixelData>> bottle_pixel_list = new();
            List<int> y_layers = new();
            List<int> y_base = new();
            List<Move> prev = new();
            List<Move> final = new();
            List<int> cols_base = new();
            List<int> cols = new();
            AutoItX3 autoItX3 = new();
            const int offset = 10;
            Color empty = Color.White;

            string android = GetProcess();
            _ = autoItX3.MouseMove(0, 0);
            autoItX3.WinActivate(android);
        _main:
            Bottle.f = false;
            Bottles.Clear();
            pixelDatas.Clear();
            bottle_pixel_list.Clear();
            y_layers.Clear();
            y_base.Clear();
            prev.Clear();
            final.Clear();
            cols_base.Clear();
            cols.Clear();
            Thread.Sleep(1000);

#pragma warning disable CA1416 // Проверка совместимости платформы


            DetectWindow(autoItX3, android, out int x, out int y, out int w, out int h);
            Rectangle Full = new(x, y, w, h);
            Rectangle bounds = new(x + 10, y + 150, w - 10, h - 300);
            using (Bitmap image = new(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                }
                image.Save("test.jpg", ImageFormat.Jpeg);
                if (!Directory.Exists(ResPath))
                {
                    throw new DirectoryNotFoundException(ResPath);
                }
                foreach (string file in Directory.GetFiles(ResPath))
                {
                    using Bitmap tofind = new(Image.FromFile(file));
                    List<Point> Pts = FindBitmapsEntry(image, tofind, 30);
                    pixelDatas.AddRange(Pts.Select(p => new PixelData(p.X, p.Y, tofind.GetPixel(0, 0))));
                    if (!file.EndsWith("empty.png"))
                    {
                        continue;
                    }
                    empty = tofind.GetPixel(0, 0);
                }
                pixelDatas = pixelDatas.OrderBy(d => d.y).ThenBy(d => d.x).ToList();
                for (int i = 0; i < pixelDatas.Count - 1; i++)
                {
                    for (int j = i + 1; j < pixelDatas.Count; j++)
                    {
                        if (Math.Abs(pixelDatas[i].x - pixelDatas[j].x) < 6
                            && Math.Abs(pixelDatas[i].y - pixelDatas[j].y) < 6
                            && pixelDatas[i].Colorsimilar(pixelDatas[j]))
                        {
                            pixelDatas.RemoveAt(j);
                            j--;
                        }
                    }
                }
                y_base.AddRange(pixelDatas.Where(d => d.c == empty).Select(d => d.y).Distinct().OrderBy(y => y));
                if (y_base.Count == 0)
                {
                    SkipAd(Full, autoItX3);
                    goto _main;
                }
                y_layers.Add(y_base[0]);
                for (int i = 0; i < y_base.Count - 1; i++)
                {
                    if (y_base[i + 1] - y_base[i] >= 18)
                    {
                        y_layers.Add(y_base[i + 1]);
                    }
                }
                y_layers.Add(Enumerable.Reverse(pixelDatas).SkipWhile(d => d.c == empty).FirstOrDefault().y);

                for (int i = 0; i <= y_layers.Count - 2; i++)
                {
                    IEnumerable<PixelData> layer = pixelDatas.Where(d => d.y >= y_layers[i] && d.y < y_layers[i + 1]).ToList();
                    cols_base.Clear();
                    cols_base.AddRange(layer.Where(d => d.c == empty).Select(d => d.x).Distinct().OrderBy(x => x));
                    cols.Clear();
                    if (cols_base.Count == 0)
                    {
                        SkipAd(Full, autoItX3);
                        goto _main;
                    }
                    cols.Add(cols_base[0]);
                    for (int idx = 0; idx < cols_base.Count - 1; idx++)
                    {
                        if (cols_base[idx + 1] - cols_base[idx] >= 18)
                        {
                            cols.Add(cols_base[idx + 1]);
                        }
                    }
                    cols.Add(cols_base.Last() + 50);
                    for (int idx = 0; idx <= cols.Count - 2; idx++)
                    {
                        bottle_pixel_list.Add(layer.Where(d => d.x >= cols[idx] && d.x < cols[idx + 1]).ToList());
                    }
                }
                SaveColorImage(bottle_pixel_list, image, new Size(6, 6));
            }
#pragma warning restore CA1416 // Проверка совместимости платформы

            Bottles.AddRange(FillBottles(empty, bottle_pixel_list));

            PrintBottles(Bottles);

            do
            {
                foreach (Move move in Bottle.GetMoves(Bottles).OrderByDescending(b => Bottles[b.to].Count))
                {
                    Bottle.MakeMove(Bottles, prev, move);
                }
                if (!prev.Any())
                {
                    SkipAd(Full, autoItX3);
                    goto _main;
                }

                prev.Reverse();
                System.Numerics.BigInteger chain = prev[0].gen_prev;
                final.Add(prev[0]);
                for (int i = 0; i < prev.Count; i++)
                {
                    if (prev[i].gen_next != chain)
                    {
                        continue;
                    }
                    chain = prev[i].gen_prev;
                    if (!final.LastOrDefault().Equals(prev[i].Opposite))
                    {
                        final.Add(prev[i]);
                    }
                }
                final.Reverse();
                foreach (Move move in final)
                {
                    Console.WriteLine(move);
                }
                int done = Bottle.ApplyMoves(Bottles, final);
                bool f = false;
                if (done != final.Count)
                {
                    final = final.Take(done).ToList();
                    f = true;
                }

                PrintBottles(Bottles);

                foreach (Bottle b in Bottles)
                {
                    if (b.TryPeek(out Color c))
                    {
                        Console.WriteLine(c.Name);
                    }
                    else
                    {
                        Console.WriteLine("Empty");
                    }
                }
                PerformMoves(bottle_pixel_list, final, bounds, autoItX3, offset);
                if (!Bottles.All(b => b.IsCompleted) || f)
                {
                    Console.WriteLine("FAILED");
                    goto _main;
                }
                Thread.Sleep(1000);
            }
            while (!prev.Any());

            GotoNext(bounds, autoItX3);
            //SkipAd(Full, autoItX3);

            goto _main;
        }

        private static string GetProcess()
        {
            Process[] scr = Process.GetProcessesByName("scrcpy");
            Process android = scr.Length == 0 ? Process.Start("scrcpy/scrcpy.exe") : scr[0];
            while (android.MainWindowHandle == IntPtr.Zero)
            {
                Thread.Sleep(1);
            }
            return android.MainWindowTitle;
        }

        private static void PrintBottles(IEnumerable<Bottle> Bottles)
        {
            foreach (Bottle b in Bottles)
            {
                Console.WriteLine(b);
            }
        }

        private static IEnumerable<Bottle> FillBottles(Color empty, List<List<PixelData>> bottle_pixel_list)
        {
            foreach (List<PixelData> b in bottle_pixel_list)
            {
                if (!b.Any(d => d.c != empty))
                {
                    yield return new Bottle();
                    continue;
                }
                int min_y = b.Min(d => d.y);
                int max_y = b.Max(d => d.y);
                int segment_len = (int)((max_y - min_y) / 4.5 + 1);
                min_y += segment_len / 2;
                List<Color> colors = new();
                for (int seg = 0; seg < 4; seg++)
                {
                    IEnumerable<PixelData> GetSegment = b.Where(d => d.y >= min_y + seg * segment_len && d.y < min_y + (seg + 1) * segment_len);
                    IEnumerable<IGrouping<Color, PixelData>> groups = GetSegment.GroupBy(d => d.c).OrderByDescending(gr => gr.Count());
                    if (groups.Any())
                    {
                        colors.Add(groups.First().Key);
                    }
                }
                colors.Reverse();
                yield return new Bottle(colors.ToArray());
            }
        }

        private static void PerformMoves(List<List<PixelData>> bottle_pixel_list, IEnumerable<Move> final, Rectangle bounds, AutoItX3 autoItX3, int offset)
        {
            _ = autoItX3.MouseMove(bounds.X, bounds.Y);
            foreach (Move move in final)
            {
                PixelData from = bottle_pixel_list[move.from][0];
                PixelData to = bottle_pixel_list[move.to][0];
                _ = autoItX3.MouseClick(StrButton, bounds.X + from.x + offset, bounds.Y + from.y + offset);
                int nX = bounds.X + to.x + offset;
                int nY = bounds.Y + to.y + offset;
                _ = autoItX3.MouseClick(StrButton, nX, nY);
                Thread.Sleep(10);
                _ = autoItX3.MouseClick(StrButton, nX, nY);
            }
        }

        private static void GotoNext(Rectangle bounds, AutoItX3 autoItX3)
        {
#pragma warning disable CA1416 // Проверка совместимости платформы
            using Bitmap image = new(bounds.Width, bounds.Height);
            using Graphics g = Graphics.FromImage(image);
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            using Bitmap tofind = new(Image.FromFile("InterfaceResources/complete.png"));
            List<Point> Pts = FindBitmapsEntry(image, tofind, 10);
            _ = autoItX3.MouseClick(StrButton, bounds.X + (int)Pts.Average(p => p.X), bounds.Y + (int)Pts.Average(p => p.Y));
#pragma warning restore CA1416 // Проверка совместимости платформы
        }

        private static void SkipAd(Rectangle bounds, AutoItX3 autoItX3)
        {
            int retries = 0;
#pragma warning disable CA1416 // Проверка совместимости платформы
            using Bitmap image = new(bounds.Width, bounds.Height);
            Bitmap[] tofind_arr = new Bitmap[]
            {
                new(Image.FromFile("InterfaceResources/skip.png")),
                new(Image.FromFile("InterfaceResources/skip2.png")),
                new(Image.FromFile("InterfaceResources/skip3.png")),
                new(Image.FromFile("InterfaceResources/skip4.png")),
            };
            while (true)
            {
                if (retries == 10)
                {
                    return;
                }
                using Graphics g = Graphics.FromImage(image);
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
                List<Point> Pts = new();
                foreach (Bitmap tofind in tofind_arr)
                {
                    Pts.AddRange(FindBitmapsEntry(image, tofind, 100));
                    if (Pts.Count != 0)
                    {
                        break;
                    }
                }
                image.Save("test.jpg", ImageFormat.Jpeg);
                if (Pts.Count == 0)
                {
                    retries++;
                    Thread.Sleep(1000);
                    continue;
                }
                _ = autoItX3.MouseClick(StrButton, bounds.X + Pts[0].X, bounds.Y + Pts[0].Y);
                foreach (Point p in Pts)
                {
                    g.FillRectangle(new SolidBrush(Color.Black), new Rectangle(p, new Size(1, 1)));
                }
                image.Save("test.jpg", ImageFormat.Jpeg);
                break;
            }
#pragma warning restore CA1416 // Проверка совместимости платформы
        }

        private static void SaveColorImage(List<List<PixelData>> bottle_pixel_list, Bitmap image, Size size)
        {
#pragma warning disable CA1416 // Проверка совместимости платформы
            using Graphics graphics = Graphics.FromImage(image);
            foreach (IEnumerable<PixelData> bottle_pixels in bottle_pixel_list)
            {
                foreach (PixelData data in bottle_pixels)
                {
                    graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black), 1), new Rectangle(new(data.x, data.y), size));
                    graphics.DrawRectangle(new Pen(new SolidBrush(data.c), 1), new Rectangle(new(data.x + 1, data.y + 1), size - new Size(2, 2)));
                    graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black), 1), new Rectangle(new(data.x + 2, data.y + 2), size - new Size(4, 4)));
                }
            }
            image.Save("level_find.png");
#pragma warning restore CA1416 // Проверка совместимости платформы
        }

        private static void DetectWindow(AutoItX3 autoItX3, string android, out int x, out int y, out int w, out int h)
        {
            h = autoItX3.WinGetClientSizeHeight(android);
            w = autoItX3.WinGetClientSizeWidth(android);
            x = autoItX3.WinGetPosX(android);
            y = autoItX3.WinGetPosY(android);
        }

        public static List<Point> FindBitmapsEntry(Bitmap sourceBitmap, Bitmap serchingBitmap, int toleration)
        {
            if (sourceBitmap == null || serchingBitmap == null)
            {
                throw new ArgumentNullException();
            }

#pragma warning disable CA1416 // Проверка совместимости платформы
            if (sourceBitmap.PixelFormat != serchingBitmap.PixelFormat)
            {
                throw new ArgumentException("Pixel formats arn't equal");
            }

            if (sourceBitmap.Width < serchingBitmap.Width || sourceBitmap.Height < serchingBitmap.Height)
            {
                throw new ArgumentException("Size of serchingBitmap bigger then sourceBitmap");
            }

            int pixelFormatSize = Image.GetPixelFormatSize(sourceBitmap.PixelFormat) / 8;


            // Copy sourceBitmap to byte array
            BitmapData sourceBitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                                                ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
            int sourceBitmapBytesLength = sourceBitmapData.Stride * sourceBitmap.Height;
            byte[] sourceBytes = new byte[sourceBitmapBytesLength];
            Marshal.Copy(sourceBitmapData.Scan0, sourceBytes, 0, sourceBitmapBytesLength);
            sourceBitmap.UnlockBits(sourceBitmapData);

            // Copy serchingBitmap to byte array
            BitmapData serchingBitmapData = serchingBitmap.LockBits(new Rectangle(0, 0, serchingBitmap.Width, serchingBitmap.Height),
                                                                    ImageLockMode.ReadOnly, serchingBitmap.PixelFormat);
            int serchingBitmapBytesLength = serchingBitmapData.Stride * serchingBitmap.Height;
            byte[] serchingBytes = new byte[serchingBitmapBytesLength];
            Marshal.Copy(serchingBitmapData.Scan0, serchingBytes, 0, serchingBitmapBytesLength);
            serchingBitmap.UnlockBits(serchingBitmapData);

            List<Point> pointsList = new();

            // Serching entries
            // minimazing serching zone
            // sourceBitmap.Height - serchingBitmap.Height + 1
            for (int mainY = 0; mainY < sourceBitmap.Height - serchingBitmap.Height + 1; mainY++)
            {
                int sourceY = mainY * sourceBitmapData.Stride;

                for (int mainX = 0; mainX < sourceBitmap.Width - serchingBitmap.Width + 1; mainX++)
                {// mainY & mainX - pixel coordinates of sourceBitmap
                 // sourceY + sourceX = pointer in array sourceBitmap bytes
                    int sourceX = mainX * pixelFormatSize;

                    bool LoopFlag = true;
                    for (int c = 0; c < pixelFormatSize; c++)
                    {// through the bytes in pixel
                        if (Math.Abs(sourceBytes[sourceX + sourceY + c] - serchingBytes[c]) > toleration)
                        {
                            LoopFlag = false;
                            break;
                        }
                    }
                    if (!LoopFlag)
                    {
                        continue;
                    }

                    LoopFlag = false;

                    // find fist equalation and now we go deeper) 
                    for (int secY = 0; secY < serchingBitmap.Height; secY++)
                    {
                        int serchY = secY * serchingBitmapData.Stride;

                        int sourceSecY = (mainY + secY) * sourceBitmapData.Stride;

                        for (int secX = 0; secX < serchingBitmap.Width; secX++)
                        {// secX & secY - coordinates of serchingBitmap
                         // serchX + serchY = pointer in array serchingBitmap bytes

                            int serchX = secX * pixelFormatSize;

                            int sourceSecX = (mainX + secX) * pixelFormatSize;

                            for (int c = 0; c < pixelFormatSize; c++)
                            {// through the bytes in pixel
                                if (Math.Abs(sourceBytes[sourceSecX + sourceSecY + c] - serchingBytes[serchX + serchY + c]) > toleration)
                                {
                                    // not equal - abort iteration
                                    LoopFlag = true;
                                    break;
                                }
                            }
                            if (LoopFlag)
                            {
                                break;
                            }
                        }
                        if (LoopFlag)
                        {
                            break;
                        }
                    }
                    if (!LoopFlag)
                    {// serching bitmap is founded!!
                        pointsList.Add(new Point(mainX, mainY));
                    }
                }
            }
#pragma warning restore CA1416 // Проверка совместимости платформы
            return pointsList;
        }
    }
}
