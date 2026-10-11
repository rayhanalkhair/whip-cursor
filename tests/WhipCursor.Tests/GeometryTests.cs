namespace WhipCursor.Tests;

public class GeometryTests
{
    [Fact]
    public void Bounds_WorksWithNegativeCoordinates()
    {
        // a monitor to the left of / above the primary one
        var points = new[] { new Vec2(-1910.5f, -40f), new Vec2(-1800f, 60f) };
        var rect = Geometry.Bounds(points, 8f);
        Assert.Equal(new PixelRect(-1919, -48, 127, 116), rect);
    }

    [Fact]
    public void Bounds_SinglePointWithoutPadding_IsAtLeastOnePixel()
    {
        var rect = Geometry.Bounds(new[] { new Vec2(0, 0) }, 0f);
        Assert.Equal(1, rect.Width);
        Assert.Equal(1, rect.Height);
    }

    [Fact]
    public void Bounds_NoPoints_Throws()
    {
        Assert.Throws<ArgumentException>(() => Geometry.Bounds(Array.Empty<Vec2>(), 1f));
    }
}
