using WhipCursor.Core;

namespace WhipCursor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var log = new Logger(AppPaths.LogFile);
        var settings = new SettingsStore(AppPaths.SettingsFile).Load();
        using var sound = new SoundPlayer(SoundSources.Load(log), log);
        using var engine = new WhipEngine(settings, sound, log);
        engine.Start();
        MessageBox.Show("Whip Cursor is running. Click OK to quit.", "Whip Cursor");
    }
}
