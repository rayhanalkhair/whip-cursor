namespace WhipCursor.Core;

public readonly record struct Spark(Vec2 Position, Vec2 Velocity, float Age, float Life)
{
    /// <summary>1 when born, 0 when expired.</summary>
    public float Remaining => 1f - Age / Life;
}

/// <summary>Tiny particle burst at the whip tip when it cracks.</summary>
public sealed class SparkSystem
{
    public const int SparksPerCrack = 6;
    public const float LifeSeconds = 0.15f;
    private const float Spread = MathF.PI / 3f;
    private const float FrictionPerSecond = 6f;

    private readonly List<Spark> _sparks = new();

    public IReadOnlyList<Spark> Sparks => _sparks;
    public bool IsActive => _sparks.Count > 0;

    public void Emit(Vec2 at, Vec2 direction, float scale, Random random)
    {
        float baseAngle = direction.Length > 1e-3f ? MathF.Atan2(direction.Y, direction.X) : -MathF.PI / 2f;
        for (int i = 0; i < SparksPerCrack; i++)
        {
            float angle = baseAngle + ((float)random.NextDouble() * 2f - 1f) * Spread;
            float speed = (150f + (float)random.NextDouble() * 250f) * scale;
            var velocity = new Vec2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
            _sparks.Add(new Spark(at, velocity, 0f, LifeSeconds));
        }
    }

    public void Update(float dt)
    {
        float keep = MathF.Max(0f, 1f - FrictionPerSecond * dt);
        for (int i = _sparks.Count - 1; i >= 0; i--)
        {
            var spark = _sparks[i];
            float age = spark.Age + dt;
            if (age >= spark.Life)
            {
                _sparks.RemoveAt(i);
                continue;
            }
            _sparks[i] = spark with
            {
                Position = spark.Position + spark.Velocity * dt,
                Velocity = spark.Velocity * keep,
                Age = age,
            };
        }
    }

    public void Clear() => _sparks.Clear();
}
