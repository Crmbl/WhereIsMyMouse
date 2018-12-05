using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace WhereIsMyMouse.Utils.Structures
{
    /// <summary>
    /// Defines the MSLLHook structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHook
    {
        public Point Point;
    }
}
