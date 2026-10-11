namespace WhipCursor.Tests;

public class SplineTests
{
    [Fact]
    public void PassesThroughEveryControlPoint()
    {
        var points = new[] { new Vec2(0, 0), new Vec2(10, 5), new Vec2(20, -5), new Vec2(30, 0) };
        var curve = Spline.CatmullRom(points, 4);

        Assert.Equal(13, curve.Count);
        for (int k = 0; k < points.Length; k++)
        {
            var p = curve[k * 4];
            Assert.True((p - points[k]).Length < 1e-4f, $"index {k}: {p} vs {points[k]}");
        }
    }

    [Fact]
    public void EvenlySpacedCollinearPoints_StayOnTheLine()
    {
        var points = new[] { new Vec2(0, 0), new Vec2(10, 0), new Vec2(20, 0) };
        var curve = Spline.CatmullRom(points, 2);
        Assert.All(curve, p => Assert.True(MathF.Abs(p.Y) < 1e-4f));
        Assert.True(MathF.Abs(curve[1].X - 5f) < 1e-3f);
    }

    [Fact]
    public void FewerThanTwoPoints_ReturnedAsIs()
    {
        Assert.Empty(Spline.CatmullRom(Array.Empty<Vec2>(), 4));
        Assert.Single(Spline.CatmullRom(new[] { new Vec2(1, 2) }, 4));
    }
}
