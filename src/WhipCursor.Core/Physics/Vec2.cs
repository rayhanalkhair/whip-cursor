namespace WhipCursor.Core;

/// <summary>2D vector in screen pixels (Y grows downward).</summary>
public readonly record struct Vec2(float X, float Y)
{
    public static readonly Vec2 Zero = new(0f, 0f);

    public float Length => MathF.Sqrt(X * X + Y * Y);

    public bool IsFinite => float.IsFinite(X) && float.IsFinite(Y);

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, float s) => new(a.X * s, a.Y * s);
    public static Vec2 operator /(Vec2 a, float s) => new(a.X / s, a.Y / s);

    public static Vec2 Lerp(Vec2 a, Vec2 b, float t) => a + (b - a) * t;
}
