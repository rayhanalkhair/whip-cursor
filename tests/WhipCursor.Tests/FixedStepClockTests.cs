namespace WhipCursor.Tests;

public class FixedStepClockTests
{
    private const double Step = 1.0 / 120.0;

    [Fact]
    public void ExactlyOneStepOfTime_YieldsOneStep()
    {
        var clock = new FixedStepClock(Step, 12);
        Assert.Equal(1, clock.Advance(Step));
    }

    [Fact]
    public void HalfSteps_AccumulateIntoOneStep()
    {
        var clock = new FixedStepClock(Step, 12);
        Assert.Equal(0, clock.Advance(Step / 2));
        Assert.Equal(1, clock.Advance(Step / 2));
    }

    [Fact]
    public void HugeElapsed_IsClampedAndDoesNotCarryOver()
    {
        // e.g. the PC slept for an hour
        var clock = new FixedStepClock(Step, 12);
        Assert.Equal(12, clock.Advance(3600));
        Assert.Equal(1, clock.Advance(Step));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void NonPositiveOrInvalidElapsed_YieldsNoSteps(double elapsed)
    {
        var clock = new FixedStepClock(Step, 12);
        Assert.Equal(0, clock.Advance(elapsed));
    }

    [Fact]
    public void Reset_DropsAccumulatedTime()
    {
        var clock = new FixedStepClock(Step, 12);
        clock.Advance(Step * 0.9);
        clock.Reset();
        Assert.Equal(0, clock.Advance(Step * 0.5));
    }
}
