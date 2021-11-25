
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    internal static class ProcessWork
    {
        private const string AppName = "com.vnstartllc.sort.water";
        private const string AppActivity = "org.cocos2dx.javascript.AppActivity";
        private const double DefaultSleep = 0.15;
        private static readonly string CURR_DIR = Directory.GetCurrentDirectory();
        private static readonly string ADB = $"{CURR_DIR}\\scrcpy\\adb.exe";
        private static readonly string StartCommand = $"shell am start {AppName}/{AppActivity}";
        private static string MakeClickCommand(int x, int y, double sleep = DefaultSleep) => $"input tap {x} {y} & sleep {sleep.ToString().Replace(',', '.')}";

        internal static bool CheckDevice() => System.Text.RegularExpressions.Regex.Matches(GetStreamData($"devices"), @"\d+\t\w+").Count == 1;

        internal static Task StartApp(int start_delay = 15000)
        {
            while (!CheckDevice())
            {
                Task.Delay(100).Wait();
            }
            Task delay = Task.Delay(GetStreamData($"shell pidof {AppName}").Length == 0 ? start_delay : 1000);
            RunCommand(StartCommand).Wait();
            return delay;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Проверка совместимости платформы", Justification = "<Ожидание>")]
        internal static System.Drawing.Bitmap GetImage()
        {
            Stream stream = GetStream($"shell screencap -p");
            const int Capacity = 0x400;
            List<byte> data = new(Capacity);
            byte[] buf = new byte[Capacity];
            bool isCR = false;

            int read;
            do
            {
                read = stream.Read(buf, 0, Capacity);

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

            return data.Count == 0 ? null : (new(new MemoryStream(data.ToArray())));
        }

        internal static async Task Click(int x, int y, double sleep = DefaultSleep) => await RunCommand($"shell {MakeClickCommand(x, y, sleep)}");

        internal static async Task Click(List<((int x, int y), double sleep)> ps)
        {
            if (ps.Count == 0)
            {
                return;
            }
            string command = "shell \"";
            foreach (((int x, int y), double sleep) in ps)
            {
                command += $"{MakeClickCommand(x, y, sleep)}; ";
            }
            await RunCommand($"{command[..^2]}\"");
        }

        private static async Task RunCommand(string command) => await Process.Start(GetInfo(command)).WaitForExitAsync();

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

        private static string GetStreamData(string command)
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
            return read;
        }
    }
}
