using System.Drawing.Drawing2D;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class LoginBrand : DesignControl
{
    private static readonly Image Mark = LoadMark();
    private static Image LoadMark()
    {
        using var stream = typeof(LoginBrand).Assembly.GetManifestResourceStream("DentalClinicSystem.Resources.LoginMark.png")!;
        using var source = Image.FromStream(stream);
        return (Image)source.Clone();
    }
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Image? LogoImage { get; init; } = Mark;
    public LoginBrand() { Dock = DockStyle.Fill; }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Prepare(e.Graphics);
        if (BackgroundImage is { } background)
        {
            var scale = Math.Min((float)Width / background.Width, (float)Height / background.Height);
            var photo = new RectangleF((Width - background.Width * scale) / 2, (Height - background.Height * scale) / 2, background.Width * scale, background.Height * scale);
            e.Graphics.DrawImage(background, photo);
        }
        using var circle = new SolidBrush(Palette.WithAlpha(Palette.Brand, .92f));
        var logoSize = Math.Min(Metrics.NavHeight * 3, Height / 3);
        var top = Space.Xl;
        e.Graphics.FillEllipse(circle, Space.Xl, top, logoSize + Space.Xl * 2, logoSize + Space.Xl * 2);
        if (LogoImage is { } logo)
        {
            var scale = Math.Min((float)logoSize / logo.Width, (float)logoSize / logo.Height);
            e.Graphics.DrawImage(logo, new RectangleF(Space.Xl * 2 + (logoSize - logo.Width * scale) / 2, top + Space.Xl + (logoSize - logo.Height * scale) / 2, logo.Width * scale, logo.Height * scale));
        }
        using var accent = new SolidBrush(Palette.WithAlpha(Palette.BrandAccent, .30f));
        e.Graphics.FillEllipse(accent, -Width / 4, Height - Height / 3, Width / 2, Width / 2);
        var titleTop = Math.Max(top + logoSize + Space.Xl * 2 + Space.Lg, Height / 2);
        // A local light shape guarantees contrast even over the darkest photo pixel.
        var copy = new Rectangle(Space.Xl, titleTop, Math.Min(Width - Space.Xl * 2, Metrics.FormWidth + Space.Xl), Typography.Display.Height + Typography.Body.Height * 2 + Space.Xl * 2);
        DesignPaint.Surface(e.Graphics, copy, Space.Xl, Palette.WithAlpha(Palette.Surface, .90f));
        TextRenderer.DrawText(e.Graphics, "Dental Care", Typography.Display, new Rectangle(copy.Left + Space.Lg, copy.Top + Space.Md, copy.Width - Space.Lg * 2, Typography.Display.Height + Space.Sm), Palette.Ink900, DesignPaint.TextFlags);
        TextRenderer.DrawText(e.Graphics, "Appointments and treatments, organized.", Typography.Body,
            new Rectangle(copy.Left + Space.Lg, copy.Top + Typography.Display.Height + Space.Xl, copy.Width - Space.Lg * 2, Typography.Body.Height * 2), Palette.Ink900, TextFormatFlags.WordBreak);
    }
}
