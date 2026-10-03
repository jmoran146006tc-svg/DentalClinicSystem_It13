using System.Runtime.CompilerServices;

namespace DentalClinicSystem.Helpers.Design;

public static class Tooltips
{
    private sealed class Attachment : IDisposable
    {
        public ToolTip Tip { get; } = new() { OwnerDraw = true, ShowAlways = true };
        public Attachment(Control owner)
        {
            Tip.Popup += (_, e) =>
            {
                var size = TextRenderer.MeasureText(Tip.GetToolTip(owner), Typography.Caption, new(Metrics.TooltipWidth, 0), TextFormatFlags.WordBreak);
                e.ToolTipSize = new(size.Width + Space.Xl, size.Height + Space.Lg);
            };
            Tip.Draw += (_, e) =>
            {
                e.Graphics.Clear(Palette.Canvas);
                DesignPaint.Surface(e.Graphics, e.Bounds, Metrics.ControlRadius, Palette.Surface, Palette.LineStrong);
                TextRenderer.DrawText(e.Graphics, e.ToolTipText, Typography.Caption, Rectangle.Inflate(e.Bounds, -Space.Md, -Space.Sm), Palette.Ink700, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            };
            owner.Disposed += (_, _) => Dispose();
        }
        public void Dispose() => Tip.Dispose();
    }
    private static readonly ConditionalWeakTable<Control, Attachment> Attachments = new();
    public static void Attach(Control control, string text) => Attachments.GetValue(control, owner => new Attachment(owner)).Tip.SetToolTip(control, text);
}
