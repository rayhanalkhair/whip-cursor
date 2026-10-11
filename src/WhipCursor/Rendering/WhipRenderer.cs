using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using WhipCursor.Core;

namespace WhipCursor;

internal sealed record RenderedWhip(Bitmap Bitmap, int Left, int Top) : IDisposable
{
    public void Dispose() => Bitmap.Dispose();
}

/// <summary>Draws the whip (tapered, outlined) and sparks into a bitmap sized to their bounds.</summary>
internal sealed class WhipRenderer
{
    private static readonly Color WhipColor = Color.FromArgb(255, 74, 46, 24);
    private static readonly Color OutlineColor = Color.FromArgb(255, 232, 214, 190);
    private static readonly Color SparkColor = Color.FromArgb(255, 255, 214, 102);
    private const float HandleWidthDip = 3f;
    private const float TipWidthDip = 1f;
    private const float OutlineExtraDip = 1.5f;
    private const float SparkWidthDip = 1.5f;
    private const float SparkTailSeconds = 0.02f;
    private const int SplineSubdivisions = 4;

    public RenderedWhip Render(IReadOnlyList<Vec2> rope, IReadOnlyList<Spark> sparks, float scale)
    {
        var curve = Spline.CatmullRom(rope, SplineSubdivisions);
        var extent = new List<Vec2>(curve);
        foreach (var spark in sparks)
        {
            extent.Add(spark.Position);
            extent.Add(spark.Position - spark.Velocity * SparkTailSeconds);
        }

        var bounds = Geometry.Bounds(extent, (HandleWidthDip + OutlineExtraDip) * scale + 2f);
        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            g.TranslateTransform(-bounds.Left, -bounds.Top);
            DrawTaperedCurve(g, curve, OutlineColor, OutlineExtraDip * scale, scale);
            DrawTaperedCurve(g, curve, WhipColor, 0f, scale);
            DrawSparks(g, sparks, scale);
        }
        return new RenderedWhip(bitmap, bounds.Left, bounds.Top);
    }

    private static void DrawTaperedCurve(Graphics g, List<Vec2> curve, Color color, float extraWidth, float scale)
    {
        int last = curve.Count - 1;
        if (last < 1) return;
        using var pen = new Pen(color) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        for (int i = 0; i < last; i++)
        {
            float t = (float)i / last;
            pen.Width = (HandleWidthDip + (TipWidthDip - HandleWidthDip) * t) * scale + extraWidth;
            g.DrawLine(pen, curve[i].X, curve[i].Y, curve[i + 1].X, curve[i + 1].Y);
        }
    }

    private static void DrawSparks(Graphics g, IReadOnlyList<Spark> sparks, float scale)
    {
        foreach (var spark in sparks)
        {
            int alpha = (int)Math.Clamp(spark.Remaining * 255f, 0f, 255f);
            using var pen = new Pen(Color.FromArgb(alpha, SparkColor), SparkWidthDip * scale)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
            };
            var tail = spark.Position - spark.Velocity * SparkTailSeconds;
            g.DrawLine(pen, spark.Position.X, spark.Position.Y, tail.X, tail.Y);
        }
    }
}
