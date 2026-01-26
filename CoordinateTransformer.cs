using SkiaSharp;

namespace AndroidDraw;

/// <summary>
/// Transforms stroke coordinates to fit within a target screen area while maintaining aspect ratio.
/// </summary>
public class CoordinateTransformer
{
    public List<Stroke> Transform(
        List<Stroke> strokes,
        SKRect sourceBounds,
        float targetX,
        float targetY,
        float targetWidth,
        float targetHeight)
    {
        float scaleX = targetWidth / sourceBounds.Width;
        float scaleY = targetHeight / sourceBounds.Height;
        float scale = Math.Min(scaleX, scaleY);

        float scaledWidth = sourceBounds.Width * scale;
        float scaledHeight = sourceBounds.Height * scale;
        float offsetX = targetX + (targetWidth - scaledWidth) / 2;
        float offsetY = targetY + (targetHeight - scaledHeight) / 2;

        var result = new List<Stroke>(strokes.Count);

        foreach (var stroke in strokes)
        {
            var transformed = new Stroke { Character = stroke.Character };

            foreach (var point in stroke.Points)
            {
                float x = (point.X - sourceBounds.Left) * scale + offsetX;
                float y = (point.Y - sourceBounds.Top) * scale + offsetY;
                transformed.Points.Add(new SKPoint(x, y));
            }

            result.Add(transformed);
        }

        return result;
    }
}
