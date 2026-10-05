using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class LoadingOverlay : DesignControl
{
    private Bitmap? _snapshot;
    private float _phase;
    public LoadingOverlay(Control target)
    {
        Dock = DockStyle.Fill; AccessibleName = "Loading, please wait"; AccessibleRole = AccessibleRole.Alert;
        if (target.Width > 0 && target.Height > 0)
        {
            try { _snapshot = DesignPaint.Snapshot(target); }
            catch (Exception) { _snapshot?.Dispose(); _snapshot = null; }
        }
        VisibleChanged += (_, _) => { if (Visible) Start(); };
    }
    private void Start() => MotionSystem.Animator.Loop(this, "loading-spin", MotionSystem.SpinnerPeriod, phase => { _phase = phase; Invalidate(); });
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        if (_snapshot is not null) e.Graphics.DrawImage(_snapshot, ClientRectangle);
        using var veil = new SolidBrush(Palette.LoadingVeil);
        e.Graphics.FillRectangle(veil, ClientRectangle);
        DesignPaint.Prepare(e.Graphics);
        using var pen = new Pen(Palette.Brand, Metrics.FocusRing);
        var spinner = new Rectangle((Width - Metrics.CompactHeight) / 2, (Height - Metrics.CompactHeight) / 2, Metrics.CompactHeight, Metrics.CompactHeight);
        e.Graphics.DrawArc(pen, spinner, _phase * 360, 250);
        TextRenderer.DrawText(e.Graphics, "Loading…", Typography.Body, new Rectangle(0, spinner.Bottom + Space.Sm, Width, Metrics.CompactHeight), Palette.Ink700, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
    }
    protected override void Dispose(bool disposing) { if (disposing) _snapshot?.Dispose(); base.Dispose(disposing); }
}

public sealed class Skeleton : DesignControl
{
    private float _phase;
    public Skeleton()
    {
        Height = Metrics.SkeletonRowHeight * 5; Width = Metrics.FormWidth; AccessibleName = "Loading records";
        VisibleChanged += (_, _) => { if (Visible) Start(); };
        ParentChanged += (_, _) => { if (Visible && Parent is not null) Start(); };
    }
    private void Start() => MotionSystem.Animator.Loop(this, "shimmer", MotionSystem.ShimmerPeriod, phase => { _phase = phase; Invalidate(); });
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        for (var row = 0; row < 5; row++)
        {
            var bounds = new Rectangle(Space.Sm, row * Metrics.SkeletonRowHeight + Space.Sm, Math.Max(0, Width - Space.Lg), Metrics.SkeletonRowHeight - Space.Lg);
            var light = MotionSystem.Enabled ? (MathF.Sin((_phase - row * .08f) * MathF.PI * 2) + 1) / 2 : 0;
            DesignPaint.Surface(e.Graphics, bounds, Metrics.ControlRadius, Theme.Lerp(Palette.Line, Palette.SurfaceAlt, light));
        }
    }
}
