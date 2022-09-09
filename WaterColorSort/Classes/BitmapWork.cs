
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    internal static class BitmapWork
    {
        internal static Point Location = Point.Empty;
        internal static Size Size = Size.Empty;
        internal static Color empty;

        internal static readonly List<ColoredBitmap> named_resources;

        static BitmapWork()
        {
            named_resources = new(13)
            {
                new(new(Resources.Palette.blue, nameof(Resources.Palette.blue)), ConsoleColor.Blue),
                new(new(Resources.Palette.brown, nameof(Resources.Palette.brown)), ConsoleColor.DarkRed),
                new(new(Resources.Palette.cyan, nameof(Resources.Palette.cyan)), ConsoleColor.Cyan),
                new(new(Resources.Palette.dark_cyan, nameof(Resources.Palette.dark_cyan)), ConsoleColor.DarkCyan),
                new(new(Resources.Palette.empty, nameof(Resources.Palette.empty)), ConsoleColor.Black),
                new(new(Resources.Palette.gray, nameof(Resources.Palette.gray)), ConsoleColor.DarkGray),
                new(new(Resources.Palette.green, nameof(Resources.Palette.green)), ConsoleColor.Green),
                new(new(Resources.Palette.magenta, nameof(Resources.Palette.magenta)), ConsoleColor.Magenta),
                new(new(Resources.Palette.orange, nameof(Resources.Palette.orange)), ConsoleColor.DarkYellow),
                new(new(Resources.Palette.pink, nameof(Resources.Palette.pink)), ConsoleColor.White),
                new(new(Resources.Palette.purple, nameof(Resources.Palette.purple)), ConsoleColor.DarkMagenta),
                new(new(Resources.Palette.red, nameof(Resources.Palette.red)), ConsoleColor.Red),
                new(new(Resources.Palette.yellow, nameof(Resources.Palette.yellow)), ConsoleColor.Yellow),
            };
        }

        private static Bitmap GetImageFromStream()
        {
            StreamReader stream = ProcessWork.GetStream($"shell screencap -p");
            const int Capacity = 1024;
            List<byte> data = new(Capacity);
            byte[] buf = new byte[Capacity];
            bool isCR = false;
            int read;
            do
            {
                read = stream.BaseStream.Read(buf, 0, Capacity);
                for (int i = 0; i < read; i++) //convert CRLF to LF 
                {
                    if (isCR && buf[i] == 0x0A)
                    {
                        isCR = false;
                        data.RemoveAt(data.Count - 1);
                        data.Add(buf[i]);
                        continue;
                    }
                    isCR = buf[i] == 0x0D;
                    data.Add(buf[i]);
                }
            }
            while (read > 0);
            return data.Count == 0 ? null : new(new MemoryStream(data.ToArray()));
        }

        internal static IEnumerable<PixelData> GetPixels()
        {
            using Bitmap image = GetBitmap();
#if DEBUG
            image.Save("test.jpg", ImageFormat.Jpeg);
#endif
            List<PixelFindStruct> input_collection = named_resources
                .Select(s => new PixelFindStruct(
                    s.namedBitmap, new Bitmap(image).Clone(new(Point.Empty, image.Size), image.PixelFormat),
                    new List<PixelData>())).ToList();
            ParallelLoopResult loopres = Parallel.ForEach(input_collection, ForeachLoopAction);
            image.Dispose();
            GC.Collect();
            return input_collection.SelectMany(i => i.result);
        }

        internal static Bitmap GetBitmap()
        {
            Bitmap temp_map = GetImageFromStream();
            if (temp_map == null)
            {
                ProcessWork.Status |= ADBStatus.ExecError;
                ProcessWork.ThrowError();
                return null;
            }
            Location.X = 0;
            Location.Y = (int)(temp_map.Height * 0.25f);
            Size.Width = temp_map.Width;
            Size.Height = (int)(temp_map.Height * 0.55f);
            temp_map = temp_map.Clone(new(Location, Size), PixelFormat.Format32bppArgb);
            return temp_map;
        }

        internal static List<Point> FindBitmapsEntry(Bitmap sourceBitmap, Bitmap serchingBitmap, int toleration)
        {
            if (sourceBitmap == null)
            {
                throw new ArgumentNullException(nameof(sourceBitmap));
            }
            if (serchingBitmap == null)
            {
                throw new ArgumentNullException(nameof(serchingBitmap));
            }
            if (sourceBitmap.PixelFormat != serchingBitmap.PixelFormat)
            {
                throw new ArgumentException("Pixel formats arn't equal");
            }
            if (sourceBitmap.Width < serchingBitmap.Width || sourceBitmap.Height < serchingBitmap.Height)
            {
                throw new ArgumentException("Size of serchingBitmap bigger then sourceBitmap");
            }

            int pixelFormatSize = Image.GetPixelFormatSize(sourceBitmap.PixelFormat) / 8;

            BitmapData sourceBitmapData = sourceBitmap.LockBits(new(Point.Empty, sourceBitmap.Size),
                                                                ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
            int sourceBitmapBytesLength = sourceBitmapData.Stride * sourceBitmap.Height;
            byte[] sourceBytes = new byte[sourceBitmapBytesLength];
            Marshal.Copy(sourceBitmapData.Scan0, sourceBytes, 0, sourceBitmapBytesLength);
            sourceBitmap.UnlockBits(sourceBitmapData);

            BitmapData serchingBitmapData = serchingBitmap.LockBits(new(Point.Empty, serchingBitmap.Size),
                                                                    ImageLockMode.ReadOnly, serchingBitmap.PixelFormat);
            int serchingBitmapBytesLength = serchingBitmapData.Stride * serchingBitmap.Height;
            byte[] serchingBytes = new byte[serchingBitmapBytesLength];
            Marshal.Copy(serchingBitmapData.Scan0, serchingBytes, 0, serchingBitmapBytesLength);
            serchingBitmap.UnlockBits(serchingBitmapData);

            List<Point> pointsList = new();

            for (int mainY = 0; mainY < sourceBitmap.Height - serchingBitmap.Height + 1; mainY++)
            {
                int sourceY = mainY * sourceBitmapData.Stride;

                for (int mainX = 0; mainX < sourceBitmap.Width - serchingBitmap.Width + 1; mainX++)
                {
                    int sourceX = mainX * pixelFormatSize;

                    bool LoopFlag = true;
                    for (int c = 0; c < pixelFormatSize; c++)
                    {
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

                    for (int secY = 0; secY < serchingBitmap.Height; secY++)
                    {
                        int serchY = secY * serchingBitmapData.Stride;

                        int sourceSecY = (mainY + secY) * sourceBitmapData.Stride;

                        for (int secX = 0; secX < serchingBitmap.Width; secX++)
                        {
                            int serchX = secX * pixelFormatSize;

                            int sourceSecX = (mainX + secX) * pixelFormatSize;

                            for (int c = 0; c < pixelFormatSize; c++)
                            {
                                if (Math.Abs(sourceBytes[sourceSecX + sourceSecY + c] - serchingBytes[serchX + serchY + c]) > toleration)
                                {
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
                    {
                        pointsList.Add(new Point(mainX, mainY));
                    }
                }
            }
            return pointsList;
        }

        internal static void SaveColorImage(List<List<PixelData>> bottle_pixel_list, Rectangle bounds, Size size)
        {
            using Bitmap image = new(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(image);
            foreach (IEnumerable<PixelData> bottle_pixels in bottle_pixel_list)
            {
                foreach (PixelData data in bottle_pixels)
                {
                    graphics.FillRectangle(new SolidBrush(data.userColor), new Rectangle(new(data.X, data.Y), size));
                }
            }
            image.Save("level_find.png");
        }

        private static readonly Action<PixelFindStruct> ForeachLoopAction = new(param =>
        {
            using Bitmap img_cpy = param.img_cpy;
            using Bitmap tofind = new(param.namedBitmap.img);

            if (param.namedBitmap.name == nameof(empty))
            {
                empty = tofind.GetPixel(0, 0);
            }
            param.result.AddRange(FindBitmapsEntry(img_cpy, tofind, 30).Select(p => new PixelData(p.X, p.Y, new(tofind.GetPixel(0, 0), param.namedBitmap.name))));
            tofind.Dispose();
            img_cpy.Dispose();
        });
    }
}
