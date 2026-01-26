using SkiaSharp;

namespace AndroidDraw;

public class StrokeSimplifier
{
    /// <summary>
    /// Simplifies strokes using Ramer-Douglas-Peucker algorithm to reduce point count
    /// while preserving the overall shape.
    /// </summary>
    public List<Stroke> Simplify(List<Stroke> strokes, float epsilon)
    {
        var simplified = new List<Stroke>(strokes.Count);

        foreach (var stroke in strokes)
        {
            if (stroke.Points.Count <= 2)
            {
                simplified.Add(stroke);
                continue;
            }

            var simplifiedPoints = RamerDouglasPeucker(stroke.Points, epsilon);
            simplified.Add(new Stroke { Points = simplifiedPoints, Character = stroke.Character });
        }

        return simplified;
    }

    private List<SKPoint> RamerDouglasPeucker(List<SKPoint> points, float epsilon)
    {
        if (points.Count < 3)
            return new List<SKPoint>(points);

        // Find the point with maximum distance from the line between first and last
        float maxDistance = 0;
        int maxIndex = 0;

        var first = points[0];
        var last = points[^1];

        for (int i = 1; i < points.Count - 1; i++)
        {
            float distance = PerpendicularDistance(points[i], first, last);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                maxIndex = i;
            }
        }

        // If max distance is greater than epsilon, recursively simplify
        if (maxDistance > epsilon)
        {
            var left = RamerDouglasPeucker(points.Take(maxIndex + 1).ToList(), epsilon);
            var right = RamerDouglasPeucker(points.Skip(maxIndex).ToList(), epsilon);

            // Combine results (skip duplicate point at junction)
            var result = new List<SKPoint>(left);
            result.AddRange(right.Skip(1));
            return result;
        }
        else
        {
            // All points between first and last are within epsilon, keep only endpoints
            return [first, last];
        }
    }

    private static float PerpendicularDistance(SKPoint point, SKPoint lineStart, SKPoint lineEnd)
    {
        float dx = lineEnd.X - lineStart.X;
        float dy = lineEnd.Y - lineStart.Y;

        // Handle degenerate case where line is actually a point
        float lineLengthSquared = dx * dx + dy * dy;
        if (lineLengthSquared == 0)
            return Distance(point, lineStart);

        // Calculate perpendicular distance using cross product formula
        float numerator = Math.Abs(dy * point.X - dx * point.Y + lineEnd.X * lineStart.Y - lineEnd.Y * lineStart.X);
        float denominator = MathF.Sqrt(lineLengthSquared);

        return numerator / denominator;
    }

    private static float Distance(SKPoint a, SKPoint b)
    {
        float dx = b.X - a.X;
        float dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
