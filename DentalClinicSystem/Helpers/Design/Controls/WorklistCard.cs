using System.Drawing.Imaging;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WorklistCard : RoundedPanel
{
    private Bitmap? _raisedShadow;
    private float _lift;
    public WorklistCard() : base(elevation: ElevationLevel.E1) => MinimumSize = new(0, Metrics.Scale(this, Metrics.WorklistHeight));
    public void AttachHover()
    {
        foreach (var control in Descendants(this).Prepend(this))
        {
            control.MouseEnter += (_, _) => Lift(true);
            control.MouseLeave += (_, _) => { if (!ClientRectangle.Contains(PointToClient(Cursor.Position))) Lift(false); };
        }
    }
    private static IEnumerable<Control> Descendants(Control parent) => parent.Controls.Cast<Control>()
        .SelectMany(control => new[] { control }.Concat(Descendants(control)));
    private void Lift(bool hover) => MotionSystem.Animator.Run(this, "worklist-lift", _lift, hover ? 1 : 0,
        MotionSystem.Fast, Easing.EaseOutCubic, t => { _lift = Math.Clamp(t, 0, 1); Invalidate(); });
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e); UpdateRaisedShadow();
    }
    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        MinimumSize = new(0, Metrics.Scale(this, Metrics.WorklistHeight)); UpdateRaisedShadow();
    }
    private void UpdateRaisedShadow()
    {
        _raisedShadow?.Dispose(); _raisedShadow = Width > 0 && Height > 0
            ? (Bitmap)ShadowCache.Get(Size, Metrics.CardRadius, ElevationLevel.E2, DeviceDpi).Clone() : null;
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_raisedShadow is null || _lift <= 0 || Width <= 0 || Height <= 0) return;
        using var attributes = new ImageAttributes(); attributes.SetColorMatrix(new ColorMatrix { Matrix33 = _lift });
        e.Graphics.DrawImage(_raisedShadow, ClientRectangle, 0, 0, _raisedShadow.Width, _raisedShadow.Height, GraphicsUnit.Pixel, attributes);
    }
    protected override void Dispose(bool disposing) { if (disposing) _raisedShadow?.Dispose(); base.Dispose(disposing); }
}
