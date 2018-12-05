using System.Runtime.InteropServices;

namespace WhereIsMyMouse.Utils.Structures
{
    /// <summary>
    /// A simple Point, with its coordinates.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public readonly int X;
        public readonly int Y;
    }
}
