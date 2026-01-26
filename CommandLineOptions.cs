namespace AndroidDraw;

public class CommandLineOptions
{
    public string Text { get; set; } = string.Empty;
    public string FontFamily { get; set; } = "JetBrains Mono";
    public float? TargetX { get; set; }
    public float? TargetY { get; set; }
    public float? TargetWidth { get; set; }
    public float? TargetHeight { get; set; }
    public int StrokeDelay { get; set; } = 50;
    public bool DryRun { get; set; }
    public string DeviceSerial { get; set; } = string.Empty;
    public bool ShowHelp { get; set; }
    public bool UseSimpleFont { get; set; }
    public bool ThinLines { get; set; }
    public string WiggleChars { get; set; } = string.Empty;

    public static CommandLineOptions Parse(string[] args)
    {
        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--help" or "-h":
                    options.ShowHelp = true;
                    break;
                case "--font" when i + 1 < args.Length:
                    options.FontFamily = args[++i];
                    break;
                case "--x" when i + 1 < args.Length:
                    options.TargetX = float.Parse(args[++i]);
                    break;
                case "--y" when i + 1 < args.Length:
                    options.TargetY = float.Parse(args[++i]);
                    break;
                case "--width" when i + 1 < args.Length:
                    options.TargetWidth = float.Parse(args[++i]);
                    break;
                case "--height" when i + 1 < args.Length:
                    options.TargetHeight = float.Parse(args[++i]);
                    break;
                case "--delay" when i + 1 < args.Length:
                    options.StrokeDelay = int.Parse(args[++i]);
                    break;
                case "--dry-run":
                    options.DryRun = true;
                    break;
                case "--simple":
                    options.UseSimpleFont = true;
                    break;
                case "--thin":
                    options.ThinLines = true;
                    break;
                case "--wiggle" when i + 1 < args.Length:
                    options.WiggleChars = args[++i];
                    break;
                case "--device" when i + 1 < args.Length:
                    options.DeviceSerial = args[++i];
                    break;
                default:
                    if (!arg.StartsWith('-'))
                        options.Text = arg;
                    break;
            }
        }

        return options;
    }

    public bool HasRequiredCoordinates => TargetX.HasValue && TargetY.HasValue && TargetWidth.HasValue && TargetHeight.HasValue;

    public static void PrintUsage()
    {
        Console.WriteLine("""
            AndroidDraw - Draw monospace text on Android via ADB touch simulation

            Usage: AndroidDraw <text> --x <X> --y <Y> --width <W> --height <H> [options]

            Arguments:
              <text>              The text to draw

            Required:
              --x <value>         Target area X position (pixels from left)
              --y <value>         Target area Y position (pixels from top)
              --width <value>     Target area width to fit text into
              --height <value>    Target area height

            Options:
              --font <name>       Font family name (default: "JetBrains Mono", also: "Hershey", "IBM")
              --thin              Skip offset pass for thinner lines
              --simple            Use built-in geometric/LCD style font
              --wiggle <chars>    Characters that need priming wiggles (e.g. ";()")
              --delay <ms>        Delay between strokes in ms (default: 50)
              --dry-run           Print ADB commands without executing
              --device <serial>   ADB device serial (for multiple devices)
              -h, --help          Show this help message

            Examples:
              # Draw on Lunar card designer (OnePlus Open, folded) with IBM thin font
              AndroidDraw "Console.Beep();" --font "IBM" --wiggle ";" --x 250 --y 1200 --width 620 --height 300

              # Preview commands without executing
              AndroidDraw "Test" --x 100 --y 500 --width 800 --height 200 --dry-run
            """);
    }
}
