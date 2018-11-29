using System;
using System.Runtime.InteropServices;

namespace WhereIsMyMouse.Utils.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHook
    {
        public Point Point;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }
}
