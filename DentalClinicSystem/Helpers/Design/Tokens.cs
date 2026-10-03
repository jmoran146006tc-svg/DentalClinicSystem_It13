using System.Drawing.Text;

namespace DentalClinicSystem.Helpers.Design;

public static class Palette
{
    public static Color Canvas { get; } = Color.FromArgb(0xF3, 0xF8, 0xF9);
    public static Color Surface { get; } = Color.White;
    public static Color SurfaceAlt { get; } = Color.FromArgb(0xF8, 0xFB, 0xFC);
    public static Color Line { get; } = Color.FromArgb(0xE3, 0xEA, 0xEF);
    public static Color LineStrong { get; } = Color.FromArgb(0xCF, 0xD9, 0xE0);
    public static Color Ink900 { get; } = Color.FromArgb(0x1B, 0x2B, 0x3A);
    public static Color Ink700 { get; } = Color.FromArgb(0x3C, 0x4D, 0x5D);
    public static Color Ink500 { get; } = Color.FromArgb(0x5B, 0x6B, 0x7B);
    public static Color Ink400 { get; } = Color.FromArgb(0x86, 0x96, 0xA6);
    public static Color BrandAccent { get; } = Color.FromArgb(0x42, 0xCA, 0xCF);
    public static Color Brand { get; } = Color.FromArgb(0x1A, 0x7F, 0x85);
    public static Color BrandHover { get; } = Color.FromArgb(0x15, 0x6A, 0x6F);
    public static Color BrandPressed { get; } = Color.FromArgb(0x11, 0x5A, 0x5E);
    public static Color BrandSoft { get; } = Color.FromArgb(0xE6, 0xF7, 0xF8);
    public static Color BrandSoftText { get; } = Color.FromArgb(0x0F, 0x5C, 0x61);
    public static Color SidebarBg => Surface;
    public static SemanticStyle Info { get; } = new(Color.FromArgb(0xE7, 0xF0, 0xFE), Color.FromArgb(0x1D, 0x4E, 0xD8));
    public static SemanticStyle Success { get; } = new(Color.FromArgb(0xE3, 0xF6, 0xEA), Color.FromArgb(0x15, 0x7F, 0x3D));
    public static SemanticStyle Neutral { get; } = new(Color.FromArgb(0xEC, 0xEF, 0xF3), Color.FromArgb(0x47, 0x55, 0x69));
    public static SemanticStyle Danger { get; } = new(Color.FromArgb(0xFD, 0xE8, 0xE8), Color.FromArgb(0xB2, 0x22, 0x22));
    public static SemanticStyle Warning { get; } = new(Color.FromArgb(0xFE, 0xF3, 0xC7), Color.FromArgb(0x92, 0x40, 0x0E));
    public static IReadOnlyList<SemanticStyle> AvatarColors { get; } = Array.AsReadOnly(new[] { Info, Success, Neutral, Warning, new SemanticStyle(BrandSoft, BrandSoftText) });
    public static Color WithAlpha(Color color, float opacity) => Color.FromArgb((int)(255 * Math.Clamp(opacity, 0, 1)), color);
}

public sealed record SemanticStyle(Color Background, Color Text);
public enum Semantic { Info, Success, Neutral, Danger, Warning }

public static class Space
{
    public const int Xs = 4, Sm = 8, Md = 12, Lg = 16, Xl = 24, Xxl = 32, Xxxl = 48;
    public static Padding Page => new(Xxl, Xl, Xxl, Xxl);
}

public static class Metrics
{
    public const int ControlHeight = 40, CompactHeight = 32, NavHeight = 44, SidebarWidth = 248;
    public const int FormWidth = 380, ControlRadius = 8, CardRadius = 12, IconSize = 20;
    public const int Border = 1, FocusRing = 2, StatusDot = 6, PressOffset = 1;
    public const int MinimumWidth = 1100, MinimumHeight = 700, DialogWidth = 480, DialogHeight = 320;
    public const int ToastWidth = 380, ToastHeight = 112, TooltipWidth = 320, KpiHeight = 144;
    public const int FieldHeight = 92, AlertHeight = 64, EmptyHeight = 176, SkeletonRowHeight = 36;
    public const int Slide = 24, ExitSlide = 12, Settle = 8, Shake = 6;
    public const int CacheLimit = 128, IdentityHeight = 56, GridHeaderHeight = 40;
    public const float IconStroke = 1.75f, IconGrid = 24, HighlightAlpha = .14f;
    public static Size MinimumWindow => new(MinimumWidth, MinimumHeight);
    public static int Scale(Control owner, int value) => (int)Math.Round(value * owner.DeviceDpi / 96d);
}

public static class Typography
{
    private static readonly string Family = ResolveFamily();
    public static Font Display { get; } = new(Family, 24, FontStyle.Bold);
    public static Font Title { get; } = new(Family, 18, FontStyle.Bold);
    public static Font Heading { get; } = new(Family, 13, FontStyle.Bold);
    public static Font Body { get; } = new(Family, 10);
    public static Font Label { get; } = new(Family, 9, FontStyle.Bold);
    public static Font Caption { get; } = new(Family, 8.5f);
    public static Font KpiNumber { get; } = new(Family, 26, FontStyle.Bold);
    public static string FamilyName => Family;

    private static string ResolveFamily()
    {
        using var installed = new InstalledFontCollection();
        return installed.Families.Any(f => f.Name == "Segoe UI Variable Text") ? "Segoe UI Variable Text" : "Segoe UI";
    }
}
