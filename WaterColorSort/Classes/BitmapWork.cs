using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace WaterColorSort.Classes
{
    internal sealed class BitmapWork
    {
        internal const int X = 0;
        internal const int Y = 335;
        internal const int W = 720;
        internal const int H = 800;
        internal static Color empty;
        private const string ResPath = "Resources";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static IEnumerable<PixelData> GetPixels()
        {
            using Bitmap image = GetBitmap();
            image.Save("test.jpg", ImageFormat.Jpeg);
            if (!Directory.Exists(ResPath))
            {
                throw new DirectoryNotFoundException(ResPath);
            }
            foreach (string file in Directory.GetFiles(ResPath))
            {
                using Bitmap tofind = new(Image.FromFile(file));
                List<Point> Pts = FindBitmapsEntry(image, tofind, 30);
                string fname = GetFileName(file);
                if (fname == nameof(empty))
                {
                    empty = tofind.GetPixel(0, 0);
                }
                foreach (Point p in Pts)
                {
                    yield return new PixelData(p.X, p.Y, new(tofind.GetPixel(0, 0), fname));
                }
            }
            image.Dispose();
            GC.Collect();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static Bitmap GetBitmap() => ProcessWork.GetImage().Clone(new(X, Y, W, H), PixelFormat.Format32bppArgb);

        private static string GetFileName(string path) => path[(path.LastIndexOf('\\') + 1)..path.LastIndexOf('.')];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        public static List<Point> FindBitmapsEntry(Bitmap sourceBitmap, Bitmap serchingBitmap, int toleration)
        {
            if (sourceBitmap == null || serchingBitmap == null)
            {
#pragma warning disable CA2208 // Правильно создавайте экземпляры исключений аргументов
                throw new ArgumentNullException();
#pragma warning restore CA2208 // Правильно создавайте экземпляры исключений аргументов
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

            BitmapData sourceBitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                                                                ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
            int sourceBitmapBytesLength = sourceBitmapData.Stride * sourceBitmap.Height;
            byte[] sourceBytes = new byte[sourceBitmapBytesLength];
            Marshal.Copy(sourceBitmapData.Scan0, sourceBytes, 0, sourceBitmapBytesLength);
            sourceBitmap.UnlockBits(sourceBitmapData);

            BitmapData serchingBitmapData = serchingBitmap.LockBits(new Rectangle(0, 0, serchingBitmap.Width, serchingBitmap.Height),
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static void SaveColorImage(List<List<PixelData>> bottle_pixel_list, Rectangle bounds, Size size)
        {
            using Bitmap image = new(bounds.Width, bounds.Height);
            using Graphics graphics = Graphics.FromImage(image);
            foreach (IEnumerable<PixelData> bottle_pixels in bottle_pixel_list)
            {
                foreach (PixelData data in bottle_pixels)
                {
                    graphics.FillRectangle(new SolidBrush(data.c), new Rectangle(new(data.x, data.y), size));
                }
            }
            image.Save("level_find.png");
        }
    }
}
