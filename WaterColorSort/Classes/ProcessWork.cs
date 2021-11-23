using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    internal static class ProcessWork
    {
        private const string AppName = "com.vnstartllc.sort.water";
        private const string AppActivity = "org.cocos2dx.javascript.AppActivity";

        private static readonly string CURR_DIR = Directory.GetCurrentDirectory();
        private static readonly string ADB = $"{CURR_DIR}\\scrcpy\\adb.exe";
        private static readonly string StartCommand = $"shell am start {AppName}/{AppActivity}";
        private static string MakeClickCommand(int x, int y) => $"input tap {x} {y}";

        internal static Task StartApp(int start_delay = 15000)
        {
            if (GetStreamLength($"shell pidof {AppName}") == 0)
            {
                RunCommand(StartCommand);
                return Task.Delay(start_delay);
            }
            else
            {
                RunCommand(StartCommand);
                return Task.Delay(1000);
            }
        }

        internal static System.Drawing.Bitmap GetImage()
        {
            Stream stream = GetStream($"shell screencap -p");
            List<byte> data = new(1024);
            bool isCR = false;

            int read;
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

            using MemoryStream memory = new(data.ToArray());
#pragma warning disable CA1416 // Проверка совместимости платформы
            return new(memory);
#pragma warning restore CA1416 // Проверка совместимости платформы
        }

        internal static void Click(int x, int y) => RunCommand($"shell {MakeClickCommand(x, y)}");
        internal static void Click(List<(int x, int y)> ps)
        {
            if (ps.Count == 0)
            {
                return;
            }
            string command = "shell \"";
            foreach ((int x, int y) in ps)
            {
                command += $"{MakeClickCommand(x, y)}; ";
            }
            RunCommand($"{command[..^2]}\"");
        }

        private static void RunCommand(string command) => Process.Start(GetInfo(command)).WaitForExit();

        private static ProcessStartInfo GetInfo(string command, bool redirect = true)
        {
            ProcessStartInfo info = new(ADB, command);
            if (redirect)
            {
                info.RedirectStandardOutput = true;
                info.UseShellExecute = false;
            }
            return info;
        }

        private static Stream GetStream(string command) => Process.Start(GetInfo(command)).StandardOutput.BaseStream;

        private static int GetStreamLength(string command)
        {
            Stream stream = GetStream(command);
            string read = "";
            Task.Delay(100).Wait();
            while (true)
            {
                int r = stream.ReadByte();
                if (r == -1)
                {
                    break;
                }
                read += (char)r;
            }
            return read.Length;
        }
    }
}
