using System.Diagnostics;
using WhipCursor.Core;

namespace WhipCursor;

/// <summary>
/// Runs mouse polling, physics, crack sounds and rendering on its own thread.
/// Draws only while the whip moves; when it is settled it polls at ~30 Hz and does no rendering.
/// </summary>
internal sealed class WhipEngine : IDisposable
{
    private const byte OverlayOpacity = 217; // 85%
    private const int IdleSleepMs = 33;
    private const int HiddenSleepMs = 50;
    private const int DwmFallbackSleepMs = 8;
    private const double FullscreenCheckSeconds = 0.5;
    private const double TopmostCheckSeconds = 1.0;
    private const double DebugLogSeconds = 0.2;

    private readonly SoundPlayer _sound;
    private readonly Logger _log;
    private readonly Thread _thread;
    private readonly bool _debug = Environment.GetEnvironmentVariable("WHIPCURSOR_DEBUG") == "1";
    private volatile AppSettings _settings;
    private volatile bool _stopRequested;

    public WhipEngine(AppSettings settings, SoundPlayer sound, Logger log)
    {
        _settings = settings;
        _sound = sound;
        _log = log;
        _thread = new Thread(Run) { IsBackground = true, Name = "WhipEngine" };
        _thread.SetApartmentState(ApartmentState.STA);
    }

    public void Start() => _thread.Start();

    public void UpdateSettings(AppSettings settings) => _settings = settings;

    public void Dispose()
    {
        _stopRequested = true;
        if (_thread.IsAlive) _thread.Join(TimeSpan.FromSeconds(2));
    }

    private void Run()
    {
        try
        {
            RunLoop();
        }
        catch (Exception ex)
        {
            _log.Error("Whip engine stopped unexpectedly.", ex);
        }
    }

    private void RunLoop()
    {
        using var overlay = new OverlayWindow();
        var renderer = new WhipRenderer();
        var sparks = new SparkSystem();
        var random = new Random();
        var clock = Stopwatch.StartNew();
        WhipSimulator? sim = null;
        double last = 0, nextFullscreenCheck = 0, nextTopmost = 0, nextDebugLog = 0;
        bool fullscreen = false, needsDraw = true, firstDrawAfterCreate = false, drawFailing = false;

        while (!_stopRequested)
        {
            OverlayWindow.PumpMessages();
            double now = clock.Elapsed.TotalSeconds;
            double elapsed = now - last;
            last = now;
            var settings = _settings;

            if (!settings.Enabled || !MouseTracker.TryGetVisiblePosition(out var mouse))
            {
                overlay.Hide();
                sim = null;
                sparks.Clear();
                _sound.ReleaseIfIdle();
                Thread.Sleep(HiddenSleepMs);
                continue;
            }

            float scale = DpiHelper.ScaleAt(mouse);
            float length = Presets.LengthDip(settings.Length) * scale;
            float threshold = Presets.CrackThresholdDip(settings.Sensitivity) * scale;
            float gravity = Presets.GravityDip * scale;
            if (sim is null)
            {
                sim = new WhipSimulator(mouse, length, threshold, gravity);
                needsDraw = true;
                firstDrawAfterCreate = true;
            }
            sim.Configure(length, threshold, gravity);
            sim.TeleportDistance = Presets.TeleportDistanceDip * scale;

            if (now >= nextFullscreenCheck)
            {
                fullscreen = FullscreenDetector.IsForegroundFullscreen();
                nextFullscreenCheck = now + FullscreenCheckSeconds;
            }
            float volume = fullscreen ? 0f : Presets.Volume(settings.Volume);

            var frame = sim.Advance(mouse, elapsed);
            if (frame.CrackCount > 0)
            {
                _sound.PlayCrack(volume);
                if (settings.SparksEnabled) sparks.Emit(frame.Tip, frame.TipVelocity, scale, random);
                if (_debug) _log.Info($"CRACK tip={frame.PeakTipSpeed:F0}px/s threshold={threshold:F0} scale={scale:F2}");
            }
            if (_debug && frame.PeakTipSpeed > 1000f && now >= nextDebugLog)
            {
                _log.Info($"tip={frame.PeakTipSpeed:F0}px/s threshold={threshold:F0}");
                nextDebugLog = now + DebugLogSeconds;
            }
            sparks.Update((float)elapsed);

            bool active = !frame.IsSettled || sparks.IsActive;
            if (active && volume > 0f) _sound.Warm();
            else if (!active) _sound.ReleaseIfIdle();

            if (active || needsDraw)
            {
                using var image = renderer.Render(sim.Rope.Points, sparks.Sparks, scale);
                int error = overlay.Present(image.Bitmap, image.Left, image.Top, OverlayOpacity);
                if (error != 0 && !drawFailing)
                    _log.Error($"UpdateLayeredWindow failed (Win32 error {error}); overlay keeps its previous content. mouse=({mouse.X:F0},{mouse.Y:F0}) rect=({image.Left},{image.Top},{image.Bitmap.Width}x{image.Bitmap.Height})", null);
                drawFailing = error != 0;
                if (_debug && firstDrawAfterCreate)
                    _log.Info($"DRAW first frame of new whip: mouse=({mouse.X:F0},{mouse.Y:F0}) rect=({image.Left},{image.Top},{image.Bitmap.Width}x{image.Bitmap.Height}) error={error}");
                firstDrawAfterCreate = false;
                needsDraw = active; // draw one more frame after going idle to show the final rest pose
            }

            if (now >= nextTopmost)
            {
                overlay.EnsureTopmost();
                nextTopmost = now + TopmostCheckSeconds;
            }

            if (!active) Thread.Sleep(IdleSleepMs);
            else if (NativeMethods.DwmFlush() != 0) Thread.Sleep(DwmFallbackSleepMs);
        }
    }
}
