using WhipCursor.Core;

namespace WhipCursor;

internal static class SoundSources
{
    private const string EmbeddedPrefix = "sounds.";

    /// <summary>User WAVs from %AppData%\WhipCursor\sounds win; otherwise the embedded crack sounds.</summary>
    public static IReadOnlyList<CachedSound> Load(Logger log)
    {
        var assembly = typeof(SoundSources).Assembly;
        var embedded = assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(EmbeddedPrefix, StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .Select(n => new NamedStream(n, () => assembly.GetManifestResourceStream(n)!));

        var user = Directory.Exists(AppPaths.SoundsDir)
            ? Directory.EnumerateFiles(AppPaths.SoundsDir, "*.wav")
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .Select(p => new NamedStream(p, () => File.OpenRead(p)))
            : Enumerable.Empty<NamedStream>();

        var sounds = SoundLibrary.LoadPreferred(user, embedded, log.Info);
        log.Info($"Loaded {sounds.Count} crack sound(s).");
        return sounds;
    }
}
