using SkiaSharp;

namespace AndroidDraw;

/// <summary>
/// Simple single-stroke monospace font using angular/geometric shapes.
/// Designed for maximum legibility when drawn with disconnected swipes.
/// </summary>
public class SingleStrokeFont
{
    public const float CharWidth = 50f;
    public const float CharHeight = 80f;
    public const float CharSpacing = 15f;

    // Grid positions (5-segment style like LCD displays)
    private const float L = 0f;      // Left
    private const float R = 50f;     // Right
    private const float C = 25f;     // Center X
    private const float T = 0f;      // Top
    private const float M = 40f;     // Middle
    private const float B = 80f;     // Bottom
    private const float U = 55f;     // Upper-middle (for lowercase)

    private static readonly Dictionary<char, List<List<SKPoint>>> _glyphs = new()
    {
        ['C'] = [
            [new(R-5, T+10), new(R-12, T+3), new(C, T), new(L+10, T+5), new(L+3, T+15),
             new(L, M),
             new(L+3, B-15), new(L+10, B-5), new(C, B), new(R-12, B-3), new(R-5, B-10)]
        ],
        ['D'] = [
            [new(L+5, T), new(R-12, T), new(R-5, T+10), new(R-2, M), new(R-5, B-10), new(R-12, B), new(L+5, B), new(L+5, T)]
        ],
        ['E'] = [
            [new(R-5, T), new(L+5, T), new(L+5, B), new(R-5, B)],
            [new(L+5, M), new(R-10, M)]
        ],
        ['F'] = [
            [new(R-5, T), new(L+5, T), new(L+5, B)],
            [new(L+5, M), new(R-10, M)]
        ],
        ['G'] = [
            [new(R-5, T+10), new(R-12, T+3), new(C, T), new(L+10, T+5), new(L+3, T+15),
             new(L, M),
             new(L+3, B-15), new(L+10, B-5), new(C, B), new(R-5, B), new(R-2, B-10), new(R-2, M), new(C, M)]
        ],
        ['H'] = [
            [new(L+5, T), new(L+5, B)],
            [new(R-5, T), new(R-5, B)],
            [new(L+5, M), new(R-5, M)]
        ],
        ['I'] = [
            [new(L+12, T), new(R-12, T)],
            [new(C, T), new(C, B)],
            [new(L+12, B), new(R-12, B)]
        ],
        ['J'] = [
            [new(L+10, T), new(R-5, T), new(R-5, B-15), new(R-12, B-3), new(C, B), new(L+10, B-5), new(L+5, B-15)]
        ],
        ['K'] = [
            [new(L+5, T), new(L+5, B)],
            [new(R-3, T), new(L+5, M+5)],
            [new(L+12, M-2), new(R-3, B)]
        ],
        ['L'] = [
            [new(L+5, T), new(L+5, B), new(R-5, B)]
        ],
        ['M'] = [
            [new(L+3, B), new(L+3, T+5), new(C, M-5), new(R-3, T+5), new(R-3, B)]
        ],
        ['N'] = [
            [new(L+5, B), new(L+5, T+3), new(R-5, B-3), new(R-5, T)]
        ],
        ['O'] = [
            [new(C, T), new(L+10, T+5), new(L+3, T+15), new(L, M), new(L+3, B-15), new(L+10, B-5),
             new(C, B), new(R-10, B-5), new(R-3, B-15), new(R, M), new(R-3, T+15), new(R-10, T+5), new(C, T)]
        ],
        ['P'] = [
            [new(L+5, B), new(L+5, T), new(R-8, T), new(R-2, T+10), new(R-2, M-8), new(R-8, M), new(L+5, M)]
        ],
        ['Q'] = [
            [new(C, T), new(L+10, T+5), new(L+3, T+15), new(L, M), new(L+3, B-15), new(L+10, B-5),
             new(C, B), new(R-10, B-5), new(R-3, B-15), new(R, M), new(R-3, T+15), new(R-10, T+5), new(C, T)],
            [new(C+5, B-15), new(R, B+5)]
        ],
        ['R'] = [
            [new(L+5, B), new(L+5, T), new(R-8, T), new(R-2, T+10), new(R-2, M-8), new(R-8, M), new(L+5, M)],
            [new(C-5, M), new(R-3, B)]
        ],
        ['S'] = [
            [new(R-5, T+8), new(R-10, T+2), new(C, T), new(L+10, T+3), new(L+3, T+12),
             new(L+3, M-8), new(L+10, M-2), new(R-10, M+2), new(R-3, M+8),
             new(R-3, B-12), new(R-10, B-3), new(C, B), new(L+10, B-2), new(L+5, B-8)]
        ],
        ['T'] = [
            [new(L, T), new(R, T)],
            [new(C, T), new(C, B)]
        ],
        ['U'] = [
            [new(L+5, T), new(L+5, B-15), new(L+12, B-3), new(C, B), new(R-12, B-3), new(R-5, B-15), new(R-5, T)]
        ],
        ['V'] = [
            [new(L+2, T), new(C, B-3), new(R-2, T)]
        ],
        ['W'] = [
            [new(L, T), new(L+12, B-3), new(C, M), new(R-12, B-3), new(R, T)]
        ],
        ['X'] = [
            [new(L+3, T), new(R-3, B)],
            [new(R-3, T), new(L+3, B)]
        ],
        ['Y'] = [
            [new(L+2, T), new(C, M)],
            [new(R-2, T), new(C, M), new(C, B)]
        ],
        ['Z'] = [
            [new(L+3, T), new(R-3, T), new(L+3, B), new(R-3, B)]
        ],

        // === LOWERCASE (JetBrains Mono has tall x-height) ===
        ['a'] = [
            [new(R-5, U-5), new(R-5, B)],
            [new(R-5, U-5), new(R-12, U-12), new(C, U-15), new(L+10, U-10), new(L+5, U),
             new(L+5, B-10), new(L+12, B-2), new(C, B), new(R-5, B-8)]
        ],
        ['b'] = [
            [new(L+5, T), new(L+5, B)],
            [new(L+5, U-10), new(L+12, U-15), new(C, U-15), new(R-10, U-10), new(R-5, U),
             new(R-5, B-10), new(R-12, B-2), new(C, B), new(L+5, B-8)]
        ],
        ['c'] = [
            [new(R-5, U-8), new(R-12, U-14), new(C, U-15), new(L+10, U-10), new(L+5, U),
             new(L+5, B-10), new(L+12, B-2), new(C, B), new(R-12, B-2), new(R-5, B-8)]
        ],
        ['d'] = [
            [new(R-5, T), new(R-5, B)],
            [new(R-5, U-10), new(R-12, U-15), new(C, U-15), new(L+10, U-10), new(L+5, U),
             new(L+5, B-10), new(L+12, B-2), new(C, B), new(R-5, B-8)]
        ],
        ['e'] = [
            // e: crossbar, up around top, down and around bottom open
            [new(L+6, M+2), new(R-6, M+2),  // crossbar
             new(R-6, U-2), new(C, U-15), new(L+6, U-2),  // up and over top
             new(L+6, B-8), new(C, B), new(R-6, B-8)]  // down around bottom, open end
        ],
        ['f'] = [
            [new(R-8, T+5), new(R-15, T), new(C+3, T), new(C-3, T+8), new(C-3, B)],
            [new(L+8, U-10), new(R-8, U-10)]
        ],
        ['g'] = [
            [new(R-5, U-15), new(R-5, B+15), new(R-12, B+22), new(C, B+25), new(L+10, B+20)],
            [new(R-5, U-10), new(R-12, U-15), new(C, U-15), new(L+10, U-10), new(L+5, U),
             new(L+5, B-10), new(L+12, B-2), new(C, B), new(R-5, B-8)]
        ],
        ['h'] = [
            [new(L+5, T), new(L+5, B)],
            [new(L+5, U-8), new(L+12, U-14), new(C, U-15), new(R-10, U-10), new(R-5, U), new(R-5, B)]
        ],
        ['i'] = [
            [new(C, U-10), new(C, B)],
            [new(C-4, T+8), new(C+4, T+8), new(C+4, T+16), new(C-4, T+16), new(C-4, T+8)]
        ],
        ['j'] = [
            [new(C+5, U-10), new(C+5, B+15), new(C-2, B+22), new(C-12, B+22)],
            [new(C+1, T+8), new(C+9, T+8), new(C+9, T+16), new(C+1, T+16), new(C+1, T+8)]
        ],
        ['k'] = [
            [new(L+5, T), new(L+5, B)],
            [new(R-5, U-12), new(L+5, M+8)],
            [new(L+15, M), new(R-5, B)]
        ],
        ['l'] = [
            [new(C-5, T), new(C-5, B-5), new(C, B), new(C+8, B)]
        ],
        ['m'] = [
            [new(L+2, B), new(L+2, U-10), new(L+8, U-15), new(C-5, U-12), new(C, U-5), new(C, B)],
            [new(C, U-10), new(C+8, U-15), new(R-8, U-12), new(R-2, U-5), new(R-2, B)]
        ],
        ['n'] = [
            [new(L+5, B), new(L+5, U-10), new(L+12, U-15), new(C, U-15), new(R-10, U-10), new(R-5, U), new(R-5, B)]
        ],
        ['o'] = [
            [new(C, U-15), new(L+10, U-10), new(L+5, U), new(L+5, B-10), new(L+12, B-2),
             new(C, B), new(R-12, B-2), new(R-5, B-10), new(R-5, U), new(R-10, U-10), new(C, U-15)]
        ],
        ['p'] = [
            [new(L+5, U-15), new(L+5, B+25)],
            [new(L+5, U-10), new(L+12, U-15), new(C, U-15), new(R-10, U-10), new(R-5, U),
             new(R-5, B-10), new(R-12, B-2), new(C, B), new(L+5, B-8)]
        ],
        ['q'] = [
            [new(R-5, U-15), new(R-5, B+25)],
            [new(R-5, U-10), new(R-12, U-15), new(C, U-15), new(L+10, U-10), new(L+5, U),
             new(L+5, B-10), new(L+12, B-2), new(C, B), new(R-5, B-8)]
        ],
        ['r'] = [
            [new(L+8, B), new(L+8, U-8), new(L+15, U-14), new(C, U-15), new(R-10, U-12), new(R-5, U-5)]
        ],
        ['s'] = [
            // s: simple S curve
            [new(R-5, U-8), new(C, U-15), new(L+5, U-8), new(L+5, M-8),
             new(C, M+2),
             new(R-5, M+12), new(R-5, B-8), new(C, B), new(L+5, B-8)]
        ],
        ['t'] = [
            [new(C-3, T+10), new(C-3, B-8), new(C+3, B-2), new(R-5, B)],
            [new(L+8, U-10), new(R-8, U-10)]
        ],
        ['u'] = [
            [new(L+5, U-15), new(L+5, B-10), new(L+12, B-2), new(C, B), new(R-10, B-5), new(R-5, B-12)],
            [new(R-5, U-15), new(R-5, B)]
        ],
        ['v'] = [
            [new(L+3, U-15), new(C, B-2), new(R-3, U-15)]
        ],
        ['w'] = [
            [new(L+2, U-15), new(L+12, B-2), new(C, U), new(R-12, B-2), new(R-2, U-15)]
        ],
        ['x'] = [
            [new(L+5, U-15), new(R-5, B)],
            [new(R-5, U-15), new(L+5, B)]
        ],
        ['y'] = [
            [new(L+5, U-15), new(C, B-5)],
            [new(R-5, U-15), new(C, B-5), new(L+10, B+20), new(L+5, B+22)]
        ],
        ['z'] = [
            [new(L+5, U-15), new(R-5, U-15), new(L+5, B), new(R-5, B)]
        ],

        // === NUMBERS ===
        ['0'] = [
            [new(C, T), new(L+10, T+5), new(L+3, T+15), new(L, M), new(L+3, B-15), new(L+10, B-5),
             new(C, B), new(R-10, B-5), new(R-3, B-15), new(R, M), new(R-3, T+15), new(R-10, T+5), new(C, T)]
        ],
        ['1'] = [
            [new(L+10, T+12), new(C, T), new(C, B)],
            [new(L+8, B), new(R-8, B)]
        ],
        ['2'] = [
            [new(L+5, T+12), new(L+10, T+3), new(C, T), new(R-10, T+3), new(R-5, T+15),
             new(R-5, M-5), new(L+5, B), new(R-5, B)]
        ],
        ['3'] = [
            [new(L+5, T+8), new(L+12, T+2), new(C, T), new(R-10, T+5), new(R-5, T+15),
             new(R-5, M-8), new(R-12, M), new(C, M)],
            [new(C, M), new(R-12, M), new(R-5, M+8), new(R-5, B-15), new(R-10, B-5),
             new(C, B), new(L+12, B-2), new(L+5, B-8)]
        ],
        ['4'] = [
            [new(L+5, T), new(L+5, M), new(R-5, M)],
            [new(R-5, T), new(R-5, B)]
        ],
        ['5'] = [
            [new(R-5, T), new(L+5, T), new(L+5, M-5), new(C, M-8), new(R-10, M-3), new(R-5, M+5),
             new(R-5, B-12), new(R-10, B-3), new(C, B), new(L+10, B-3), new(L+5, B-10)]
        ],
        ['6'] = [
            [new(R-8, T+5), new(C, T), new(L+10, T+5), new(L+5, T+15), new(L+5, B-15),
             new(L+10, B-3), new(C, B), new(R-10, B-3), new(R-5, B-12), new(R-5, M+5),
             new(R-10, M-3), new(C, M-5), new(L+5, M)]
        ],
        ['7'] = [
            [new(L+5, T), new(R-5, T), new(C, B)]
        ],
        ['8'] = [
            [new(C, T), new(L+12, T+5), new(L+8, T+15), new(L+8, M-8), new(L+12, M-2), new(C, M)],
            [new(C, T), new(R-12, T+5), new(R-8, T+15), new(R-8, M-8), new(R-12, M-2), new(C, M)],
            [new(C, M), new(L+10, M+3), new(L+5, M+12), new(L+5, B-12), new(L+10, B-3), new(C, B)],
            [new(C, M), new(R-10, M+3), new(R-5, M+12), new(R-5, B-12), new(R-10, B-3), new(C, B)]
        ],
        ['9'] = [
            [new(L+8, B-5), new(C, B), new(R-10, B-5), new(R-5, B-15), new(R-5, T+15),
             new(R-10, T+3), new(C, T), new(L+10, T+3), new(L+5, T+12), new(L+5, M-5),
             new(L+10, M+3), new(C, M+5), new(R-5, M)]
        ],

        // === PUNCTUATION ===
        ['.'] = [
            [new(C-4, B-8), new(C+4, B-8), new(C+4, B), new(C-4, B), new(C-4, B-8)]
        ],
        [','] = [
            [new(C+2, B-8), new(C+2, B), new(C-5, B+12)]
        ],
        [';'] = [
            [new(C, M-10), new(C+5, M-5), new(C, M), new(C-5, M-5), new(C, M-10)],
            [new(C, B-10), new(C, B-2), new(C-8, B+10)]
        ],
        [':'] = [
            [new(C-4, U-5), new(C+4, U-5), new(C+4, U+3), new(C-4, U+3), new(C-4, U-5)],
            [new(C-4, B-8), new(C+4, B-8), new(C+4, B), new(C-4, B), new(C-4, B-8)]
        ],
        ['!'] = [
            [new(C, T), new(C, M+10)],
            [new(C-4, B-8), new(C+4, B-8), new(C+4, B), new(C-4, B), new(C-4, B-8)]
        ],
        ['?'] = [
            [new(L+5, T+12), new(L+10, T+3), new(C, T), new(R-10, T+3), new(R-5, T+15),
             new(R-5, M-5), new(C, M+5), new(C, M+18)],
            [new(C-4, B-8), new(C+4, B-8), new(C+4, B), new(C-4, B), new(C-4, B-8)]
        ],
        ['('] = [
            [new(C+8, T), new(C+3, T+5), new(C-2, T+15), new(C-5, T+30), new(C-5, M),
             new(C-5, B-30), new(C-2, B-15), new(C+3, B-5), new(C+8, B)]
        ],
        [')'] = [
            [new(C-8, T), new(C-3, T+5), new(C+2, T+15), new(C+5, T+30), new(C+5, M),
             new(C+5, B-30), new(C+2, B-15), new(C-3, B-5), new(C-8, B)]
        ],
        ['['] = [
            [new(R-10, T), new(L+10, T), new(L+10, B), new(R-10, B)]
        ],
        [']'] = [
            [new(L+10, T), new(R-10, T), new(R-10, B), new(L+10, B)]
        ],
        ['{'] = [
            [new(R-8, T), new(C+5, T), new(C, T+8), new(C, M-8), new(C-8, M), new(C, M+8), new(C, B-8), new(C+5, B), new(R-8, B)]
        ],
        ['}'] = [
            [new(L+8, T), new(C-5, T), new(C, T+8), new(C, M-8), new(C+8, M), new(C, M+8), new(C, B-8), new(C-5, B), new(L+8, B)]
        ],
        ['"'] = [
            [new(C-12, T+3), new(C-8, T), new(C-8, T+18)],
            [new(C+8, T+3), new(C+12, T), new(C+12, T+18)]
        ],
        ['\''] = [
            [new(C-2, T+3), new(C+2, T), new(C+2, T+18)]
        ],
        ['-'] = [
            [new(L+8, M), new(R-8, M)]
        ],
        ['_'] = [
            [new(L, B), new(R, B)]
        ],
        ['+'] = [
            [new(L+8, M), new(R-8, M)],
            [new(C, M-20), new(C, M+20)]
        ],
        ['='] = [
            [new(L+8, M-8), new(R-8, M-8)],
            [new(L+8, M+8), new(R-8, M+8)]
        ],
        ['<'] = [
            [new(R-8, T+15), new(L+8, M), new(R-8, B-15)]
        ],
        ['>'] = [
            [new(L+8, T+15), new(R-8, M), new(L+8, B-15)]
        ],
        ['/'] = [
            [new(R-5, T), new(L+5, B)]
        ],
        ['\\'] = [
            [new(L+5, T), new(R-5, B)]
        ],
        ['|'] = [
            [new(C, T), new(C, B)]
        ],
        ['*'] = [
            [new(C, M-12), new(C, M+12)],
            [new(L+8, M-8), new(R-8, M+8)],
            [new(R-8, M-8), new(L+8, M+8)]
        ],
        ['&'] = [
            [new(R-3, B), new(C, M), new(C+8, T+15), new(C, T+5), new(C-8, T+15), new(L+5, M),
             new(L+5, B-12), new(L+12, B-3), new(C+5, B), new(R-5, M+10)]
        ],
        ['@'] = [
            [new(R-5, M+5), new(C+5, M), new(C+5, B-15), new(R-8, B-15), new(R-5, M),
             new(R-8, T+8), new(C, T), new(L+8, T+8), new(L+5, M), new(L+8, B-8), new(C, B), new(R-3, B-5)]
        ],
        ['#'] = [
            [new(L+12, T), new(L+8, B)],
            [new(R-8, T), new(R-12, B)],
            [new(L+3, M-10), new(R-3, M-10)],
            [new(L+3, M+10), new(R-3, M+10)]
        ],
        ['$'] = [
            [new(R-6, T+12), new(R-12, T+5), new(C, T+3), new(L+10, T+8), new(L+6, T+18),
             new(L+8, M-2), new(R-8, M+2), new(R-6, B-15), new(R-10, B-5), new(C, B-3), new(L+10, B-5), new(L+6, B-12)],
            [new(C, T), new(C, B)]
        ],
        ['%'] = [
            [new(R-5, T), new(L+5, B)],
            [new(L+5, T+5), new(L+15, T+5), new(L+15, T+15), new(L+5, T+15), new(L+5, T+5)],
            [new(R-15, B-15), new(R-5, B-15), new(R-5, B-5), new(R-15, B-5), new(R-15, B-15)]
        ],
        ['^'] = [
            [new(L+8, M-5), new(C, T+8), new(R-8, M-5)]
        ],
        ['~'] = [
            [new(L+3, M+3), new(L+15, M-5), new(R-15, M+5), new(R-3, M-3)]
        ],
        ['`'] = [
            [new(L+12, T), new(C, T+12)]
        ],

        // === UPPERCASE ===
        ['A'] = [
            [new(L, B), new(L, M), new(C, T), new(R, M), new(R, B)],  // outer shape
            [new(L, M), new(R, M)]  // crossbar
        ],
        ['B'] = [
            [new(L, B), new(L, T), new(R, T), new(R, M), new(L, M)],  // top half
            [new(L, M), new(R, M), new(R, B), new(L, B)]  // bottom half
        ],
        ['C'] = [
            [new(R, T), new(L, T), new(L, B), new(R, B)]
        ],
        ['D'] = [
            [new(L, T), new(R, T), new(R, B), new(L, B), new(L, T)]
        ],
        ['E'] = [
            [new(R, T), new(L, T), new(L, B), new(R, B)],
            [new(L, M), new(R - 10, M)]
        ],
        ['F'] = [
            [new(R, T), new(L, T), new(L, B)],
            [new(L, M), new(R - 10, M)]
        ],
        ['G'] = [
            [new(R, T), new(L, T), new(L, B), new(R, B), new(R, M), new(C, M)]
        ],
        ['H'] = [
            [new(L, T), new(L, B)],
            [new(R, T), new(R, B)],
            [new(L, M), new(R, M)]
        ],
        ['I'] = [
            [new(L + 10, T), new(R - 10, T)],
            [new(C, T), new(C, B)],
            [new(L + 10, B), new(R - 10, B)]
        ],
        ['J'] = [
            [new(L, T), new(R, T)],
            [new(C, T), new(C, B), new(L, B), new(L, M)]
        ],
        ['K'] = [
            [new(L, T), new(L, B)],
            [new(R, T), new(L, M), new(R, B)]
        ],
        ['L'] = [
            [new(L, T), new(L, B), new(R, B)]
        ],
        ['M'] = [
            [new(L, B), new(L, T), new(C, M), new(R, T), new(R, B)]
        ],
        ['N'] = [
            [new(L, B), new(L, T), new(R, B), new(R, T)]
        ],
        ['O'] = [
            [new(L, T), new(R, T), new(R, B), new(L, B), new(L, T)]
        ],
        ['P'] = [
            [new(L, B), new(L, T), new(R, T), new(R, M), new(L, M)]
        ],
        ['Q'] = [
            [new(L, T), new(R, T), new(R, B), new(L, B), new(L, T)],
            [new(C, M), new(R + 5, B + 5)]
        ],
        ['R'] = [
            [new(L, B), new(L, T), new(R, T), new(R, M), new(L, M)],
            [new(C, M), new(R, B)]
        ],
        ['S'] = [
            [new(R, T), new(L, T), new(L, M), new(R, M), new(R, B), new(L, B)]
        ],
        ['T'] = [
            [new(L, T), new(R, T)],
            [new(C, T), new(C, B)]
        ],
        ['U'] = [
            [new(L, T), new(L, B), new(R, B), new(R, T)]
        ],
        ['V'] = [
            [new(L, T), new(C, B), new(R, T)]
        ],
        ['W'] = [
            [new(L, T), new(L, B), new(C, M), new(R, B), new(R, T)]
        ],
        ['X'] = [
            [new(L, T), new(R, B)],
            [new(R, T), new(L, B)]
        ],
        ['Y'] = [
            [new(L, T), new(C, M), new(R, T)],
            [new(C, M), new(C, B)]
        ],
        ['Z'] = [
            [new(L, T), new(R, T), new(L, B), new(R, B)]
        ],

        // === LOWERCASE (simplified, same height as uppercase for consistency) ===
        ['a'] = [
            [new(L, U), new(R, U), new(R, B), new(L, B), new(L, M), new(R, M)]
        ],
        ['b'] = [
            [new(L, T), new(L, B), new(R, B), new(R, U), new(L, U)]
        ],
        ['c'] = [
            [new(R, U), new(L, U), new(L, B), new(R, B)]
        ],
        ['d'] = [
            [new(R, T), new(R, B), new(L, B), new(L, U), new(R, U)]
        ],
        ['e'] = [
            [new(L, M), new(R, M), new(R, U), new(L, U), new(L, B), new(R, B)]
        ],
        ['f'] = [
            [new(R, T + 10), new(C, T), new(C, B)],
            [new(L + 5, U), new(R - 5, U)]
        ],
        ['g'] = [
            [new(R, U), new(L, U), new(L, B), new(R, B), new(R, B + 20), new(L, B + 20)]
        ],
        ['h'] = [
            [new(L, T), new(L, B)],
            [new(L, U), new(R, U), new(R, B)]
        ],
        ['i'] = [
            [new(C, U), new(C, B)],
            [new(C - 5, T + 10), new(C + 5, T + 10)]  // dot
        ],
        ['j'] = [
            [new(C, U), new(C, B + 15), new(L, B + 15)],
            [new(C - 5, T + 10), new(C + 5, T + 10)]  // dot
        ],
        ['k'] = [
            [new(L, T), new(L, B)],
            [new(R, U), new(L, M), new(R, B)]
        ],
        ['l'] = [
            [new(C, T), new(C, B)]
        ],
        ['m'] = [
            [new(L, B), new(L, U), new(C, U), new(C, B)],
            [new(C, U), new(R, U), new(R, B)]
        ],
        ['n'] = [
            [new(L, B), new(L, U), new(R, U), new(R, B)]
        ],
        ['o'] = [
            [new(L, U), new(R, U), new(R, B), new(L, B), new(L, U)]
        ],
        ['p'] = [
            [new(L, B + 20), new(L, U), new(R, U), new(R, B), new(L, B)]
        ],
        ['q'] = [
            [new(R, B + 20), new(R, U), new(L, U), new(L, B), new(R, B)]
        ],
        ['r'] = [
            [new(L, B), new(L, U), new(R, U)]
        ],
        ['s'] = [
            [new(R, U), new(L, U), new(L, M), new(R, M), new(R, B), new(L, B)]
        ],
        ['t'] = [
            [new(C, T), new(C, B), new(R, B)],
            [new(L + 5, U), new(R - 5, U)]
        ],
        ['u'] = [
            [new(L, U), new(L, B), new(R, B), new(R, U)]
        ],
        ['v'] = [
            [new(L, U), new(C, B), new(R, U)]
        ],
        ['w'] = [
            [new(L, U), new(L, B), new(C, M), new(R, B), new(R, U)]
        ],
        ['x'] = [
            [new(L, U), new(R, B)],
            [new(R, U), new(L, B)]
        ],
        ['y'] = [
            [new(L, U), new(C, M)],
            [new(R, U), new(L, B + 20)]
        ],
        ['z'] = [
            [new(L, U), new(R, U), new(L, B), new(R, B)]
        ],

        // === NUMBERS ===
        ['0'] = [[new(L, T), new(R, T), new(R, B), new(L, B), new(L, T)]],
        ['1'] = [[new(L + 10, T + 15), new(C, T), new(C, B)], [new(L + 5, B), new(R - 5, B)]],
        ['2'] = [[new(L, T), new(R, T), new(R, M), new(L, M), new(L, B), new(R, B)]],
        ['3'] = [[new(L, T), new(R, T), new(R, B), new(L, B)], [new(C, M), new(R, M)]],
        ['4'] = [[new(L, T), new(L, M), new(R, M)], [new(R, T), new(R, B)]],
        ['5'] = [[new(R, T), new(L, T), new(L, M), new(R, M), new(R, B), new(L, B)]],
        ['6'] = [[new(R, T), new(L, T), new(L, B), new(R, B), new(R, M), new(L, M)]],
        ['7'] = [[new(L, T), new(R, T), new(C, B)]],
        ['8'] = [[new(L, T), new(R, T), new(R, B), new(L, B), new(L, T)], [new(L, M), new(R, M)]],
        ['9'] = [[new(L, B), new(R, B), new(R, T), new(L, T), new(L, M), new(R, M)]],

        // === PUNCTUATION ===
        ['.'] = [[new(C - 3, B - 5), new(C + 3, B - 5)]],
        [','] = [[new(C, B - 5), new(C - 8, B + 10)]],
        [';'] = [[new(C - 5, M), new(C + 5, M)], [new(C, B - 5), new(C - 8, B + 10)]],
        [':'] = [[new(C - 5, U), new(C + 5, U)], [new(C - 5, B - 5), new(C + 5, B - 5)]],
        ['!'] = [[new(C, T), new(C, M + 10)], [new(C - 3, B - 5), new(C + 3, B - 5)]],
        ['?'] = [[new(L, T + 10), new(L, T), new(R, T), new(R, M), new(C, M), new(C, M + 15)], [new(C - 3, B - 5), new(C + 3, B - 5)]],
        ['('] = [[new(R - 5, T), new(L + 10, M), new(R - 5, B)]],
        [')'] = [[new(L + 5, T), new(R - 10, M), new(L + 5, B)]],
        ['['] = [[new(R - 5, T), new(L + 5, T), new(L + 5, B), new(R - 5, B)]],
        [']'] = [[new(L + 5, T), new(R - 5, T), new(R - 5, B), new(L + 5, B)]],
        ['{'] = [[new(R - 5, T), new(C, T), new(C, M - 5), new(L + 5, M), new(C, M + 5), new(C, B), new(R - 5, B)]],
        ['}'] = [[new(L + 5, T), new(C, T), new(C, M - 5), new(R - 5, M), new(C, M + 5), new(C, B), new(L + 5, B)]],
        ['"'] = [[new(C - 10, T), new(C - 10, T + 15)], [new(C + 10, T), new(C + 10, T + 15)]],
        ['\''] = [[new(C, T), new(C, T + 15)]],
        ['-'] = [[new(L + 5, M), new(R - 5, M)]],
        ['_'] = [[new(L, B), new(R, B)]],
        ['+'] = [[new(L + 5, M), new(R - 5, M)], [new(C, M - 20), new(C, M + 20)]],
        ['='] = [[new(L + 5, M - 10), new(R - 5, M - 10)], [new(L + 5, M + 10), new(R - 5, M + 10)]],
        ['<'] = [[new(R - 5, T + 10), new(L + 5, M), new(R - 5, B - 10)]],
        ['>'] = [[new(L + 5, T + 10), new(R - 5, M), new(L + 5, B - 10)]],
        ['/'] = [[new(R, T), new(L, B)]],
        ['\\'] = [[new(L, T), new(R, B)]],
        ['|'] = [[new(C, T), new(C, B)]],
        ['*'] = [[new(C, M - 15), new(C, M + 15)], [new(L + 5, M - 10), new(R - 5, M + 10)], [new(R - 5, M - 10), new(L + 5, M + 10)]],
        ['&'] = [[new(R, B), new(L, M), new(C, T), new(L, M), new(R, M), new(L, B)]],
        ['@'] = [[new(R - 5, M), new(C, M), new(C, B - 10), new(R - 5, B - 10), new(R - 5, T + 5), new(L + 5, T + 5), new(L + 5, B - 5), new(R, B - 5)]],
        ['#'] = [[new(L + 10, T), new(L + 10, B)], [new(R - 10, T), new(R - 10, B)], [new(L, M - 10), new(R, M - 10)], [new(L, M + 10), new(R, M + 10)]],
        ['$'] = [[new(R, T + 10), new(L, T + 10), new(L, M), new(R, M), new(R, B - 10), new(L, B - 10)], [new(C, T), new(C, B)]],
        ['%'] = [[new(R, T), new(L, B)], [new(L + 5, T + 5), new(L + 15, T + 5)], [new(R - 15, B - 5), new(R - 5, B - 5)]],
        ['^'] = [[new(L + 5, M - 10), new(C, T + 5), new(R - 5, M - 10)]],
        ['~'] = [[new(L, M + 5), new(L + 15, M - 5), new(R - 15, M + 5), new(R, M - 5)]],
        ['`'] = [[new(L + 10, T), new(C, T + 15)]],

        [' '] = []  // Space - no strokes
    };

    public List<Stroke> GetTextStrokes(string text)
    {
        var strokes = new List<Stroke>();
        float xOffset = 0;

        foreach (char c in text)
        {
            if (_glyphs.TryGetValue(c, out var glyphStrokes))
            {
                foreach (var strokePoints in glyphStrokes)
                {
                    var stroke = new Stroke { Character = c };
                    foreach (var point in strokePoints)
                    {
                        stroke.Points.Add(new SKPoint(point.X + xOffset, point.Y));
                    }
                    if (stroke.Points.Count > 0)
                        strokes.Add(stroke);
                }
            }

            xOffset += CharWidth + CharSpacing;
        }

        return strokes;
    }

    public SKRect GetTextBounds(string text)
    {
        float totalWidth = text.Length * (CharWidth + CharSpacing) - CharSpacing;
        return new SKRect(0, 0, Math.Max(totalWidth, 1), CharHeight);
    }
}
