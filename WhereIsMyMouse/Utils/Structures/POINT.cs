using System.Runtime.InteropServices;

namespace WhereIsMyMouse.Utils.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public readonly int X;
        public readonly int Y;
    }
}
