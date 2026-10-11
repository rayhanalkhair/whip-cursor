namespace WhipCursor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, @"Local\WhipCursor.SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance) return;

        ApplicationConfiguration.Initialize();
        var log = new Logger(AppPaths.LogFile);
        Application.ThreadException += (_, e) => log.Error("Unhandled UI exception.", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => log.Error("Unhandled exception.", e.ExceptionObject as Exception);
        log.Info("Whip Cursor starting.");

        using var app = new TrayApp(log);
        Application.Run(app);
        log.Info("Whip Cursor exiting.");
    }
}
