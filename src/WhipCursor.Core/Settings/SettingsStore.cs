using System.Text.Json;
using System.Text.Json.Serialization;

namespace WhipCursor.Core;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path;

    public SettingsStore(string path) => _path = path;

    /// <summary>Loads settings; any missing, unreadable or invalid file yields defaults.</summary>
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_path), Options);
            return loaded is null ? new AppSettings() : Sanitize(loaded);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new AppSettings();
        }
    }

    /// <summary>Writes atomically via a temp file. Throws on IO failure.</summary>
    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        var temp = _path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(settings, Options));
        File.Move(temp, _path, overwrite: true);
    }

    private static AppSettings Sanitize(AppSettings s)
    {
        var defaults = new AppSettings();
        return s with
        {
            Volume = Enum.IsDefined(s.Volume) ? s.Volume : defaults.Volume,
            Length = Enum.IsDefined(s.Length) ? s.Length : defaults.Length,
            Sensitivity = Enum.IsDefined(s.Sensitivity) ? s.Sensitivity : defaults.Sensitivity,
        };
    }
}
