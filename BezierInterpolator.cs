using SkiaSharp;

namespace AndroidDraw;

/// <summary>
/// Interpolates quadratic and cubic Bézier curves into line segments for drawing.
/// </summary>
public class BezierInterpolator
{
    private const int SegmentsPerCurve = 30;

    /// <summary>
    /// Interpolates a quadratic Bézier curve: P = (1-t)²P0 + 2(1-t)tP1 + t²P2
    /// </summary>
    public List<SKPoint> InterpolateQuadratic(SKPoint p0, SKPoint p1, SKPoint p2)
    {
        var points = new List<SKPoint>(SegmentsPerCurve + 1);

        for (int i = 0; i <= SegmentsPerCurve; i++)
        {
            float t = i / (float)SegmentsPerCurve;
            float u = 1 - t;

            float x = u * u * p0.X + 2 * u * t * p1.X + t * t * p2.X;
            float y = u * u * p0.Y + 2 * u * t * p1.Y + t * t * p2.Y;

            points.Add(new SKPoint(x, y));
        }

        return points;
    }

    /// <summary>
    /// Interpolates a cubic Bézier curve: P = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3
    /// </summary>
    public List<SKPoint> InterpolateCubic(SKPoint p0, SKPoint p1, SKPoint p2, SKPoint p3)
    {
        var points = new List<SKPoint>(SegmentsPerCurve + 1);

        for (int i = 0; i <= SegmentsPerCurve; i++)
        {
            float t = i / (float)SegmentsPerCurve;
            float u = 1 - t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t2 = t * t;
            float t3 = t2 * t;

            float x = u3 * p0.X + 3 * u2 * t * p1.X + 3 * u * t2 * p2.X + t3 * p3.X;
            float y = u3 * p0.Y + 3 * u2 * t * p1.Y + 3 * u * t2 * p2.Y + t3 * p3.Y;

            points.Add(new SKPoint(x, y));
        }

        return points;
    }
}
