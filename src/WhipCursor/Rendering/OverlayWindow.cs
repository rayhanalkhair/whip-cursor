using static WhipCursor.NativeMethods;

namespace WhipCursor;

/// <summary>
/// Small layered, click-through, topmost, non-activating window. Each frame it is moved and resized
/// to the whip's bounding box and given new per-pixel-alpha content.
/// Must be created, used and disposed on the same thread, which must pump messages.
/// </summary>
internal sealed class OverlayWindow : NativeWindow, IDisposable
{
    private bool _visible;

    public OverlayWindow()
    {
        CreateHandle(new CreateParams
        {
            Caption = "WhipCursor Overlay",
            X = 0,
            Y = 0,
            Width = 1,
            Height = 1,
            Style = WS_POPUP,
            ExStyle = WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TOPMOST,
        });
    }

    public void Present(Bitmap bitmap, int left, int top, byte opacity)
    {
        IntPtr screenDc = GetDC(IntPtr.Zero);
        IntPtr memoryDc = CreateCompatibleDC(screenDc);
        IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
        IntPtr previous = SelectObject(memoryDc, hBitmap);
        try
        {
            var size = new SIZE(bitmap.Width, bitmap.Height);
            var source = new POINT(0, 0);
            var destination = new POINT(left, top);
            var blend = new BLENDFUNCTION
            {
                BlendOp = AC_SRC_OVER,
                SourceConstantAlpha = opacity,
                AlphaFormat = AC_SRC_ALPHA,
            };
            UpdateLayeredWindow(Handle, screenDc, ref destination, ref size, memoryDc, ref source, 0, ref blend, ULW_ALPHA);
        }
        finally
        {
            SelectObject(memoryDc, previous);
            DeleteObject(hBitmap);
            DeleteDC(memoryDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }

        if (!_visible)
        {
            ShowWindow(Handle, SW_SHOWNOACTIVATE);
            _visible = true;
        }
    }

    public void Hide()
    {
        if (!_visible) return;
        ShowWindow(Handle, SW_HIDE);
        _visible = false;
    }

    public void EnsureTopmost()
    {
        if (_visible) SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    public static void PumpMessages()
    {
        while (PeekMessage(out var msg, IntPtr.Zero, 0, 0, PM_REMOVE))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    public void Dispose() => DestroyHandle();
}
