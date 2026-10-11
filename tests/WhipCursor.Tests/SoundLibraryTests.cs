using NAudio.Utils;
using NAudio.Wave;

namespace WhipCursor.Tests;

public class SoundLibraryTests
{
    private static MemoryStream MakeWav(int sampleRate, int channels, int frames)
    {
        var samples = new float[frames * channels];
        for (int i = 0; i < samples.Length; i++) samples[i] = 0.5f * MathF.Sin(i * 0.05f);

        var stream = new MemoryStream();
        using (var writer = new WaveFileWriter(new IgnoreDisposeStream(stream), new WaveFormat(sampleRate, 16, channels)))
            writer.WriteSamples(samples, 0, samples.Length);
        stream.Position = 0;
        return stream;
    }

    private static NamedStream Named(string name, Func<Stream> open) => new(name, open);

    [Fact]
    public void Decode_Stereo44k_KeepsFrameCount()
    {
        var sound = SoundLibrary.Decode(MakeWav(44100, 2, 1000));
        Assert.Equal(2000, sound.Samples.Length);
        Assert.Equal(SoundLibrary.MixFormat, sound.Format);
    }

    [Fact]
    public void Decode_Mono_IsConvertedToStereo()
    {
        Assert.Equal(2000, SoundLibrary.Decode(MakeWav(44100, 1, 1000)).Samples.Length);
    }

    [Fact]
    public void Decode_22k_IsResampledTo44k()
    {
        int samples = SoundLibrary.Decode(MakeWav(22050, 1, 1000)).Samples.Length;
        Assert.InRange(samples, 3600, 4400);
    }

    [Fact]
    public void Decode_LongFile_IsTruncatedToMaxSeconds()
    {
        var sound = SoundLibrary.Decode(MakeWav(44100, 2, 44100 * 10));
        Assert.Equal((int)(SoundLibrary.MaxSeconds * 44100) * 2, sound.Samples.Length);
    }

    [Fact]
    public void LoadPreferred_BadUserFile_IsSkippedAndEmbeddedUsed()
    {
        var messages = new List<string>();
        var user = new[] { Named("broken.wav", () => new MemoryStream(new byte[] { 1, 2, 3, 4, 5 })) };
        var embedded = new[] { Named("crack1.wav", () => MakeWav(44100, 1, 500)) };

        var sounds = SoundLibrary.LoadPreferred(user, embedded, messages.Add);

        Assert.Single(sounds);
        Assert.Contains(messages, m => m.Contains("broken.wav"));
    }

    [Fact]
    public void LoadPreferred_ValidUserFile_WinsOverEmbedded()
    {
        var user = new[] { Named("mine.wav", () => MakeWav(44100, 2, 300)) };
        var embedded = new[]
        {
            Named("crack1.wav", () => MakeWav(44100, 2, 500)),
            Named("crack2.wav", () => MakeWav(44100, 2, 500)),
        };

        var sounds = SoundLibrary.LoadPreferred(user, embedded, _ => { });

        Assert.Single(sounds);
        Assert.Equal(600, sounds[0].Samples.Length);
    }
}
