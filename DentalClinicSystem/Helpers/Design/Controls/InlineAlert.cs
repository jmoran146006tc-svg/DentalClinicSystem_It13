using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class InlineAlert : DesignControl
{
    private float _progress = 1;
    private Semantic _semantic;
    public InlineAlert()
    {
        Height = Metrics.AlertHeight; Dock = DockStyle.Top; TabStop = true; AccessibleRole = AccessibleRole.Alert;
        Cursor = Cursors.Hand;
    }
    public void ShowMessage(string message, Semantic semantic = Semantic.Danger)
    {
        Text = message; _semantic = semantic; Visible = true;
        MotionSystem.Animator.Run(this, "alert", 0, 1, MotionSystem.Base, Easing.EaseOutCubic,
            t => { _progress = t; Height = (int)(Metrics.AlertHeight * Math.Clamp(t, 0, 1)); Invalidate(); });
    }
    public void Dismiss() => MotionSystem.Animator.Run(this, "alert", _progress, 0, MotionSystem.Base, Easing.EaseOutCubic,
        t => { _progress = t; Height = (int)(Metrics.AlertHeight * Math.Clamp(t, 0, 1)); Invalidate(); }, () => Visible = false);
    protected override void OnMouseClick(MouseEventArgs e) { if (e.X >= Width - Metrics.CompactHeight) Dismiss(); base.OnMouseClick(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode is Keys.Escape or Keys.Space or Keys.Enter) { Dismiss(); e.SuppressKeyPress = true; } base.OnKeyDown(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        var style = Theme.SemanticStyle(_semantic);
        DesignPaint.Surface(e.Graphics, ClientRectangle, Metrics.ControlRadius, Theme.Lerp(Parent?.BackColor ?? Palette.Surface, style.Background, _progress));
        Icons.Draw(e.Graphics, IconKind.Warning, new(Space.Md, Space.Md, Metrics.IconSize, Metrics.IconSize), style.Text);
        var text = new Rectangle(Space.Md + Metrics.IconSize + Space.Sm, Space.Sm, Math.Max(0, Width - Metrics.IconSize - Space.Md - Space.Sm - Metrics.CompactHeight), Math.Max(0, Height - Space.Lg));
        TextRenderer.DrawText(e.Graphics, Text, Typography.Body, text, style.Text, TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        Icons.Draw(e.Graphics, IconKind.Close, new(Math.Max(0, Width - Metrics.CompactHeight), Space.Md, Metrics.IconSize, Metrics.IconSize), style.Text);
    }
}
