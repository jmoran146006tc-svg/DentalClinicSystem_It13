using System.Drawing.Text;

namespace DentalClinicSystem.Helpers.Design;

public static class Palette
{
    public static Color Transparent { get; } = Color.Transparent;
    public static Color Black { get; } = Color.Black;
    public static Color Canvas { get; } = Color.FromArgb(0xF3, 0xF8, 0xF9);
    public static Color Surface { get; } = Color.White;
    public static Color SurfaceAlt { get; } = Color.FromArgb(0xF8, 0xFB, 0xFC);
    public static Color Line { get; } = Color.FromArgb(0xE3, 0xEA, 0xEF);
    public static Color LineStrong { get; } = Color.FromArgb(0xCF, 0xD9, 0xE0);
    public static Color Ink900 { get; } = Color.FromArgb(0x1B, 0x2B, 0x3A);
    public static Color Ink700 { get; } = Color.FromArgb(0x3C, 0x4D, 0x5D);
    public static Color Ink500 { get; } = Color.FromArgb(0x5B, 0x6B, 0x7B);
    public static Color Ink400 { get; } = Color.FromArgb(0x86, 0x96, 0xA6);
    public static Color Placeholder => Ink500;
    public static Color BrandAccent { get; } = Color.FromArgb(0x42, 0xCA, 0xCF);
    public static Color Brand { get; } = Color.FromArgb(0x1A, 0x7F, 0x85);
    public static Color BrandHover { get; } = Color.FromArgb(0x15, 0x6A, 0x6F);
    public static Color BrandPressed { get; } = Color.FromArgb(0x11, 0x5A, 0x5E);
    public static Color BrandSoft { get; } = Color.FromArgb(0xE6, 0xF7, 0xF8);
    public static Color BrandSoftText { get; } = Color.FromArgb(0x0F, 0x5C, 0x61);
    public static Color LoginTint => WithAlpha(BrandSoft, .14f);
    public static Color LoginVignette => WithAlpha(Ink900, .16f);
    public static Color GlassWash => WithAlpha(Surface, .94f);
    public static Color GlassEdge => WithAlpha(Surface, .90f);
    public static Color LoadingVeil => WithAlpha(Surface, .86f);
    public static Color LoginBodyInk => Ink500;
    public static Color LoginFieldBorder => Ink500;
    // Decorative-only colors and opacity: never used for interactive chrome.
    public static Color DecorAqua { get; } = Color.FromArgb(0x8F, 0xE3, 0xE6);
    public static Color DecorSky { get; } = Color.FromArgb(0x7D, 0xB7, 0xFF);
    public static Color DecorLilac { get; } = Color.FromArgb(0xB9, 0xA7, 0xF5);
    public static Color DecorOrbAqua => WithAlpha(DecorAqua, .22f);
    public static Color DecorOrbLilac => WithAlpha(DecorLilac, .20f);
    public static Color DecorOrbAccent => WithAlpha(BrandAccent, .16f);
    public static Color DecorWashSky => WithAlpha(DecorSky, .04f);
    public static Color DecorWashAccent => WithAlpha(BrandAccent, .03f);
    public static Color DecorGlassWhite => WithAlpha(Surface, .20f);
    public static Color DecorGlassAccent => WithAlpha(BrandAccent, .12f);
    public static Color DecorGlassEdge => WithAlpha(Surface, .55f);
    public static Color DecorShadow => WithAlpha(Ink900, .03f);
    public static Color DecorRing => WithAlpha(BrandAccent, .28f);
    public static Color DecorRingWhite => WithAlpha(Surface, .28f);
    public static Color DecorTooth => WithAlpha(BrandAccent, .12f);
    public static Color DecorToothWhite => WithAlpha(Surface, .14f);
    public static Color DecorSparkle => WithAlpha(Surface, .35f);
    public static Color DecorSparkleAccent => WithAlpha(BrandAccent, .30f);
    public static Color DecorAccent => WithAlpha(BrandAccent, .24f);
    public static Color SidebarBg => Surface;
    public static SemanticStyle Info { get; } = new(Color.FromArgb(0xE7, 0xF0, 0xFE), Color.FromArgb(0x1D, 0x4E, 0xD8));
    public static SemanticStyle Success { get; } = new(Color.FromArgb(0xE3, 0xF6, 0xEA), Color.FromArgb(0x15, 0x7F, 0x3D));
    public static SemanticStyle Neutral { get; } = new(Color.FromArgb(0xEC, 0xEF, 0xF3), Color.FromArgb(0x47, 0x55, 0x69));
    public static SemanticStyle Danger { get; } = new(Color.FromArgb(0xFD, 0xE8, 0xE8), Color.FromArgb(0xB2, 0x22, 0x22));
    public static SemanticStyle Warning { get; } = new(Color.FromArgb(0xFE, 0xF3, 0xC7), Color.FromArgb(0x92, 0x40, 0x0E));
    public static Color WarningAccent { get; } = Color.FromArgb(0xD9, 0x77, 0x06);
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
    public const int ControlHeight = 44, CompactHeight = 32, NavHeight = 48, SidebarWidth = 248;
    public const int FormWidth = 380, ControlRadius = 8, CardRadius = 12, IconSize = 20;
    public const int Border = 1, FocusRing = 2, StatusDot = 6, PressOffset = 1;
    public const int MinimumWidth = 1100, MinimumHeight = 700, DialogWidth = 480, DialogHeight = 320;
    public const int RecordDialogWidth = 640, RecordDialogHeight = 600, TimeOffDialogSize = 720;
    public const int ToastWidth = 380, ToastHeight = 112, TooltipWidth = 320, KpiHeight = 208;
    public const int FieldHeight = 92, AlertHeight = 64, EmptyHeight = 176, SkeletonRowHeight = 36;
    public const int Slide = 24, ExitSlide = 12, Settle = 8, Shake = 6;
    public const int CacheLimit = 128, IdentityHeight = 56, GridHeaderHeight = 40;
    public const int GridWideColumn = 180, GridIdentityColumn = 160, GridCompactColumn = 110;
    public const int BaselineDpi = 96;
    public const int TopBarHeight = 56, LoginWidth = 1120, LoginHeight = 600, LoginFormWidth = 360;
    public const int LoginTargetHeight = 749, LoginCardWidth = 440, HeroRadius = 20;
    public const int LoginMinimumWidth = 960, LoginBadgeSize = 80, LoginMarkSize = 56;
    public const int LoginBlurScale = 32, LoginBlurPasses = 3, LoginBlurRadius = 4;
    public const int LoginFieldHeight = 48, LoginDecorClearHeight = 600;
    public const float LoginDecorStroke = 1.75f, LoginDecorPillAspect = .46f;
    public const float LoginDecorMinimumVisibleArea = .40f, LoginGlassMaxLuminanceSpread = .04f;
    public const int LoginDecorGridColumns = 5, LoginDecorGridRows = 4;
    public const int LoginOrbAquaSize = 640, LoginOrbLilacSize = 720, LoginOrbAccentSize = 440;
    public static IReadOnlyList<LoginDecorSpec> LoginDecorations { get; } = Array.AsReadOnly(new LoginDecorSpec[]
    {
        new(LoginDecorKind.GlassSquare, .025f, .16f, 128, 18),
        new(LoginDecorKind.GlassCircle, .97f, .12f, 120),
        new(LoginDecorKind.GlassPill, .04f, .90f, 136),
        new(LoginDecorKind.GlassSquare, .98f, .85f, 112, -16),
        new(LoginDecorKind.Ring, .17f, .20f, 72),
        new(LoginDecorKind.Ring, .88f, .74f, 86),
        new(LoginDecorKind.Arc, .08f, .91f, 144),
        new(LoginDecorKind.Tooth, .14f, .79f, 100, -12),
        new(LoginDecorKind.Sparkle, .11f, .06f, 24),
        new(LoginDecorKind.Sparkle, .90f, .30f, 22),
        new(LoginDecorKind.Sparkle, .18f, .56f, 28),
        new(LoginDecorKind.Sparkle, .90f, .95f, 18),
        new(LoginDecorKind.Cross, .055f, .50f, 18),
        new(LoginDecorKind.Cross, .84f, .07f, 20),
        new(LoginDecorKind.Dot, .17f, .36f, 8),
        new(LoginDecorKind.Dot, .94f, .58f, 10),
        new(LoginDecorKind.Dot, .05f, .73f, 6),
        new(LoginDecorKind.DotGrid, .85f, .88f, 64)
    });
    public const int CalendarHeaderHeight = 48, CalendarHourHeight = 72, CalendarGutter = 64;
    public const int CalendarDayWidth = 100, CalendarViewportHeight = 560, CalendarStatusBar = 3;
    public const int WorklistHeight = 216, HistoryRowHeight = 156;
    public const int ChartHeight = 240, ReportCardHeight = 360, ChartAxisWidth = 80, ChartLabelWidth = 132;
    public const int ReportDateWidth = 156, ReportRangeWidth = 344, ReportLegendNumberWidth = 52, ChartSegmentGap = 2;
    public const float ChartDimOpacity = .55f, ChartAreaOpacity = .14f, DonutHoleRatio = .56f, ChartAreaStart = .65f;
    public const float IconStroke = 1.75f, IconGrid = 24, HighlightAlpha = .14f;
    public static Size MinimumWindow => new(MinimumWidth, MinimumHeight);
    public static int Scale(Control owner, int value) => (int)Math.Round(value * owner.DeviceDpi / (double)BaselineDpi);
}

public static class Typography
{
    private static readonly string Family = ResolveFamily();
    public static Font Display { get; } = new(Family, 24, FontStyle.Bold);
    public static Font Title { get; } = new(Family, 18, FontStyle.Bold);
    public static Font Heading { get; } = new(Family, 13, FontStyle.Bold);
    public static Font Body { get; } = new(Family, 10);
    public static Font Nav { get; } = new(Family, 11);
    public static Font Label { get; } = new(Family, 9, FontStyle.Bold);
    public static Font Caption { get; } = new(Family, 8.5f);
    public static Font KpiNumber { get; } = new(Family, 26, FontStyle.Bold);
    public static string FamilyName => Family;
    public static Font PixelFont(Font font, int dpi) => new(font.FontFamily, font.SizeInPoints * dpi / 72f, font.Style, GraphicsUnit.Pixel);

    private static string ResolveFamily()
    {
        using var installed = new InstalledFontCollection();
        return installed.Families.Any(f => f.Name == "Segoe UI Variable Text") ? "Segoe UI Variable Text" : "Segoe UI";
    }
}
