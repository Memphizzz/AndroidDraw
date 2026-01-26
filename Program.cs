using AndroidDraw;

var options = CommandLineOptions.Parse(args);

if (options.ShowHelp || string.IsNullOrEmpty(options.Text) || !options.HasRequiredCoordinates)
{
    CommandLineOptions.PrintUsage();
    return options.ShowHelp ? 0 : 1;
}

// SimpleFont is already single-stroke, offset pass would just create artifacts
if (options.UseSimpleFont)
    options.ThinLines = true;

Console.WriteLine($"Text: \"{options.Text}\"");
Console.WriteLine($"Font: {(options.UseSimpleFont ? "SimpleFont (built-in)" : options.FontFamily)}");
Console.WriteLine($"Target: ({options.TargetX}, {options.TargetY}) {options.TargetWidth}x{options.TargetHeight}");
Console.WriteLine($"Options:{(options.ThinLines ? " --thin" : "")}{(options.UseSimpleFont ? " --simple" : "")}{(options.WiggleChars.Length > 0 ? $" --wiggle \"{options.WiggleChars}\"" : "")} delay={options.StrokeDelay}ms{(options.DryRun ? " --dry-run" : "")}");
Console.WriteLine();

var coordinateTransformer = new CoordinateTransformer();
var adbController = new AdbController(options.DeviceSerial, options.DryRun, options.ThinLines, options.WiggleChars);

List<Stroke> strokes;
SkiaSharp.SKRect bounds;

if (options.UseSimpleFont)
{
    // Use built-in single-stroke font
    var singleStrokeFont = new SingleStrokeFont();
    strokes = singleStrokeFont.GetTextStrokes(options.Text);
    bounds = singleStrokeFont.GetTextBounds(options.Text);
}
else
{
    // Use real font outlines via SkiaSharp (default)
    // Process character by character to tag strokes with source character
    var fontExtractor = new FontPathExtractor();
    var bezierInterpolator = new BezierInterpolator();
    var pathToStrokes = new PathToStrokes(bezierInterpolator);
    var strokeSimplifier = new StrokeSimplifier();

    strokes = [];
    float xOffset = 0;
    foreach (char c in options.Text)
    {
        var charPath = fontExtractor.GetCharPath(c, options.FontFamily, xOffset);
        var charStrokes = pathToStrokes.Convert(charPath, c);
        charStrokes = strokeSimplifier.Simplify(charStrokes, epsilon: 1.5f);
        strokes.AddRange(charStrokes);
        xOffset += fontExtractor.GetCharWidth(c, options.FontFamily);
    }

    var fullPath = fontExtractor.GetTextPath(options.Text, options.FontFamily);
    bounds = fullPath.Bounds;
}

Console.WriteLine($"Strokes: {strokes.Count}");

// Transform coordinates to fit target area
var transformedStrokes = coordinateTransformer.Transform(
    strokes,
    bounds,
    options.TargetX!.Value,
    options.TargetY!.Value,
    options.TargetWidth!.Value,
    options.TargetHeight!.Value);

// Execute drawing via ADB
if (options.DryRun)
    Console.WriteLine("Dry run output:");
await adbController.DrawStrokes(transformedStrokes, options.StrokeDelay);
if (!options.DryRun)
    Console.WriteLine("\nDone!");

return 0;
