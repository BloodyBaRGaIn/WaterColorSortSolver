using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    internal static class ProcessWork
    {
        private static readonly string path = System.IO.Directory.GetCurrentDirectory();
        internal static string GetProcess()
        {
            Process[] scr = Process.GetProcessesByName("scrcpy");
            Process android = scr.Length == 0
                ? Process.Start(new ProcessStartInfo($"{path}\\scrcpy\\scrcpy.exe")
                { UseShellExecute = true })
                : scr[0];
            Task<int> HandleTask = Task.Run(() =>
            {
                while (android.MainWindowHandle == IntPtr.Zero)
                {
                    if (android.HasExited)
                    {
                        return android.ExitCode;
                    }
                    Task.Delay(100).Wait();
                }
                return 0;
            });
            HandleTask.Wait();
            return HandleTask.Result == 0 ? android?.MainWindowTitle : null;
        }

        internal static System.Drawing.Bitmap GetImage()
        {
            var stream = Process.Start(new ProcessStartInfo($"{path}\\scrcpy\\adb.exe", $"shell screencap -p") { RedirectStandardOutput = true, UseShellExecute = false }).StandardOutput.BaseStream;
            List<byte> data = new(1024);

            int read = 0;
            bool isCR = false;
            do
            {
                byte[] buf = new byte[1024];
                read = stream.Read(buf, 0, buf.Length);

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

            if (data.Count == 0)
            {
                Console.WriteLine("fail");
                return null;
            }

            using System.IO.MemoryStream memory = new(data.ToArray());
#pragma warning disable CA1416 // Проверка совместимости платформы
            return new(memory);
        }

        internal static void Click(int x, int y) => Process.Start(new ProcessStartInfo($"{path}\\scrcpy\\adb.exe", $"shell input tap {x} {y}")).WaitForExit();

        internal static System.Drawing.Rectangle DetectWindow(AutoItX3Lib.AutoItX3 autoItX3,
                                                              string android,
                                                              int x_shift = 0,
                                                              int y_shift = 0,
                                                              int w_shift = 0,
                                                              int h_shift = 0) => new(autoItX3.WinGetPosX(android) + x_shift,
                                                                                      autoItX3.WinGetPosY(android) + y_shift,
                                                                                      autoItX3.WinGetPosWidth(android) + w_shift,
                                                                                      autoItX3.WinGetPosHeight(android) + h_shift);
    }
}
