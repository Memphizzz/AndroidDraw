# Android Monospace Text Drawer - Handoff Document

## Goal

Build a C# console application that draws monospace text on an Android device's screen via ADB, simulating finger strokes. The use case is writing code-styled text (like `Console.Beep();`) in an app's free-draw text input field that only offers handwriting fonts.

## Problem Statement

The Lunar Bank app allows custom text on metal cards with a free-draw input, but only provides cursive/script fonts. Code jokes don't land when written in fancy handwriting. We want to programmatically draw text that *looks* like monospace code.

## Technical Approach

### 1. Extract Glyph Paths from Font

Use **SkiaSharp** to get vector paths for each character from a monospace font:

```csharp
using SkiaSharp;

var paint = new SKPaint
{
    Typeface = SKTypeface.FromFamilyName("Consolas"), // or JetBrains Mono, Fira Code
    TextSize = 100,
    IsAntialias = true,
    Style = SKPaintStyle.Stroke
};

using var path = paint.GetTextPath("Console.Beep();", 0, 0);
```

### 2. Iterate Path Segments

Use `SKPath.Iterator` to walk through the path:
- `MoveTo` - lift finger, move to new position
- `LineTo` - draw a line (single swipe)
- `QuadTo` / `CubicTo` - curves (need to interpolate into multiple small line segments)
- `Close` - close the current contour

### 3. Transform Coordinates

- Get the bounding box of the text
- Scale and translate to fit within target screen area
- User should be able to specify: target X, Y, width, height on screen

### 4. Send ADB Commands

For each stroke segment:

```bash
# Single point tap
adb shell input tap X Y

# Line/swipe
adb shell input swipe X1 Y1 X2 Y2 DURATION_MS
```

For curves, break them into small line segments and chain swipes.

### 5. Handle Multi-Stroke Characters

Many characters have multiple disconnected parts (like `i` has dot and stem, `;` has dot and comma). Each `MoveTo` after drawing indicates lifting the finger and starting a new stroke.

## Requirements

### NuGet Packages
- `SkiaSharp` - for font path extraction

### Prerequisites
- ADB installed and in PATH
- Android device connected via USB with USB debugging enabled
- Device unlocked and app open with the draw field visible

### Command Line Interface

```bash
# Basic usage
MonospaceDraw.exe "Console.Beep();"

# With options
MonospaceDraw.exe "Console.Beep();" --font "JetBrains Mono" --x 150 --y 800 --width 800 --height 200 --speed 50
```

### Options
- `--font` - Font family name (default: "Consolas")
- `--x` - Target area X position on screen (default: auto-center or specified)
- `--y` - Target area Y position on screen
- `--width` - Target area width to fit text into
- `--height` - Target area height
- `--speed` - Swipe duration in ms, lower = faster (default: 30)
- `--delay` - Delay between strokes in ms (default: 50)
- `--dry-run` - Print ADB commands without executing
- `--device` - ADB device serial (for multiple devices)

## Implementation Notes

### Curve Interpolation

For `QuadTo` and `CubicTo`, implement Bézier interpolation:

```csharp
// Quadratic Bézier: P = (1-t)²P0 + 2(1-t)tP1 + t²P2
// Cubic Bézier: P = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3

// Sample at regular intervals (e.g., 10 points per curve)
// Convert to series of short line segments
```

### Stroke Batching

Instead of calling `adb shell` for every tiny segment, consider:
1. Batching multiple commands
2. Using `adb shell` with a script
3. Keeping reasonable segment lengths (don't over-interpolate)

### Coordinate System

- Android screen coordinates: (0,0) at top-left
- SkiaSharp text paths: (0,0) at baseline-left, Y increases downward
- Need to flip Y and translate to fit target area

### Testing

1. First test with `--dry-run` to see generated commands
2. Test on a simple drawing app first before the actual Lunar app
3. Start with simple text like "Hi" before full `Console.Beep();`

## Project Structure

```
MonospaceDraw/
├── MonospaceDraw.csproj
├── Program.cs              # Entry point, CLI parsing
├── FontPathExtractor.cs    # SkiaSharp font path logic
├── PathToStrokes.cs        # Convert SKPath to stroke segments
├── AdbController.cs        # Execute ADB commands
├── CoordinateTransformer.cs # Scale/translate coordinates
└── BezierInterpolator.cs   # Curve to line segments
```

## Success Criteria

Running `MonospaceDraw.exe "Console.Beep();" --x 150 --y 550 --width 800 --height 200` should draw legible monospace-style text on the Android screen that looks like actual code, not handwriting.

## Stretch Goals

- Preview window showing what will be drawn before sending to device
- Support for multiple lines
- Adjust stroke thickness by varying swipe speed
- Save/load coordinate presets for specific apps
