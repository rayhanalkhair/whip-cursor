namespace WhipCursor.Core;

/// <summary>
/// Converts variable frame time into a whole number of fixed physics steps.
/// Elapsed time is clamped so a long stall (sleep, debugger) cannot cause a burst of steps.
/// </summary>
public sealed class FixedStepClock
{
    private double _accumulator;

    public FixedStepClock(double stepSeconds, int maxStepsPerAdvance)
    {
        if (!(stepSeconds > 0)) throw new ArgumentOutOfRangeException(nameof(stepSeconds));
        if (maxStepsPerAdvance < 1) throw new ArgumentOutOfRangeException(nameof(maxStepsPerAdvance));
        StepSeconds = stepSeconds;
        MaxStepsPerAdvance = maxStepsPerAdvance;
    }

    public double StepSeconds { get; }
    public int MaxStepsPerAdvance { get; }

    public int Advance(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0) return 0;

        _accumulator += Math.Min(elapsedSeconds, StepSeconds * MaxStepsPerAdvance);
        int steps = Math.Min((int)Math.Floor(_accumulator / StepSeconds), MaxStepsPerAdvance);
        _accumulator -= steps * StepSeconds;
        if (_accumulator < 0 || _accumulator >= StepSeconds) _accumulator = 0;
        return steps;
    }

    public void Reset() => _accumulator = 0;
}
