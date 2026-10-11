namespace WhipCursor;

internal static class AppPaths
{
    public static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhipCursor");

    public static readonly string SettingsFile = Path.Combine(Root, "settings.json");
    public static readonly string LogFile = Path.Combine(Root, "log.txt");
    public static readonly string SoundsDir = Path.Combine(Root, "sounds");
}
