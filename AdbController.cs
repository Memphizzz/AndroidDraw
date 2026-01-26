using System.Diagnostics;
using System.Globalization;

namespace AndroidDraw;

/// <summary>
/// Executes drawing commands on Android device via ADB motionevent.
/// </summary>
public class AdbController
{
    private const string AdbPath = @"C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe";
    private const int MaxCommandsPerBatch = 100;

    // Timing constants
    private const float MoveDelaySeconds = 0.02f;
    private const float TouchPrimeDelaySeconds = 0.05f;
    private const float TouchRegisterDelaySeconds = 0.1f;

    // Stroke detection
    private const float SmallStrokeThreshold = 15f;
    private const int DotRadius = 5;
    private const int DotAngleStep = 30;

    // Interpolation
    private const float PixelsPerMoveEvent = 2f;
    private const int OffsetPassPixels = 1;

    private readonly string _deviceSerial;
    private readonly bool _dryRun;
    private readonly bool _thinLines;
    private readonly string _wiggleChars;

    public AdbController(string deviceSerial, bool dryRun, bool thinLines = false, string wiggleChars = "")
    {
        _deviceSerial = deviceSerial;
        _dryRun = dryRun;
        _thinLines = thinLines;
        _wiggleChars = wiggleChars;
    }

    public async Task DrawStrokes(List<Stroke> strokes, int strokeDelay)
    {
        var commands = new List<string>();

        AddWakeUpTap(commands, strokes);

        foreach (var stroke in strokes)
        {
            if (stroke.Points.Count == 0)
                continue;

            var bounds = GetStrokeBounds(stroke);
            bool isSmallStroke = bounds.Width < SmallStrokeThreshold && bounds.Height < SmallStrokeThreshold;

            if (stroke.Points.Count == 1 || isSmallStroke)
            {
                AddDotCommands(commands, bounds, stroke.Character);
            }
            else
            {
                AddStrokeCommands(commands, stroke);
            }

            if (strokeDelay > 0)
            {
                commands.Add(FormatSleep(strokeDelay / 1000f));
            }
        }

        await ExecuteInBatches(commands);
    }

    private void AddWakeUpTap(List<string> commands, List<Stroke> strokes)
    {
        if (strokes.Count == 0 || strokes[0].Points.Count == 0)
            return;

        var point = strokes[0].Points[0];
        int x = (int)point.X;
        int y = (int)point.Y;

        commands.Add($"input motionevent DOWN {x} {y}");
        commands.Add(FormatSleep(TouchPrimeDelaySeconds));
        commands.Add($"input motionevent UP {x} {y}");
        commands.Add(FormatSleep(TouchRegisterDelaySeconds));
    }

    private void AddDotCommands(List<string> commands, StrokeBounds bounds, char character)
    {
        int centerX = (int)((bounds.MinX + bounds.MaxX) / 2);
        int centerY = (int)((bounds.MinY + bounds.MaxY) / 2);

        commands.Add($"input motionevent DOWN {centerX} {centerY}");
        commands.Add(FormatSleep(MoveDelaySeconds));

        if (_wiggleChars.Contains(character))
        {
            AddPrimingWiggles(commands, centerX, centerY);
        }

        // Draw filled circle by spiraling outward
        for (int radius = 1; radius <= DotRadius; radius += 2)
        {
            for (int angle = 0; angle < 360; angle += DotAngleStep)
            {
                int x = centerX + (int)(radius * MathF.Cos(angle * MathF.PI / 180));
                int y = centerY + (int)(radius * MathF.Sin(angle * MathF.PI / 180));
                commands.Add($"input motionevent MOVE {x} {y}");
                commands.Add(FormatSleep(MoveDelaySeconds));
            }
        }

        commands.Add($"input motionevent UP {centerX} {centerY}");
    }

    private void AddStrokeCommands(List<string> commands, Stroke stroke)
    {
        var firstPoint = stroke.Points[0];
        int firstX = (int)firstPoint.X;
        int firstY = (int)firstPoint.Y;

        // Touch down with priming wiggles to ensure first point registers
        commands.Add($"input motionevent DOWN {firstX} {firstY}");
        commands.Add(FormatSleep(TouchRegisterDelaySeconds));

        if (_wiggleChars.Contains(stroke.Character))
        {
            AddPrimingWiggles(commands, firstX, firstY);
        }

        // Main stroke pass
        AddInterpolatedPath(commands, stroke.Points, offsetX: 0, offsetY: 0);

        // Offset pass fills gaps left by thin brush (skip for --thin mode)
        if (!_thinLines)
        {
            AddInterpolatedPath(commands, stroke.Points, OffsetPassPixels, OffsetPassPixels);
        }

        var lastPoint = stroke.Points[^1];
        commands.Add($"input motionevent UP {(int)lastPoint.X} {(int)lastPoint.Y}");
    }

    private void AddPrimingWiggles(List<string> commands, int x, int y)
    {
        int[] offsets = [0, 1, -1];

        foreach (int dx in offsets)
        {
            commands.Add($"input motionevent MOVE {x + dx} {y}");
            commands.Add(FormatSleep(MoveDelaySeconds));
        }

        commands.Add($"input motionevent MOVE {x} {y + 1}");
        commands.Add(FormatSleep(MoveDelaySeconds));
        commands.Add($"input motionevent MOVE {x} {y - 1}");
        commands.Add(FormatSleep(MoveDelaySeconds));
        commands.Add($"input motionevent MOVE {x} {y}");
        commands.Add(FormatSleep(MoveDelaySeconds));
    }

    private void AddInterpolatedPath(List<string> commands, List<SkiaSharp.SKPoint> points, int offsetX, int offsetY)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            var from = points[i];
            var to = points[i + 1];

            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            int steps = Math.Max(1, (int)(distance / PixelsPerMoveEvent));

            for (int step = 1; step <= steps; step++)
            {
                float t = step / (float)steps;
                int x = (int)(from.X + dx * t) + offsetX;
                int y = (int)(from.Y + dy * t) + offsetY;
                commands.Add($"input motionevent MOVE {x} {y}");
                commands.Add(FormatSleep(MoveDelaySeconds));
            }
        }
    }

    private async Task ExecuteInBatches(List<string> commands)
    {
        int totalBatches = (commands.Count + MaxCommandsPerBatch - 1) / MaxCommandsPerBatch;
        int batchNum = 0;

        for (int i = 0; i < commands.Count; i += MaxCommandsPerBatch)
        {
            batchNum++;
            if (!_dryRun)
            {
                Console.Write($"\rDrawing... batch {batchNum}/{totalBatches}");
            }

            var batch = commands.Skip(i).Take(MaxCommandsPerBatch);
            var batchCommand = string.Join(" && ", batch);
            await ExecuteAdbShell(batchCommand);
        }
    }

    private static string FormatSleep(float seconds)
    {
        return string.Format(CultureInfo.InvariantCulture, "sleep {0:F3}", seconds);
    }

    private static StrokeBounds GetStrokeBounds(Stroke stroke)
    {
        if (stroke.Points.Count == 0)
            return new StrokeBounds();

        float minX = stroke.Points[0].X;
        float minY = stroke.Points[0].Y;
        float maxX = minX;
        float maxY = minY;

        foreach (var point in stroke.Points)
        {
            if (point.X < minX) minX = point.X;
            if (point.Y < minY) minY = point.Y;
            if (point.X > maxX) maxX = point.X;
            if (point.Y > maxY) maxY = point.Y;
        }

        return new StrokeBounds(minX, minY, maxX, maxY);
    }

    private async Task ExecuteAdbShell(string shellCommand)
    {
        var adbArgs = string.IsNullOrEmpty(_deviceSerial)
            ? $"shell \"{shellCommand}\""
            : $"-s {_deviceSerial} shell \"{shellCommand}\"";

        if (_dryRun)
        {
            Console.WriteLine($"adb {adbArgs}");
            Console.WriteLine();
            return;
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = AdbPath,
                Arguments = adbArgs,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            Console.Error.WriteLine($"ADB error: {error}");
        }
    }

    private readonly record struct StrokeBounds(float MinX, float MinY, float MaxX, float MaxY)
    {
        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;
    }
}
