namespace WhipCursor;

/// <summary>Thread-safe append-only log, rotated to log.txt.old past 1 MB. Never throws.</summary>
internal sealed class Logger
{
    private const long MaxBytes = 1_000_000;
    private readonly string _path;
    private readonly object _gate = new();

    public Logger(string path) => _path = path;

    public void Info(string message) => Write("INFO", message, null);

    public void Error(string message, Exception? exception) => Write("ERROR", message, exception);

    private void Write(string level, string message, Exception? exception)
    {
        lock (_gate)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                var info = new FileInfo(_path);
                if (info.Exists && info.Length > MaxBytes) File.Move(_path, _path + ".old", overwrite: true);
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                if (exception is not null) line += Environment.NewLine + exception;
                File.AppendAllText(_path, line + Environment.NewLine);
            }
            catch
            {
                // logging must never take the app down
            }
        }
    }
}
