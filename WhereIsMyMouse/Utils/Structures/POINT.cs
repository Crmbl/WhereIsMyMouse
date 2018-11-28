using System.Runtime.InteropServices;

namespace WhereIsMyMouse.Utils.Structures
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int x;
        public int y;
    }
}
