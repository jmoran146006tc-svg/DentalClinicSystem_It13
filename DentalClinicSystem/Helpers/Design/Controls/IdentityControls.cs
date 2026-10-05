namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class Avatar : DesignControl
{
    public Avatar(string name) { Text = name; Size = new(Metrics.NavHeight, Metrics.NavHeight); AccessibleRole = AccessibleRole.Graphic; }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        Draw(e.Graphics, ClientRectangle, Text);
    }
    public static void Draw(Graphics graphics, Rectangle bounds, string name)
    {
        DesignPaint.Prepare(graphics);
        var style = DisplayFormat.AvatarStyle(name);
        using var brush = new SolidBrush(style.Background);
        using var ring = new Pen(Palette.Surface, Metrics.FocusRing);
        bounds.Inflate(-Metrics.FocusRing, -Metrics.FocusRing);
        graphics.FillEllipse(brush, bounds); graphics.DrawEllipse(ring, bounds);
        TextRenderer.DrawText(graphics, DisplayFormat.Initials(name), Typography.Label, bounds, style.Text, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
    }
}

public sealed class Toggle : AntdUI.Checkbox
{
    public Toggle(string text = "Show inactive")
    {
        Theme.MarkPrimitive(this); Text = text; Font = Typography.Body;
        Height = Metrics.ControlHeight; Width = Metrics.FormWidth / 2;
        TabStop = true; Cursor = Cursors.Hand; AccessibleRole = AccessibleRole.CheckButton;
        TextChanged += (_, _) => Fit(); DpiChangedAfterParent += (_, _) => Fit(); Fit();
    }
    private void Fit() => MinimumSize = new(TextRenderer.MeasureText(Text, Font).Width + Metrics.Scale(this, Metrics.ControlHeight + Space.Md), Metrics.Scale(this, Metrics.ControlHeight));
}
