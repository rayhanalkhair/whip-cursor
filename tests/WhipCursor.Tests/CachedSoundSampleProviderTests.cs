using NAudio.Wave;

namespace WhipCursor.Tests;

public class CachedSoundSampleProviderTests
{
    private static CachedSound Ramp(int frames)
    {
        var samples = new float[frames * 2];
        for (int f = 0; f < frames; f++) { samples[2 * f] = f; samples[2 * f + 1] = -f; }
        return new CachedSound(samples, WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
    }

    [Fact]
    public void NormalPitchAndVolume_ReproducesSamples()
    {
        var sound = Ramp(10);
        var buffer = new float[100];
        int read = new CachedSoundSampleProvider(sound, 1f, 1f).Read(buffer, 0, buffer.Length);
        Assert.Equal(20, read);
        Assert.Equal(sound.Samples, buffer.Take(20).ToArray());
    }

    [Fact]
    public void DoublePitch_PlaysHalfAsManyFrames()
    {
        var buffer = new float[100];
        int read = new CachedSoundSampleProvider(Ramp(10), 2f, 1f).Read(buffer, 0, buffer.Length);
        Assert.Equal(10, read);
        Assert.Equal(new float[] { 0, 0, 2, -2, 4, -4, 6, -6, 8, -8 }, buffer.Take(10).ToArray());
    }

    [Fact]
    public void HalfVolume_HalvesSamples_AndEndsWithZero()
    {
        var provider = new CachedSoundSampleProvider(Ramp(4), 1f, 0.5f);
        var buffer = new float[8];
        Assert.Equal(8, provider.Read(buffer, 0, 8));
        Assert.Equal(new float[] { 0, 0, 0.5f, -0.5f, 1, -1, 1.5f, -1.5f }, buffer);
        Assert.Equal(0, provider.Read(buffer, 0, 8));
    }

    [Fact]
    public void NonPositivePitch_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CachedSoundSampleProvider(Ramp(4), 0f, 1f));
    }
}
