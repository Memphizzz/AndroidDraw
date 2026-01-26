using SkiaSharp;

namespace AndroidDraw;

/// <summary>
/// Represents a single continuous stroke (finger down, move, finger up).
/// </summary>
public class Stroke
{
    /// <summary>
    /// Ordered list of points in this stroke.
    /// First point is where finger touches down, last is where it lifts.
    /// </summary>
    public List<SKPoint> Points { get; set; } = [];

    /// <summary>
    /// The source character this stroke belongs to (for per-character options).
    /// </summary>
    public char Character { get; set; }
}
