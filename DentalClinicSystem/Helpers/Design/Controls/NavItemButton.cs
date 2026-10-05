using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

// The existing Antd button remains the navigation command; this child owns its
// presentation because AntdUI 2.4.12 centers icon/text groups regardless of TextAlign.
public sealed class NavItemButton : DesignControl
{
    private readonly IconKind _icon;
    private Color _fill = Palette.SidebarBg;
    private bool _selected, _hovered;
    public Rectangle IconBounds => new(0, (Height - Metrics.Scale(this, Metrics.IconSize)) / 2,
        Metrics.Scale(this, Metrics.IconSize), Metrics.Scale(this, Metrics.IconSize));
    public Rectangle TextBounds => new(IconBounds.Right + Metrics.Scale(this, Space.Md), 0,
        Math.Max(0, Width - IconBounds.Right - Metrics.Scale(this, Space.Md)), Height);
    [System.ComponentModel.DefaultValue(false)]
    public bool Selected { get => _selected; set { _selected = value; UpdateFill(); } }
    public NavItemButton(string text, IconKind icon)
    {
        _icon = icon; Text = text; Font = Typography.Nav; Dock = DockStyle.Fill;
        Margin = Padding.Empty; Cursor = Cursors.Hand; TabStop = true;
        AccessibleRole = AccessibleRole.PushButton; AccessibleName = text;
    }
    private void UpdateFill() => MotionSystem.Animator.RunColor(this, "navigation-fill", _fill,
        _selected || _hovered ? Palette.BrandSoft : Palette.SidebarBg, MotionSystem.Fast,
        color => { _fill = color; Invalidate(); });
    protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hovered = true; UpdateFill(); }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = false; UpdateFill(); }
    protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) Focus(); base.OnMouseDown(e); }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Enter or Keys.Space || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter or Keys.Space) { e.Handled = true; e.SuppressKeyPress = true; }
        base.OnKeyDown(e);
    }
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (Enabled && e.KeyCode is Keys.Enter or Keys.Space) { OnClick(EventArgs.Empty); e.Handled = true; }
        base.OnKeyUp(e);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        DesignPaint.Surface(e.Graphics, ClientRectangle, Metrics.Scale(this, Metrics.ControlRadius), _fill);
        var ink = !Enabled ? Palette.Ink500 : _selected ? Palette.BrandSoftText : Palette.Ink700;
        Icons.Draw(e.Graphics, _icon, IconBounds, ink);
        TextRenderer.DrawText(e.Graphics, Text, Font, TextBounds, ink, DesignPaint.TextFlags);
        if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -Metrics.FocusRing, -Metrics.FocusRing), Palette.Brand, _fill);
    }
}
