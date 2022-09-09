using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WaterColorSort.Classes
{
    internal sealed class OutputSink : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern int SetStdHandle(int nStdHandle, IntPtr hHandle);

        private const int StdOutHandleNumber = -11;
        private const int StdErrHandleNumber = -12;

        private const string NullStreamMessage = "Stream is null";
        private const string NullHandleMessage = "Handle is null";

        private static readonly TextWriter _oldOut;
        private static readonly TextWriter _oldError;
        private static readonly IntPtr _oldOutHandle;
        private static readonly IntPtr _oldErrorHandle;

        static OutputSink()
        {
            _oldOutHandle = GetStdHandle(StdOutHandleNumber);
            _oldErrorHandle = GetStdHandle(StdErrHandleNumber);
            _oldOut = Console.Out;
            _oldError = Console.Error;
        }

        internal OutputSink()
        {
            if (Console.Out == TextWriter.Null)
            {
                throw new ArgumentNullException(nameof(Console.Out), NullStreamMessage);
            }
            if (Console.Error == TextWriter.Null)
            {
                throw new ArgumentNullException(nameof(Console.Error), NullStreamMessage);
            }
            if (GetStdHandle(StdOutHandleNumber) == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(StdOutHandleNumber),
                                                $"{NullHandleMessage} ({StdOutHandleNumber})");
            }
            if (GetStdHandle(StdErrHandleNumber) == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(StdErrHandleNumber),
                                                $"{NullHandleMessage} ({StdErrHandleNumber})");
            }

            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
            _ = SetStdHandle(StdOutHandleNumber, IntPtr.Zero);
            _ = SetStdHandle(StdErrHandleNumber, IntPtr.Zero);
        }

        public void Dispose()
        {
            _ = SetStdHandle(StdOutHandleNumber, _oldOutHandle);
            _ = SetStdHandle(StdErrHandleNumber, _oldErrorHandle);
            Console.SetOut(_oldOut);
            Console.SetError(_oldError);
        }
    }
}
