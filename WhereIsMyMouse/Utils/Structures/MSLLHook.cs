using System;
using System.Runtime.InteropServices;

namespace WhereIsMyMouse.Utils.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHook
    {
        public Point pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
