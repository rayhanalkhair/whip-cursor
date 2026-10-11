namespace WhipCursor.Core;

public readonly record struct WhipFrame(int CrackCount, Vec2 Tip, Vec2 TipVelocity, float PeakTipSpeed, bool IsSettled);

/// <summary>
/// Drives the rope from real frame times: fixed-step physics, mouse interpolation across sub-steps,
/// teleport handling and crack detection.
/// </summary>
public sealed class WhipSimulator
{
    public const int PointCount = 24;
    public const double StepSeconds = 1.0 / 120.0;
    public const int MaxStepsPerAdvance = 12;

    private readonly FixedStepClock _clock = new(StepSeconds, MaxStepsPerAdvance);
    private Vec2 _lastMouse;
    private double _simTime;

    public WhipSimulator(Vec2 mouse, float length, float crackThreshold, float gravity)
    {
        Rope = new RopeSimulation(PointCount, length, mouse) { Gravity = gravity };
        Detector = new CrackDetector(crackThreshold);
        _lastMouse = mouse;
    }

    public RopeSimulation Rope { get; }
    public CrackDetector Detector { get; }

    /// <summary>A mouse jump larger than this in one frame is treated as a teleport, not a flick.</summary>
    public float TeleportDistance { get; set; } = 800f;

    /// <summary>Rope counts as settled when every point is slower than this (px/s).</summary>
    public float SettledSpeed { get; set; } = 5f;

    public void Configure(float length, float crackThreshold, float gravity)
    {
        if (MathF.Abs(Rope.TotalLength - length) > 0.01f) Rope.SetLength(length);
        Detector.Threshold = crackThreshold;
        Rope.Gravity = gravity;
    }

    public void Reset(Vec2 mouse)
    {
        Rope.Reset(mouse);
        Detector.Reset();
        _clock.Reset();
        _lastMouse = mouse;
    }

    public WhipFrame Advance(Vec2 mouse, double elapsedSeconds)
    {
        if ((mouse - _lastMouse).Length > TeleportDistance)
        {
            Reset(mouse);
            return new WhipFrame(0, Rope.Tip, Vec2.Zero, 0f, true);
        }

        int steps = _clock.Advance(elapsedSeconds);
        int cracks = 0;
        float peak = 0f;
        for (int i = 0; i < steps; i++)
        {
            var anchor = Vec2.Lerp(_lastMouse, mouse, (i + 1f) / steps);
            Rope.Step(anchor, (float)StepSeconds);
            _simTime += StepSeconds;

            float speed = Rope.TipVelocity.Length;
            peak = MathF.Max(peak, speed);
            if (Detector.Update(speed, _simTime)) cracks++;
        }
        if (steps > 0) _lastMouse = mouse;

        bool settled = mouse == _lastMouse && Rope.MaxSpeed < SettledSpeed;
        return new WhipFrame(cracks, Rope.Tip, Rope.TipVelocity, peak, settled);
    }
}
