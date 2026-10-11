namespace WhipCursor.Tests;

public class WhipSimulatorTests
{
    private const double Frame = 1.0 / 120.0;
    private static readonly Vec2 Start = new(500, 500);

    private static WhipSimulator NewSettledSim(float threshold = 3000f)
    {
        var sim = new WhipSimulator(Start, 120f, threshold, 900f);
        for (int i = 0; i < 120; i++) sim.Advance(Start, Frame);
        return sim;
    }

    [Fact]
    public void FastFlick_CracksExactlyOnce()
    {
        var sim = NewSettledSim();
        int cracks = 0;
        // 400 px in ~42 ms ≈ 9600 px/s, then stop dead
        for (int i = 1; i <= 5; i++) cracks += sim.Advance(new Vec2(500 + 80 * i, 500), Frame).CrackCount;
        for (int i = 0; i < 240; i++) cracks += sim.Advance(new Vec2(900, 500), Frame).CrackCount;
        Assert.Equal(1, cracks);
    }

    [Fact]
    public void SlowCircles_NeverCrack()
    {
        var sim = NewSettledSim();
        int cracks = 0;
        for (int i = 0; i < 360; i++)
        {
            float angle = (float)(i * Frame * 3.0); // 3 rad/s on radius 100 ≈ 300 px/s
            var mouse = new Vec2(400 + 100 * MathF.Cos(angle), 500 + 100 * MathF.Sin(angle)); // angle 0 == Start, no initial jump
            cracks += sim.Advance(mouse, Frame).CrackCount;
        }
        Assert.Equal(0, cracks);
    }

    [Fact]
    public void Teleport_ResetsRopeWithoutCracking()
    {
        var sim = NewSettledSim();
        var far = new Vec2(2500, 300);
        var frame = sim.Advance(far, Frame);

        Assert.Equal(0, frame.CrackCount);
        Assert.Equal(far, sim.Rope.Points[0]);
        Assert.True(MathF.Abs(sim.Rope.Tip.Y - (far.Y + 120f)) < 0.01f);
        Assert.True(frame.IsSettled);
    }

    [Fact]
    public void HugeElapsed_AfterSleep_StaysFiniteAndQuiet()
    {
        var sim = NewSettledSim();
        var frame = sim.Advance(Start, 3600);
        Assert.Equal(0, frame.CrackCount);
        Assert.All(sim.Rope.Points, p => Assert.True(p.IsFinite));
    }

    [Fact]
    public void AfterMovementStops_ReportsSettled()
    {
        var sim = NewSettledSim();
        for (int i = 1; i <= 20; i++) sim.Advance(new Vec2(500 + 10 * i, 500), Frame);
        Assert.False(sim.Advance(new Vec2(705, 500), Frame).IsSettled);

        WhipFrame last = default;
        for (int i = 0; i < 360; i++) last = sim.Advance(new Vec2(705, 500), Frame);
        Assert.True(last.IsSettled);
    }

    [Fact]
    public void TinyElapsed_WithMouseMoved_IsNotSettled()
    {
        var sim = NewSettledSim();
        var frame = sim.Advance(new Vec2(510, 500), Frame / 10);
        Assert.False(frame.IsSettled);
    }

    [Fact]
    public void Configure_UpdatesLengthThresholdAndGravity()
    {
        var sim = NewSettledSim();
        sim.Configure(60f, 1234f, 450f);
        Assert.True(MathF.Abs(sim.Rope.TotalLength - 60f) < 0.01f);
        Assert.Equal(1234f, sim.Detector.Threshold);
        Assert.Equal(450f, sim.Rope.Gravity);
    }
}
