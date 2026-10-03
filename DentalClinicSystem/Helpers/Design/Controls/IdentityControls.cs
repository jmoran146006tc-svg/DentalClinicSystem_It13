using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

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

public sealed class Toggle : DesignControl
{
    private bool _checked;
    private float _position;
    private bool _hover;
    public event EventHandler? CheckedChanged;
    [System.ComponentModel.DefaultValue(false)]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked = value;
            MotionSystem.Animator.Run(this, "toggle", _position, value ? 1 : 0, MotionSystem.Base, Easing.EaseOutBack, t => { _position = t; Invalidate(); });
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public Toggle(string text = "Show inactive")
    {
        Text = text; Size = new(Metrics.FormWidth / 2, Metrics.ControlHeight); TabStop = true; Cursor = Cursors.Hand;
        AccessibleRole = AccessibleRole.CheckButton;
        MouseEnter += (_, _) => { _hover = true; Invalidate(); }; MouseLeave += (_, _) => { _hover = false; Invalidate(); };
        GotFocus += (_, _) => Invalidate(); LostFocus += (_, _) => Invalidate(); EnabledChanged += (_, _) => Invalidate();
    }
    protected override void OnClick(EventArgs e) { if (Enabled) { Focus(); Checked = !Checked; } base.OnClick(e); }
    protected override void OnKeyDown(KeyEventArgs e) { if (e.KeyCode is Keys.Space or Keys.Enter) { Checked = !Checked; e.SuppressKeyPress = true; } base.OnKeyDown(e); }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        var track = new Rectangle(Space.Xs, Space.Sm, Metrics.NavHeight, Height - Space.Lg);
        DesignPaint.Surface(e.Graphics, track, track.Height / 2f, Enabled ? Theme.Lerp(Palette.Neutral.Background, Palette.Brand, _position) : Palette.SurfaceAlt, Focused || _hover ? Palette.BrandAccent : null);
        var diameter = Math.Max(0, track.Height - Space.Xs);
        var left = track.Left + Metrics.FocusRing + (track.Width - diameter - Space.Xs) * Math.Clamp(_position, -.1f, 1.1f);
        using var brush = new SolidBrush(Palette.Surface);
        e.Graphics.FillEllipse(brush, left, track.Top + Metrics.FocusRing, diameter, diameter);
        TextRenderer.DrawText(e.Graphics, Text, Typography.Body, new Rectangle(track.Right + Space.Sm, 0, Math.Max(0, Width - track.Right - Space.Sm), Height), Enabled ? Palette.Ink700 : Palette.Ink400, DesignPaint.TextFlags);
    }
}
