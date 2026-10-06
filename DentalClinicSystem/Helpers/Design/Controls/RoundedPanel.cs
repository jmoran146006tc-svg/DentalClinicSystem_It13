using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public class RoundedPanel : Panel
{
    private Bitmap? _shadow;
    private ElevationLevel _elevation;
    private Color _faceColor = Palette.Surface;
    protected Color FaceBorder { get; set; } = Palette.Line;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Color FaceColor
    {
        get => _faceColor;
        set { _faceColor = value; Content.BackColor = value; Invalidate(); }
    }
    public Panel Content { get; } = new() { Dock = DockStyle.Fill, BackColor = Palette.Surface };
    [System.ComponentModel.DefaultValue(ElevationLevel.E0)]
    public ElevationLevel Elevation
    {
        get => _elevation;
        set { _elevation = value; UpdateShadow(); }
    }
    public RoundedPanel(string? title = null, ElevationLevel elevation = ElevationLevel.E0)
    {
        Theme.MarkPrimitive(this);
        DesignPaint.Enable(this);
        BackColor = Palette.Surface; Font = Typography.Body; Margin = new Padding(Space.Sm);
        Controls.Add(Content);
        if (!string.IsNullOrWhiteSpace(title))
        {
            var heading = new Label { Text = title, Dock = DockStyle.Top, Height = Metrics.ControlHeight, Font = Typography.Heading, ForeColor = Palette.Ink900, BackColor = Palette.Surface };
            Content.Controls.Add(heading);
        }
        Elevation = elevation;
    }
    private void UpdateShadow()
    {
        _shadow?.Dispose(); _shadow = null;
        var inset = Metrics.Scale(this, Design.Elevation.Padding(_elevation));
        Padding = new Padding(inset + Metrics.Scale(this, Space.Lg));
        if (Width > 0 && Height > 0 && _elevation != ElevationLevel.E0)
            _shadow = (Bitmap)ShadowCache.Get(Size, Metrics.CardRadius, _elevation, DeviceDpi).Clone();
        Invalidate();
    }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); UpdateShadow(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); UpdateShadow(); }
    protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Palette.Canvas);
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        if (_shadow is not null) e.Graphics.DrawImageUnscaled(_shadow, Point.Empty);
        var inset = Metrics.Scale(this, Design.Elevation.Padding(Elevation));
        var bounds = Rectangle.Inflate(ClientRectangle, -inset, -inset);
        DesignPaint.Surface(e.Graphics, bounds, Metrics.Scale(this, Metrics.CardRadius), FaceColor, FaceBorder);
        base.OnPaint(e);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _shadow?.Dispose(); MotionSystem.Animator.Cancel(this); }
        base.Dispose(disposing);
    }
}
