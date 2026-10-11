using NAudio.Wave;

namespace WhipCursor.Core;

/// <summary>A fully decoded sound, interleaved float samples in <see cref="Format"/>.</summary>
public sealed class CachedSound
{
    public CachedSound(float[] samples, WaveFormat format)
    {
        Samples = samples;
        Format = format;
    }

    public float[] Samples { get; }
    public WaveFormat Format { get; }
}
