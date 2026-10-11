using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using WhipCursor.Core;

namespace WhipCursor;

/// <summary>
/// Low-latency, overlapping crack playback through WASAPI shared mode.
/// The device is opened on demand and released when idle so the app never blocks system sleep.
/// </summary>
internal sealed class SoundPlayer : IDisposable
{
    private const int LatencyMs = 30;
    private const float PitchJitter = 0.08f;
    private const float VolumeJitter = 0.15f;
    private static readonly TimeSpan IdleRelease = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly IReadOnlyList<CachedSound> _sounds;
    private readonly Logger _log;
    private readonly Random _random = new();
    private readonly object _gate = new();
    private readonly Stopwatch _sinceUse = new();
    private readonly Stopwatch _sinceFailure = new();

    private WasapiOut? _output;
    private MixingSampleProvider? _mixer;
    private bool _broken;

    public SoundPlayer(IReadOnlyList<CachedSound> sounds, Logger log)
    {
        _sounds = sounds;
        _log = log;
    }

    public void PlayCrack(float volume)
    {
        if (volume <= 0f || _sounds.Count == 0) return;
        lock (_gate)
        {
            if (!EnsureOutput()) return;
            var sound = _sounds[_random.Next(_sounds.Count)];
            float pitch = 1f + Jitter(PitchJitter);
            float gain = Math.Clamp(volume * (1f + Jitter(VolumeJitter)), 0f, 1f);
            _mixer!.AddMixerInput(new CachedSoundSampleProvider(sound, pitch, gain));
        }
    }

    public void Warm()
    {
        if (_sounds.Count == 0) return;
        lock (_gate) EnsureOutput();
    }

    public void ReleaseIfIdle()
    {
        lock (_gate)
        {
            if (_output is not null && _sinceUse.Elapsed > IdleRelease) DisposeOutput();
        }
    }

    public void Dispose()
    {
        lock (_gate) DisposeOutput();
    }

    private float Jitter(float amount) => ((float)_random.NextDouble() * 2f - 1f) * amount;

    private bool EnsureOutput()
    {
        _sinceUse.Restart();
        if (_output is not null && !_broken) return true;
        if (_sinceFailure.IsRunning && _sinceFailure.Elapsed < RetryDelay) return false;

        DisposeOutput();
        try
        {
            _mixer = new MixingSampleProvider(SoundLibrary.MixFormat) { ReadFully = true };
            _output = new WasapiOut(AudioClientShareMode.Shared, true, LatencyMs);
            _output.PlaybackStopped += OnPlaybackStopped;
            _output.Init(_mixer);
            _output.Play();
            _broken = false;
            _sinceFailure.Reset();
            return true;
        }
        catch (Exception ex)
        {
            _log.Error("Audio output unavailable; whip continues without sound.", ex);
            DisposeOutput();
            _sinceFailure.Restart();
            return false;
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception is not null) _log.Error("Audio playback stopped (device change?).", e.Exception);
        lock (_gate) _broken = true;
    }

    private void DisposeOutput()
    {
        if (_output is not null)
        {
            _output.PlaybackStopped -= OnPlaybackStopped;
            try { _output.Dispose(); }
            catch (Exception ex) { _log.Error("Error closing audio output.", ex); }
        }
        _output = null;
        _mixer = null;
    }
}
