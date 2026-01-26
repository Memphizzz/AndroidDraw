using SkiaSharp;

namespace AndroidDraw;

/// <summary>
/// Converts SkiaSharp vector paths to drawable strokes by processing path verbs
/// (MoveTo, LineTo, QuadTo, CubicTo, Close) and interpolating curves.
/// </summary>
public class PathToStrokes
{
    private readonly BezierInterpolator _bezierInterpolator;

    public PathToStrokes(BezierInterpolator bezierInterpolator)
    {
        _bezierInterpolator = bezierInterpolator;
    }

    public List<Stroke> Convert(SKPath path) => Convert(path, '\0');

    public List<Stroke> Convert(SKPath path, char character)
    {
        var strokes = new List<Stroke>();
        var currentStroke = new Stroke { Character = character };
        var currentPoint = SKPoint.Empty;

        using var iterator = path.CreateIterator(false);
        var points = new SKPoint[4];

        SKPathVerb verb;
        while ((verb = iterator.Next(points)) != SKPathVerb.Done)
        {
            switch (verb)
            {
                case SKPathVerb.Move:
                    if (currentStroke.Points.Count > 0)
                    {
                        strokes.Add(currentStroke);
                        currentStroke = new Stroke { Character = character };
                    }
                    currentPoint = points[0];
                    currentStroke.Points.Add(currentPoint);
                    break;

                case SKPathVerb.Line:
                    currentPoint = points[1];
                    currentStroke.Points.Add(currentPoint);
                    break;

                case SKPathVerb.Quad:
                    var quadPoints = _bezierInterpolator.InterpolateQuadratic(currentPoint, points[1], points[2]);
                    foreach (var point in quadPoints.Skip(1))
                    {
                        currentStroke.Points.Add(point);
                    }
                    currentPoint = points[2];
                    break;

                case SKPathVerb.Cubic:
                    var cubicPoints = _bezierInterpolator.InterpolateCubic(currentPoint, points[1], points[2], points[3]);
                    foreach (var point in cubicPoints.Skip(1))
                    {
                        currentStroke.Points.Add(point);
                    }
                    currentPoint = points[3];
                    break;

                case SKPathVerb.Close:
                    if (currentStroke.Points.Count > 0)
                    {
                        var firstPoint = currentStroke.Points[0];
                        if (currentPoint != firstPoint)
                        {
                            currentStroke.Points.Add(firstPoint);
                        }
                        strokes.Add(currentStroke);
                        currentStroke = new Stroke { Character = character };
                    }
                    break;
            }
        }

        if (currentStroke.Points.Count > 0)
        {
            strokes.Add(currentStroke);
        }

        return strokes;
    }
}
