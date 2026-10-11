using NAudio.Wave;

namespace WhipCursor.Core;

/// <summary>Plays a <see cref="CachedSound"/> once at a given pitch (playback-rate) and volume.</summary>
public sealed class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound _sound;
    private readonly double _rate;
    private readonly float _volume;
    private double _frame;

    public CachedSoundSampleProvider(CachedSound sound, float pitch, float volume)
    {
        if (!(pitch > 0)) throw new ArgumentOutOfRangeException(nameof(pitch));
        _sound = sound;
        _rate = pitch;
        _volume = volume;
    }

    public WaveFormat WaveFormat => _sound.Format;

    public int Read(float[] buffer, int offset, int count)
    {
        int channels = _sound.Format.Channels;
        int totalFrames = _sound.Samples.Length / channels;
        int framesWanted = count / channels;
        int written = 0;

        for (int f = 0; f < framesWanted; f++)
        {
            int i0 = (int)_frame;
            if (i0 >= totalFrames) break;
            int i1 = Math.Min(i0 + 1, totalFrames - 1);
            float frac = (float)(_frame - i0);

            for (int c = 0; c < channels; c++)
            {
                float a = _sound.Samples[i0 * channels + c];
                float b = _sound.Samples[i1 * channels + c];
                buffer[offset + written++] = (a + (b - a) * frac) * _volume;
            }
            _frame += _rate;
        }
        return written;
    }
}
