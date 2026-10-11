namespace WhipCursor.Core;

public static class Spline
{
    /// <summary>Uniform Catmull-Rom curve through all points, <paramref name="subdivisions"/> samples per segment.</summary>
    public static List<Vec2> CatmullRom(IReadOnlyList<Vec2> points, int subdivisions)
    {
        if (subdivisions < 1) throw new ArgumentOutOfRangeException(nameof(subdivisions));
        if (points.Count < 2) return new List<Vec2>(points);

        var result = new List<Vec2>((points.Count - 1) * subdivisions + 1);
        for (int i = 0; i < points.Count - 1; i++)
        {
            var p0 = i > 0 ? points[i - 1] : points[0] * 2f - points[1]; // reflected phantom point keeps ends unflattened
            var p1 = points[i];
            var p2 = points[i + 1];
            var p3 = i + 2 < points.Count ? points[i + 2] : points[^1] * 2f - points[^2];
            for (int s = 0; s < subdivisions; s++)
                result.Add(Evaluate(p0, p1, p2, p3, (float)s / subdivisions));
        }
        result.Add(points[^1]);
        return result;
    }

    private static Vec2 Evaluate(Vec2 p0, Vec2 p1, Vec2 p2, Vec2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return (p1 * 2f
                + (p2 - p0) * t
                + (p0 * 2f - p1 * 5f + p2 * 4f - p3) * t2
                + (p1 * 3f - p0 - p2 * 3f + p3) * t3) * 0.5f;
    }
}
