namespace WhipCursor.Tests;

public class SparkSystemTests
{
    [Fact]
    public void Emit_AddsSparksThatMoveAndThenExpire()
    {
        var sparks = new SparkSystem();
        sparks.Emit(new Vec2(100, 100), new Vec2(1, 0), 1f, new Random(1));
        Assert.Equal(SparkSystem.SparksPerCrack, sparks.Sparks.Count);
        Assert.True(sparks.IsActive);

        sparks.Update(0.05f);
        Assert.Equal(SparkSystem.SparksPerCrack, sparks.Sparks.Count);
        Assert.All(sparks.Sparks, s => Assert.NotEqual(new Vec2(100, 100), s.Position));

        sparks.Update(0.11f);
        Assert.False(sparks.IsActive);
    }

    [Fact]
    public void Emit_WithZeroDirection_ProducesFiniteSparks()
    {
        var sparks = new SparkSystem();
        sparks.Emit(Vec2.Zero, Vec2.Zero, 1f, new Random(1));
        sparks.Update(0.01f);
        Assert.All(sparks.Sparks, s => Assert.True(s.Position.IsFinite && s.Velocity.IsFinite));
    }
}
