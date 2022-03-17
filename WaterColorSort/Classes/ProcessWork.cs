using System;
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
        private const string ADBNAME = "adb.exe";
        private const double DefaultSleep = 0.15;
        private static readonly string CURR_DIR = Directory.GetCurrentDirectory();
        private static readonly string ADBPATH = $"{CURR_DIR}\\adb\\{ADBNAME}";
        private static readonly string StartCommand = $"shell am start {AppName}/{AppActivity}";

        internal static void KillADB()
        {
            foreach (Process p in Process.GetProcessesByName(ADBNAME))
            {
                p.Kill();
                p.Dispose();
            }
        }

        internal static Task StartApp(int start_delay = 20000)
        {
            KillADB();
            if (!File.Exists(ADBPATH))
            {
                ThrowError($"Cannot find file {ADBPATH}\nProgram cannot start");
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

        internal static StreamReader GetStream(string command) => Process.Start(GetInfo(command)).StandardOutput;

        private static string MakeClickCommand(int x, int y, double sleep = DefaultSleep) => $"input tap {x} {y} & sleep {sleep.ToString().Replace(',', '.')}";

        private static Task RunCommand(string command)
        {
            Process process = Process.Start(GetInfo(command));
            string error = process.StandardError.ReadToEnd();
            if (error.Contains("Error") || error.Contains("no devices/emulators found"))
            {
                ThrowError($"Failed to run the command\n{error}");
            }
            return process.WaitForExitAsync();
        }

        private static readonly Task LiveConnectionCheck = Task.Run(() =>
        {
            while (true)
            {
                int num = System.Text.RegularExpressions.Regex.Matches(GetStreamData($"devices"), @"\d+\t\w+").Count;
                if (num != 1)
                {
                    ThrowError(num == 0 ? "No devices connected" : "Too many devices connected");
                }
                using Task delaytask = Task.Delay(100);
                delaytask.Wait();
                delaytask.Dispose();
            }
        });

        private static void ThrowError(string error)
        {
            Console.Clear();
            Console.ResetColor();
            Console.Error.WriteLine(error);
            KillADB();
            Environment.Exit(1);
        }

        private static ProcessStartInfo GetInfo(string command, bool redirect = true)
        {
            ProcessStartInfo info = new(ADBPATH, command);
            if (redirect)
            {
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;
            }
            return info;
        }

        private static string GetStreamData(string command)
        {
            using (Task delaytask = Task.Delay(100))
            {
                delaytask.Wait();
                delaytask.Dispose();
            }
            using StreamReader stream = GetStream(command);
            string read = stream.ReadToEnd();
            stream.Close();
            stream.Dispose();
            return read;
        }
    }
}
