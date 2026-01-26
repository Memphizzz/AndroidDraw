using System.Reflection;
using SkiaSharp;

namespace AndroidDraw;

/// <summary>
/// Extracts vector path data from fonts using SkiaSharp.
/// </summary>
public class FontPathExtractor
{
    private const float TextSize = 200f;

    public SKPath GetTextPath(string text, string fontFamily)
    {
        var typeface = LoadTypeface(fontFamily);
        using var font = new SKFont(typeface, TextSize);
        return font.GetTextPath(text, new SKPoint(0, 0));
    }

    public SKPath GetCharPath(char c, string fontFamily, float xOffset)
    {
        var typeface = LoadTypeface(fontFamily);
        using var font = new SKFont(typeface, TextSize);
        return font.GetTextPath(c.ToString(), new SKPoint(xOffset, 0));
    }

    public float GetCharWidth(char c, string fontFamily)
    {
        var typeface = LoadTypeface(fontFamily);
        using var font = new SKFont(typeface, TextSize);
        return font.MeasureText(c.ToString());
    }

    private static SKTypeface LoadTypeface(string fontFamily)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Try embedded fonts first
        var embeddedFont = fontFamily.ToLowerInvariant() switch
        {
            "jetbrains mono" => "AndroidDraw.Fonts.JetBrainsMono-Regular.ttf",
            "hershey" or "hershey simplex" => "AndroidDraw.Fonts.AVHersheySimplexLight.ttf",
            "ibm" or "ibm plex" or "ibm plex mono" => "AndroidDraw.Fonts.IBMPlexMono-Thin.ttf",
            _ => null
        };

        if (embeddedFont != null)
        {
            using var stream = assembly.GetManifestResourceStream(embeddedFont);
            if (stream != null)
            {
                return SKTypeface.FromStream(stream);
            }
        }

        // Fall back to system font
        return SKTypeface.FromFamilyName(fontFamily)
            ?? throw new InvalidOperationException($"Could not load font: {fontFamily}");
    }
}
