using System;

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
}
