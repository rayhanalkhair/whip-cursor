namespace WhipCursor.Tests;

public class CrackDetectorTests
{
    private const float Threshold = 5000f;
    private const double Dt = 1.0 / 120.0;

    [Fact]
    public void OneFlick_ProducesExactlyOneCrack()
    {
        var detector = new CrackDetector(Threshold);
        int cracks = 0;
        // speed rises 0 → 15000 → 0 over ~0.1 s
        for (int i = 0; i <= 12; i++)
        {
            float speed = 15000f * MathF.Sin(MathF.PI * i / 12f);
            if (detector.Update(speed, i * Dt)) cracks++;
        }
        Assert.Equal(1, cracks);
    }

    [Fact]
    public void SlowMovement_NeverCracks()
    {
        var detector = new CrackDetector(Threshold);
        var random = new Random(7);
        for (int i = 0; i < 600; i++)
            Assert.False(detector.Update((float)random.NextDouble() * 4000f, i * Dt));
    }

    [Fact]
    public void SpeedEqualToThreshold_DoesNotCrack()
    {
        var detector = new CrackDetector(Threshold);
        Assert.False(detector.Update(Threshold, 0));
    }

    [Fact]
    public void SecondFlickWithinCooldown_IsIgnored()
    {
        var detector = new CrackDetector(Threshold);
        Assert.True(detector.Update(8000f, 0.00));
        Assert.False(detector.Update(1000f, 0.05));
        Assert.False(detector.Update(8000f, 0.10));
    }

    [Fact]
    public void SecondFlickAfterCooldownAndSlowdown_Cracks()
    {
        var detector = new CrackDetector(Threshold);
        Assert.True(detector.Update(8000f, 0.00));
        Assert.False(detector.Update(1000f, 0.10));
        Assert.True(detector.Update(8000f, 0.30));
    }

    [Fact]
    public void SustainedHighSpeed_CracksOnlyOnce()
    {
        var detector = new CrackDetector(Threshold);
        int cracks = 0;
        for (int i = 0; i < 240; i++)
            if (detector.Update(8000f, i * Dt)) cracks++;
        Assert.Equal(1, cracks);
    }

    [Fact]
    public void Reset_RearmsImmediately()
    {
        var detector = new CrackDetector(Threshold);
        Assert.True(detector.Update(8000f, 0.0));
        detector.Reset();
        Assert.True(detector.Update(8000f, 0.01));
    }
}
