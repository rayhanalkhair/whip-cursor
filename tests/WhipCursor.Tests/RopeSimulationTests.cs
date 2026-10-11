namespace WhipCursor.Tests;

public class RopeSimulationTests
{
    private const float Dt = 1f / 120f;
    private const float Length = 120f;

    private static RopeSimulation NewRope(Vec2 anchor) => new(24, Length, anchor);

    private static float MaxSegment(RopeSimulation rope)
    {
        float max = 0f;
        for (int i = 1; i < rope.PointCount; i++)
            max = MathF.Max(max, (rope.Points[i] - rope.Points[i - 1]).Length);
        return max;
    }

    private static void AssertNear(float expected, float actual, float tolerance) =>
        Assert.True(MathF.Abs(expected - actual) <= tolerance, $"Expected {expected} ± {tolerance}, got {actual}");

    [Fact]
    public void Reset_HangsStraightDownFromAnchor()
    {
        var rope = NewRope(new Vec2(10, 20));
        Assert.Equal(new Vec2(10, 20), rope.Points[0]);
        AssertNear(10f, rope.Tip.X, 0.001f);
        AssertNear(20f + Length, rope.Tip.Y, 0.01f);
    }

    [Fact]
    public void Step_PinsFirstPointToAnchor()
    {
        var rope = NewRope(Vec2.Zero);
        rope.Step(new Vec2(37, -5), Dt);
        Assert.Equal(new Vec2(37, -5), rope.Points[0]);
    }

    [Fact]
    public void ViolentJitter_StaysFiniteAndNeverOverstretches()
    {
        var rope = NewRope(new Vec2(500, 500));
        var random = new Random(42);
        for (int i = 0; i < 1200; i++)
        {
            var anchor = new Vec2(500 + random.Next(-200, 201), 500 + random.Next(-200, 201));
            rope.Step(anchor, Dt);
            Assert.All(rope.Points, p => Assert.True(p.IsFinite));
            Assert.True(MaxSegment(rope) <= rope.SegmentLength * 1.001f);
        }
    }

    [Fact]
    public void SteadyDrag_TipMovesAtAnchorSpeed()
    {
        var rope = NewRope(Vec2.Zero);
        const float speed = 1200f;
        for (int i = 1; i <= 120; i++) rope.Step(new Vec2(speed * Dt * i, 0), Dt);
        AssertNear(speed, rope.TipVelocity.X, speed * 0.1f);
    }

    [Fact]
    public void AfterAnchorStops_RopeSettlesHangingDown()
    {
        var rope = NewRope(Vec2.Zero);
        for (int i = 1; i <= 30; i++) rope.Step(new Vec2(10f * i, 0), Dt);
        var anchor = new Vec2(300, 0);
        for (int i = 0; i < 360; i++) rope.Step(anchor, Dt);

        Assert.True(rope.MaxSpeed < 5f, $"MaxSpeed {rope.MaxSpeed}");
        Assert.True(rope.Tip.Y - anchor.Y > Length * 0.95f, $"Tip.Y {rope.Tip.Y}");
        AssertNear(anchor.X, rope.Tip.X, Length * 0.05f);
    }

    [Fact]
    public void SetLength_WhileSwinging_ObeysNewLengthImmediately()
    {
        var rope = NewRope(new Vec2(400, 400));
        for (int i = 0; i < 60; i++)
            rope.Step(new Vec2(400 + 150 * MathF.Cos(i * 0.3f), 400 + 150 * MathF.Sin(i * 0.3f)), Dt);

        rope.SetLength(60f);
        rope.Step(new Vec2(420, 400), Dt);
        Assert.True(MaxSegment(rope) <= 60f / 23f * 1.001f);
        Assert.All(rope.Points, p => Assert.True(p.IsFinite));

        rope.SetLength(180f);
        for (int i = 0; i < 60; i++) rope.Step(new Vec2(420, 400), Dt);
        Assert.True(MaxSegment(rope) <= 180f / 23f * 1.001f);
        Assert.All(rope.Points, p => Assert.True(p.IsFinite));
    }

    [Theory]
    [InlineData(1, 120f)]
    [InlineData(24, 0f)]
    [InlineData(24, -5f)]
    public void InvalidConstruction_Throws(int points, float length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RopeSimulation(points, length, Vec2.Zero));
    }
}
