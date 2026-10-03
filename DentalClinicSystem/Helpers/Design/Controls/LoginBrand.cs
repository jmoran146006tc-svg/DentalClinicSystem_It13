using System.Drawing.Drawing2D;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class LoginBrand : DesignControl
{
    public LoginBrand() { Dock = DockStyle.Fill; }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Prepare(e.Graphics);
        using var gradient = new LinearGradientBrush(ClientRectangle, Palette.BrandAccent, Palette.Brand, LinearGradientMode.ForwardDiagonal);
        e.Graphics.FillRectangle(gradient, ClientRectangle);
        using var circle = new SolidBrush(Palette.WithAlpha(Palette.Surface, .08f));
        e.Graphics.FillEllipse(circle, -Width / 3, -Width / 3, Width, Width);
        e.Graphics.FillEllipse(circle, Width / 2, Height - Width / 2, Width, Width);
        var top = Height / 3;
        Icons.Draw(e.Graphics, IconKind.Dentist, new(Space.Xxxl, top, Metrics.NavHeight, Metrics.NavHeight), Palette.Surface);
        TextRenderer.DrawText(e.Graphics, "Dental Care", Typography.Display, new Rectangle(Space.Xxxl, top + Metrics.NavHeight + Space.Xl, Width - Space.Xxxl * 2, Metrics.FieldHeight), Palette.Surface, DesignPaint.TextFlags);
        TextRenderer.DrawText(e.Graphics, "Appointments and treatments, organized.", Typography.Body,
            new Rectangle(Space.Xxxl, top + Metrics.FieldHeight + Space.Xxxl, Width - Space.Xxxl * 2, Metrics.FieldHeight), Palette.Surface, TextFormatFlags.WordBreak);
    }
}
