using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public class RoundedPanel : Panel
{
    private Bitmap? _shadow;
    private ElevationLevel _elevation;
    protected virtual int ContentInset => Space.Lg;
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
        Padding = new Padding(inset + Metrics.Scale(this, ContentInset));
        if (Width > 0 && Height > 0 && _elevation != ElevationLevel.E0)
            _shadow = (Bitmap)ShadowCache.Get(Size, Metrics.CardRadius, _elevation, DeviceDpi).Clone();
        Invalidate();
    }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); UpdateShadow(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); UpdateShadow(); }
    protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(DesignPaint.ParentBackground(this));
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        OnPaintBackground(e);
        DesignPaint.Prepare(e.Graphics);
        PaintSurface(e.Graphics);
        base.OnPaint(e);
    }
    // Photo-backed cards reuse the same surface and cached shadow while painting
    // the actual parent pixels beneath the rounded corners.
    protected void PaintSurface(Graphics graphics)
    {
        if (_shadow is not null) graphics.DrawImageUnscaled(_shadow, Point.Empty);
        var inset = Metrics.Scale(this, Design.Elevation.Padding(Elevation));
        var bounds = Rectangle.Inflate(ClientRectangle, -inset, -inset);
        DesignPaint.Surface(graphics, bounds, Metrics.Scale(this, Metrics.CardRadius), Palette.Surface, Palette.Line);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _shadow?.Dispose(); MotionSystem.Animator.Cancel(this); }
        base.Dispose(disposing);
    }
}
