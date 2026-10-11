using System.Drawing.Drawing2D;

namespace WhipCursor;

internal static class TrayIconFactory
{
    /// <summary>Draws a small whip glyph: a handle and a curling lash.</summary>
    public static Icon Create()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var handle = new Pen(Color.FromArgb(255, 122, 78, 40), 5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            using var lash = new Pen(Color.FromArgb(255, 74, 46, 24), 2.5f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawLine(handle, 4, 28, 12, 18);
            g.DrawBezier(lash, 12, 18, 22, 4, 30, 14, 24, 24);
        }

        IntPtr hIcon = bitmap.GetHicon();
        try
        {
            using var temporary = Icon.FromHandle(hIcon);
            return (Icon)temporary.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(hIcon);
        }
    }
}
