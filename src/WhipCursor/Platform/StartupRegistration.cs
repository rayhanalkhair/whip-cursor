using Microsoft.Win32;

namespace WhipCursor;

/// <summary>Per-user "Start with Windows" via the HKCU Run key (no admin rights).</summary>
internal static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "WhipCursor";

    public static void Apply(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
            key.SetValue(ValueName, $"\"{Environment.ProcessPath}\"");
        else
            key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
