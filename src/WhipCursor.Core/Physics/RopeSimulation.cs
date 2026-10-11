namespace WhipCursor.Core;

/// <summary>
/// Verlet rope whose first point is pinned to an anchor (the mouse).
/// A final follow-the-leader pass makes the rope inextensible, which keeps it stable under any anchor motion.
/// </summary>
public sealed class RopeSimulation
{
    private readonly Vec2[] _pos;
    private readonly Vec2[] _prev;

    public RopeSimulation(int pointCount, float totalLength, Vec2 anchor)
    {
        if (pointCount < 2) throw new ArgumentOutOfRangeException(nameof(pointCount));
        if (!(totalLength > 0)) throw new ArgumentOutOfRangeException(nameof(totalLength));
        _pos = new Vec2[pointCount];
        _prev = new Vec2[pointCount];
        SegmentLength = totalLength / (pointCount - 1);
        Reset(anchor);
    }

    public int PointCount => _pos.Length;
    public float SegmentLength { get; private set; }
    public float TotalLength => SegmentLength * (_pos.Length - 1);

    /// <summary>Downward acceleration in px/s².</summary>
    public float Gravity { get; set; } = 900f;

    /// <summary>Fraction of velocity lost per step (0..1).</summary>
    public float Damping { get; set; } = 0.1f;

    public int Iterations { get; set; } = 16;

    public IReadOnlyList<Vec2> Points => _pos;
    public Vec2 Tip => _pos[^1];

    /// <summary>Tip velocity over the last step, px/s.</summary>
    public Vec2 TipVelocity { get; private set; }

    /// <summary>Fastest non-anchor point speed over the last step, px/s.</summary>
    public float MaxSpeed { get; private set; }

    public void Reset(Vec2 anchor)
    {
        for (int i = 0; i < _pos.Length; i++)
        {
            _pos[i] = new Vec2(anchor.X, anchor.Y + i * SegmentLength);
            _prev[i] = _pos[i];
        }
        TipVelocity = Vec2.Zero;
        MaxSpeed = 0f;
    }

    public void SetLength(float totalLength)
    {
        if (!(totalLength > 0)) throw new ArgumentOutOfRangeException(nameof(totalLength));
        SegmentLength = totalLength / (_pos.Length - 1);
    }

    public void Step(Vec2 anchor, float dt)
    {
        if (!(dt > 0)) throw new ArgumentOutOfRangeException(nameof(dt));

        float keep = 1f - Damping;
        var gravityStep = new Vec2(0f, Gravity * dt * dt);

        _prev[0] = _pos[0];
        _pos[0] = anchor;
        for (int i = 1; i < _pos.Length; i++)
        {
            var current = _pos[i];
            var velocity = (current - _prev[i]) * keep;
            _prev[i] = current;
            _pos[i] = current + velocity + gravityStep;
        }

        for (int iteration = 0; iteration < Iterations; iteration++)
            for (int i = 0; i < _pos.Length - 1; i++)
                SolveSegment(i);

        ClampToMaxLength();

        float maxSpeed = 0f;
        for (int i = 1; i < _pos.Length; i++)
            maxSpeed = MathF.Max(maxSpeed, (_pos[i] - _prev[i]).Length / dt);
        MaxSpeed = maxSpeed;
        TipVelocity = (_pos[^1] - _prev[^1]) / dt;
    }

    private void SolveSegment(int i)
    {
        var delta = _pos[i + 1] - _pos[i];
        float length = delta.Length;
        if (length < 1e-4f) return;
        float error = (length - SegmentLength) / length;

        if (i == 0)
        {
            _pos[1] -= delta * error; // anchor is immovable
        }
        else
        {
            _pos[i] += delta * (error * 0.5f);
            _pos[i + 1] -= delta * (error * 0.5f);
        }
    }

    private void ClampToMaxLength()
    {
        for (int i = 1; i < _pos.Length; i++)
        {
            var delta = _pos[i] - _pos[i - 1];
            float length = delta.Length;
            if (length > SegmentLength)
                _pos[i] = _pos[i - 1] + delta * (SegmentLength / length);
        }
    }
}
