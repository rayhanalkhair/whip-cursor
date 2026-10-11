namespace WhipCursor.Core;

public readonly record struct PixelRect(int Left, int Top, int Width, int Height);

public static class Geometry
{
    /// <summary>Integer bounding box around the points, grown by <paramref name="padding"/> on every side.</summary>
    public static PixelRect Bounds(IReadOnlyList<Vec2> points, float padding)
    {
        if (points.Count == 0) throw new ArgumentException("At least one point is required.", nameof(points));

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        foreach (var p in points)
        {
            minX = MathF.Min(minX, p.X);
            minY = MathF.Min(minY, p.Y);
            maxX = MathF.Max(maxX, p.X);
            maxY = MathF.Max(maxY, p.Y);
        }

        int left = (int)MathF.Floor(minX - padding);
        int top = (int)MathF.Floor(minY - padding);
        int right = (int)MathF.Ceiling(maxX + padding);
        int bottom = (int)MathF.Ceiling(maxY + padding);
        return new PixelRect(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
    }
}
