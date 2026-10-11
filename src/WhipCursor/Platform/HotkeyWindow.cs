using static WhipCursor.NativeMethods;

namespace WhipCursor;

/// <summary>Message-only window that owns the global Ctrl+Alt+W hotkey.</summary>
internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HotkeyId = 1;
    private const uint MOD_ALT = 0x0001;
    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_NOREPEAT = 0x4000;
    private const uint VK_W = 0x57;

    public HotkeyWindow()
    {
        CreateHandle(new CreateParams { Parent = HWND_MESSAGE });
        Registered = RegisterHotKey(Handle, HotkeyId, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_W);
    }

    public event EventHandler? Pressed;

    public bool Registered { get; }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_HOTKEY && m.WParam == (IntPtr)HotkeyId) Pressed?.Invoke(this, EventArgs.Empty);
        base.WndProc(ref m);
    }

    public void Dispose()
    {
        if (Registered) UnregisterHotKey(Handle, HotkeyId);
        DestroyHandle();
    }
}
