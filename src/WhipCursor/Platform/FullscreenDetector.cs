using System.Runtime.InteropServices;
using System.Text;
using static WhipCursor.NativeMethods;

namespace WhipCursor;

internal static class FullscreenDetector
{
    private const int QUNS_BUSY = 2;
    private const int QUNS_RUNNING_D3D_FULL_SCREEN = 3;
    private const int QUNS_PRESENTATION_MODE = 4;

    public static bool IsForegroundFullscreen()
    {
        if (SHQueryUserNotificationState(out int state) == 0
            && state is QUNS_BUSY or QUNS_RUNNING_D3D_FULL_SCREEN or QUNS_PRESENTATION_MODE)
            return true;

        return ForegroundCoversItsMonitor();
    }

    private static bool ForegroundCoversItsMonitor()
    {
        IntPtr hwnd = GetForegroundWindow();
        if (hwnd == IntPtr.Zero || hwnd == GetShellWindow() || hwnd == GetDesktopWindow()) return false;

        var className = new StringBuilder(64);
        GetClassName(hwnd, className, className.Capacity);
        if (className.ToString() is "Progman" or "WorkerW") return false; // the desktop itself

        if (!GetWindowRect(hwnd, out var window)) return false;
        var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST), ref info)) return false;

        var monitor = info.rcMonitor;
        return window.Left <= monitor.Left && window.Top <= monitor.Top
            && window.Right >= monitor.Right && window.Bottom >= monitor.Bottom;
    }
}
