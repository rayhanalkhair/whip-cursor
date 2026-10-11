using System.Runtime.InteropServices;
using WhipCursor.Core;
using static WhipCursor.NativeMethods;

namespace WhipCursor;

/// <summary>Reads the cursor position. Read-only: never hooks or intercepts input.</summary>
internal static class MouseTracker
{
    /// <returns>false when the system cursor is hidden or suppressed (e.g. video playback, touch).</returns>
    public static bool TryGetVisiblePosition(out Vec2 position)
    {
        var info = new CURSORINFO { cbSize = Marshal.SizeOf<CURSORINFO>() };
        if (!GetCursorInfo(ref info) || (info.flags & CURSOR_SHOWING) == 0)
        {
            position = default;
            return false;
        }
        position = new Vec2(info.ptScreenPos.X, info.ptScreenPos.Y);
        return true;
    }
}
