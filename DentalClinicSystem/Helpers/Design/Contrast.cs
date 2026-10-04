namespace DentalClinicSystem.Helpers.Design;

public static class Contrast
{
    public static Color Composite(Color foreground, Color background)
    {
        var alpha = foreground.A / 255d;
        return Color.FromArgb(Channel(foreground.R, background.R), Channel(foreground.G, background.G), Channel(foreground.B, background.B));
        int Channel(byte front, byte back) => (int)Math.Round(front * alpha + back * (1 - alpha));
    }
    public static double Luminance(Color color) => .2126 * Channel(color.R) + .7152 * Channel(color.G) + .0722 * Channel(color.B);
    public static double Ratio(Color foreground, Color background)
    {
        var a = Luminance(foreground);
        var b = Luminance(background);
        return (Math.Max(a, b) + .05) / (Math.Min(a, b) + .05);
    }
    private static double Channel(byte value)
    {
        var s = value / 255d;
        return s <= .04045 ? s / 12.92 : Math.Pow((s + .055) / 1.055, 2.4);
    }

    public static IEnumerable<(string Name, Color Text, Color Background)> RequiredPairs()
    {
        foreach (var (name, ink) in new[] { (nameof(Palette.Ink900), Palette.Ink900), (nameof(Palette.Ink700), Palette.Ink700), (nameof(Palette.Ink500), Palette.Ink500) })
        {
            yield return ($"{name}/Surface", ink, Palette.Surface);
            yield return ($"{name}/Canvas", ink, Palette.Canvas);
        }
        yield return ("Surface/Brand", Palette.Surface, Palette.Brand);
        yield return ("Surface/BrandHover", Palette.Surface, Palette.BrandHover);
        yield return ("Surface/BrandPressed", Palette.Surface, Palette.BrandPressed);
        foreach (var semantic in Enum.GetValues<Semantic>())
        {
            var style = Theme.SemanticStyle(semantic);
            yield return ($"{semantic} soft", style.Text, style.Background);
        }
        yield return ("BrandSoftText/BrandSoft", Palette.BrandSoftText, Palette.BrandSoft);
        yield return ("Danger/Surface", Palette.Danger.Text, Palette.Surface);
        var loginBackground = Composite(Palette.LoginScrimBrandEdge, Composite(Palette.LoginPhotoWash, Palette.Surface));
        yield return ("Login headline/scrim", Palette.Surface, loginBackground);
        yield return ("Login caption/scrim", Composite(Palette.LoginCaption, Palette.BrandPressed), loginBackground);
        yield return ("Login footer/scrim", Composite(Palette.LoginFooter, Palette.BrandPressed), loginBackground);
        yield return ("Login chip/scrim", Palette.Surface, Composite(Palette.LoginChipFill, loginBackground));
        yield return ("Login mark/badge", Palette.Surface, Composite(Palette.LoginBadgeFill, loginBackground));
        yield return ("Caps Lock/Surface", Palette.Warning.Text, Palette.Surface);
    }
}
