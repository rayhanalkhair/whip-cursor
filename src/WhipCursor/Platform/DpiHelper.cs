using WhipCursor.Core;
using static WhipCursor.NativeMethods;

namespace WhipCursor;

internal static class DpiHelper
{
    /// <summary>Monitor scale factor (1.0 = 96 DPI) of the monitor containing the point.</summary>
    public static float ScaleAt(Vec2 screenPoint)
    {
        var monitor = MonitorFromPoint(new POINT((int)screenPoint.X, (int)screenPoint.Y), MONITOR_DEFAULTTONEAREST);
        return GetDpiForMonitor(monitor, MDT_EFFECTIVE_DPI, out uint dpiX, out _) == 0 && dpiX > 0
            ? dpiX / 96f
            : 1f;
    }
}
