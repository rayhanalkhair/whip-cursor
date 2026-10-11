namespace WhipCursor.Core;

/// <summary>
/// Fires once when tip speed exceeds <see cref="Threshold"/>. It re-arms only after the speed has
/// dropped below <see cref="RearmFraction"/> × threshold and <see cref="CooldownSeconds"/> has passed.
/// </summary>
public sealed class CrackDetector
{
    private bool _armed = true;
    private bool _slowedSinceCrack;
    private double _lastCrackTime = double.NegativeInfinity;

    public CrackDetector(float threshold) => Threshold = threshold;

    /// <summary>Tip speed in px/s that triggers a crack.</summary>
    public float Threshold { get; set; }
    public float RearmFraction { get; init; } = 0.5f;
    public double CooldownSeconds { get; init; } = 0.25;

    public bool Update(float tipSpeed, double timeSeconds)
    {
        if (!_armed)
        {
            if (tipSpeed < Threshold * RearmFraction) _slowedSinceCrack = true;
            if (!_slowedSinceCrack || timeSeconds - _lastCrackTime < CooldownSeconds) return false;
            _armed = true;
        }

        if (!(tipSpeed > Threshold)) return false;

        _armed = false;
        _slowedSinceCrack = false;
        _lastCrackTime = timeSeconds;
        return true;
    }

    public void Reset()
    {
        _armed = true;
        _slowedSinceCrack = false;
        _lastCrackTime = double.NegativeInfinity;
    }
}
