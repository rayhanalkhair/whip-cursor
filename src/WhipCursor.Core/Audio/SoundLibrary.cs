using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WhipCursor.Core;

public sealed record NamedStream(string Name, Func<Stream> Open);

public static class SoundLibrary
{
    public static readonly WaveFormat MixFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
    public const double MaxSeconds = 5.0;

    /// <summary>Decodes a WAV stream into <see cref="MixFormat"/>. Throws on unreadable or unsupported data.</summary>
    public static CachedSound Decode(Stream wav)
    {
        using var reader = new WaveFileReader(wav);
        ISampleProvider provider = reader.ToSampleProvider();

        if (provider.WaveFormat.Channels == 1)
            provider = provider.ToStereo();
        else if (provider.WaveFormat.Channels != 2)
            throw new InvalidDataException($"Unsupported channel count {provider.WaveFormat.Channels}.");

        if (provider.WaveFormat.SampleRate != MixFormat.SampleRate)
            provider = new WdlResamplingSampleProvider(provider, MixFormat.SampleRate);

        int maxSamples = (int)(MaxSeconds * MixFormat.SampleRate) * MixFormat.Channels;
        var samples = new List<float>();
        var buffer = new float[MixFormat.SampleRate * MixFormat.Channels / 10];
        int read;
        while (samples.Count < maxSamples && (read = provider.Read(buffer, 0, buffer.Length)) > 0)
            for (int i = 0; i < read; i++) samples.Add(buffer[i]);

        if (samples.Count > maxSamples) samples.RemoveRange(maxSamples, samples.Count - maxSamples);
        if (samples.Count == 0) throw new InvalidDataException("WAV file contains no audio.");
        return new CachedSound(samples.ToArray(), MixFormat);
    }

    /// <summary>Uses the user's sounds if any decode, otherwise the embedded ones. Bad files are logged and skipped.</summary>
    public static IReadOnlyList<CachedSound> LoadPreferred(
        IEnumerable<NamedStream> user, IEnumerable<NamedStream> embedded, Action<string> log)
    {
        var fromUser = LoadAll(user, log);
        return fromUser.Count > 0 ? fromUser : LoadAll(embedded, log);
    }

    private static List<CachedSound> LoadAll(IEnumerable<NamedStream> sources, Action<string> log)
    {
        var sounds = new List<CachedSound>();
        foreach (var source in sources)
        {
            try
            {
                using var stream = source.Open();
                sounds.Add(Decode(stream));
            }
            catch (Exception ex) // arbitrary user files: any decode failure just skips the file
            {
                log($"Skipping sound '{source.Name}': {ex.Message}");
            }
        }
        return sounds;
    }
}
