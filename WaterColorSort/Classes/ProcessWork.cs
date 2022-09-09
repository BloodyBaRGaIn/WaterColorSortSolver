using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WaterColorSort.Classes
{
    [Flags]
    internal enum ADBStatus : byte
    {
        Ok = 0,
        NoADB = 1,
        ExecError = 2,
        NoDevice = 4,
        TooManyDevice = 8,
        NoApp = 16,
        DeviceLocked = 32,
        NoFocus = 64
    };


    internal static class ProcessWork
    {
        private const string AppName = "com.vnstartllc.sort.water";
        private const string ADBNAME = "adb.exe";

        private static readonly string CURR_DIR = Directory.GetCurrentDirectory();
        private static readonly string ADBPATH = $"{CURR_DIR}\\adb\\{ADBNAME}";
        private static readonly string StartCommand = $"shell monkey -p {AppName} -c android.intent.category.LAUNCHER 1";

        internal static volatile ADBStatus Status = ADBStatus.Ok;

        internal static void KillADB()
        {
            foreach (Process p in Process.GetProcessesByName(ADBNAME))
            {
                p.Kill();
                p.Dispose();
            }
        }

        internal static void StartLiveConnectionCheck()
        {
            if (LiveConnectionCheck.Status == TaskStatus.Created)
            {
                LiveConnectionCheck.Start();
            }
        }

        internal static void StartApp(int start_delay = 20000)
        {
            KillADB();
            if (!File.Exists(ADBPATH))
            {
                Status |= ADBStatus.NoADB;
                ThrowError();
            }
            CheckConnection();
            int delay = start_delay;            
            // started but not focused
            if (GetStreamData($"shell pidof {AppName}").Length > 0)
            {
                delay = 1000;
            }
            // focused
            if (IsTargetAppFocused())
            {
                delay = 1;
            }
            using Task delayTask = Task.Delay(delay);
            using Task appTask = Task.Run(() =>
            {
                string res = GetStreamData(StartCommand);
                if (res.Contains("No activities found to run, monkey aborted."))
                {
                    Status |= ADBStatus.NoApp;
                    ThrowError();
                }
            });
            Task.WaitAll(delayTask, appTask);
        }

        internal static async Task Click(params ClickData[] ps)
        {
            if (ps.Length == 0)
            {
                return;
            }
            string command = "shell ";
            if (ps.Length > 1)
            {
                command += "\"";
                for (int i = 0; i < ps.Length; i++)
                {
                    command += MakeClickCommand(ps[i]);
                    command += (i < ps.Length - 1) ? "; " : "\"";
                }
            }
            else
            {
                command += MakeClickCommand(ps[0]);
            }
            await RunCommand(command);
        }

        internal static StreamReader GetStream(string command)
        {
            return Process.Start(GetInfo(command)).StandardOutput;
        }

        private static string MakeClickCommand(ClickData data)
        {
            return $"input tap {data.x} {data.y} & sleep {data.sleep.ToString("F2").Replace(',', '.')}";
        }

        private static Task RunCommand(string command)
        {
            Process process = Process.Start(GetInfo(command));
            string error = process.StandardError.ReadToEnd();
            if (error.Contains("Error") || error.Contains("no devices/emulators found"))
            {
                Status |= ADBStatus.ExecError;
                ThrowError();
            }
            return process.WaitForExitAsync();
        }

        private static void CheckConnection()
        {
            string get_info = GetStreamData("devices");
            int num = System.Text.RegularExpressions.Regex.Matches(get_info, @"\tdevice").Count;
            if (num != 1)
            {
                Status |= (num == 0) ? ADBStatus.NoDevice : ADBStatus.TooManyDevice;
                ThrowError();
            }
        }

        private static string GetActiveApp()
        {
            CheckConnection();
            string stream_data = GetStreamData("shell dumpsys activity | grep top-activity");
            // locked
            if (string.IsNullOrWhiteSpace(stream_data))
            {
                Status |= ADBStatus.DeviceLocked;
                ThrowError();
            }
            return stream_data;
        }

        private static bool IsTargetAppFocused()
        {
            return GetActiveApp().Contains(AppName);
        }

        private static void CheckAppFocused()
        {
            if (!IsTargetAppFocused())
            {
                Status |= ADBStatus.NoFocus;
                ThrowError();
            }
        }

        private static readonly Task LiveConnectionCheck = new(() =>
        {
            while (true)
            {
                CheckConnection();
                CheckAppFocused();
                System.Threading.Thread.Sleep(100);
            }
        });

        private static readonly Task StatusCheckTask = Task.Run(() =>
        {
            while (true)
            {
                ThrowError();
                System.Threading.Thread.Sleep(1);
            }
        });

        private static void ThrowError(string error, int code)
        {
            Console.Clear();
            Console.ResetColor();
            Console.Error.WriteLine(error);
            KillADB();
            Environment.Exit(code);
        }

        private static Task ThrowTask = null;

        private static string error_text = "";

        internal static void ThrowError()
        {
            if (Status == ADBStatus.Ok || !string.IsNullOrWhiteSpace(error_text))
            {
                return;
            }

            if ((Status & ADBStatus.NoADB) == ADBStatus.NoADB)
            {
                error_text += $"Cannot find file {ADBPATH}\nProgram cannot start\n";
            }
            if ((Status & ADBStatus.ExecError) == ADBStatus.ExecError)
            {
                error_text += "Connection lost\n";
            }
            if ((Status & ADBStatus.NoDevice) == ADBStatus.NoDevice)
            {
                error_text += "No devices connected\n";
            }
            if ((Status & ADBStatus.TooManyDevice) == ADBStatus.TooManyDevice)
            {
                error_text += "Too many devices connected\n";
            }
            if ((Status & ADBStatus.NoApp) == ADBStatus.NoApp)
            {
                error_text += "Target application not found\n";
            }
            if ((Status & ADBStatus.DeviceLocked) == ADBStatus.DeviceLocked)
            {
                error_text += "Device is locked\n";
            }
            if ((Status & ADBStatus.NoFocus) == ADBStatus.NoFocus)
            {
                error_text += "Target app is not in focus\n";
            }

            if (ThrowTask == null)
            {
                ThrowTask = Task.Run(() => ThrowError(error_text, (int)Status));
                ThrowTask.Wait();
                ThrowTask?.Dispose();
            }
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
