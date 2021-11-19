using System;
using System.Diagnostics;

namespace WaterColorSort.Classes
{
    internal static class ProcessWork
    {
        internal static string GetProcess()
        {
            Process[] scr = Process.GetProcessesByName("scrcpy");
            Process android = scr.Length == 0 ? Process.Start("scrcpy/scrcpy.exe") : scr[0];
            System.Threading.Tasks.Task HandleTask = System.Threading.Tasks.Task.Run(() =>
            {
                while (android.MainWindowHandle == IntPtr.Zero)
                {
                    System.Threading.Tasks.Task.Delay(100);
                }
            });
            HandleTask.Wait();
            return android.MainWindowTitle;
        }

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
