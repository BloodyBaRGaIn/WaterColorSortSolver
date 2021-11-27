
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
        private static readonly string ADB = $"{CURR_DIR}\\adb\\adb.exe";
        private static readonly string StartCommand = $"shell am start {AppName}/{AppActivity}";
        private static string MakeClickCommand(int x, int y, double sleep = DefaultSleep) => $"input tap {x} {y} & sleep {sleep.ToString().Replace(',', '.')}";

        internal static bool CheckDevice() => System.Text.RegularExpressions.Regex.Matches(GetStreamData($"devices"), @"\d+\t\w+").Count == 1;

        internal static void KillADB()
        {
            foreach (Process p in Process.GetProcessesByName("adb.exe"))
            {
                p.Kill();
                p.Dispose();
            }
        }

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

        internal static Stream GetStream(string command) => Process.Start(GetInfo(command)).StandardOutput.BaseStream;

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
