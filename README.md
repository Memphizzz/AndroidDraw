# AndroidDraw

**Draw monospace text on Android screens via ADB — because some apps only give you cursive fonts.**

| JetBrains Mono | IBM Plex Mono Thin |
|:--:|:--:|
| ![JetBrains Mono](docs/Screenshot%20Mono.jpg) | ![IBM Plex Mono](docs/Screenshot%20IBM.jpg) |

https://github.com/user-attachments/assets/5f2831d3-2e23-4196-b28c-4df313d306eb

https://github.com/user-attachments/assets/67f379cc-7564-4c77-96a0-98cef04b8987

## The Backstory

I wanted to put `Console.Beep();` on my [Lunar](https://lunar.app/) metal bank card. You know, as a dev joke.

One problem: Lunar's card designer only offers two fonts — both *cursive handwriting* styles. Code humor doesn't land when it looks like a wedding invitation.

But wait — there's a "free draw" option! You can hand-write anything you want. Surely I can just... draw monospace letters with my finger?

*Spoiler: I cannot. My handwriting is terrible.*

So naturally, the solution was to:
1. Extract glyph vector paths from fonts using SkiaSharp
2. Convert Bezier curves to line segments
3. Inject MotionEvents via ADB to simulate finger drawing
4. Fine-tune with optional offset passes and priming wiggles

**It worked.**

Somewhere in Denmark, a Lunar employee is staring at a moderation queue wondering how someone submitted perfect monospace code in a free-draw field that only supports finger painting.

## What It Does

AndroidDraw takes text and a font, extracts the vector paths, and "draws" them on your Android device's screen using ADB input injection. It's essentially a robot finger that can write in any font.

## Installation

### Prerequisites

- [.NET 10.0](https://dotnet.microsoft.com/download) or later
- [ADB](https://developer.android.com/studio/releases/platform-tools) installed and in your PATH
- Android device with USB debugging enabled

### Build

```bash
git clone https://github.com/Memphizzz/AndroidDraw.git
cd AndroidDraw
dotnet build -c Release
```

## Usage

```bash
dotnet run -c Release -- "Your text" --x <X> --y <Y> --width <W> --height <H>
```

### Options

| Option | Description | Required |
|--------|-------------|----------|
| `--x` | Target area X position (pixels from left) | Yes |
| `--y` | Target area Y position (pixels from top) | Yes |
| `--width` | Target area width to fit text into | Yes |
| `--height` | Target area height | Yes |
| `--font` | Font family: `JetBrains Mono` (default), `IBM`, `Hershey` | No |
| `--thin` | Skip offset pass for thinner lines | No |
| `--wiggle` | Characters that need priming wiggles (e.g. `";"`) | No |
| `--simple` | Use built-in geometric/LCD style font | No |
| `--delay` | Delay between strokes in ms (default: `50`) | No |
| `--device` | ADB device serial (for multiple devices) | No |
| `--dry-run` | Print commands without executing | No |

### Fonts

Three fonts are embedded:

- **JetBrains Mono** (default) — the classic programming font
- **IBM Plex Mono Thin** (`--font "IBM"`) — thinner strokes, good for line thickness requirements
- **Hershey Simplex** (`--font "Hershey"`) — classic single-stroke engraving font

### Example: The Lunar Card

These coordinates worked for pixel-perfect centering on the Lunar card designer (OnePlus Open, folded):

```bash
# Using IBM Plex Mono Thin for thinner lines (Lunar requires pen-weight strokes)
dotnet run -c Release -- "Console.Beep();" --font "IBM" --wiggle ";" --x 250 --y 1200 --width 620 --height 300
```

**Finding your coordinates:** Use `--dry-run` to preview, then test in a drawing app like Google Keep to dial in the position before attempting on the target app.

## How It Works

### 1. Font Path Extraction

Uses SkiaSharp to extract vector paths from font glyphs. The paths contain MoveTo, LineTo, QuadTo, and CubicTo segments that define each character's shape.

### 2. Bezier Interpolation

Quadratic and cubic Bezier curves are converted to line segments (30 segments per curve) for smooth drawing.

### 3. Stroke Simplification

Ramer-Douglas-Peucker algorithm reduces point count while preserving shape, making drawing faster without losing quality.

### 4. ADB MotionEvent Injection

Sends touch events to simulate finger drawing:

```bash
adb shell input motionevent DOWN x y
adb shell input motionevent MOVE x y
# ... more MOVE events along the path
adb shell input motionevent UP x y
```

### 5. Two-Pass Rendering (Optional)

Single-pixel strokes can be too thin for some drawing apps. By default, AndroidDraw runs two passes:

1. **Main pass** — draws the stroke at original coordinates
2. **Offset pass** — redraws at +1px diagonal (X+1, Y+1) to fill gaps

Use `--thin` to skip the offset pass for thinner lines.

### 6. Special Handling

- **Wake-up tap** — primes the touch system before drawing
- **Priming wiggles** — optional per-character wiggles to ensure stroke start registers (use `--wiggle "chars"`)
- **Spiral dots** — periods and small punctuation are drawn as filled circles (not taps)
- **Progress output** — shows batch progress during drawing

## Tips

- **Test coordinates first** — Use a drawing app like Google Keep to dial in your `--x`, `--y`, `--width`, `--height` before attempting on the target app
- **Line too thick?** — Try `--font "IBM"` with `--thin` for the thinnest possible strokes
- **Stroke start missing?** — Add that character to `--wiggle` (e.g., `--wiggle ";"`)

## Dependencies

Just one:

```xml
<PackageReference Include="SkiaSharp" Version="3.119.1" />
```

Three fonts are embedded as resources — no external font installation required.

## The Result

A metal bank card with `Console.Beep();` in monospace, achieved through:
- Font vector extraction
- Bezier curve interpolation
- ADB touch injection
- Sheer determination to make a dumb joke work

Worth it.

## License

MIT

## Acknowledgments

- [Claude](https://claude.ai/) (Anthropic) for pair-programming this entire thing
- [SkiaSharp](https://github.com/mono/SkiaSharp) for font path extraction
- [JetBrains Mono](https://www.jetbrains.com/lp/mono/) for being the best programming font
- [IBM Plex](https://www.ibm.com/plex/) for the thin variant that passed Lunar's guidelines
- Lunar Bank for having a free-draw option (and only cursive fonts)
- The Lunar moderation team employee who approved this and is probably still confused
