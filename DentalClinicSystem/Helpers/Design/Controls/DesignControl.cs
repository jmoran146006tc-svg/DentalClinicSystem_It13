using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public abstract class DesignControl : Control
{
    protected DesignControl()
    {
        DesignPaint.Enable(this);
        BackColor = Palette.Surface;
        ForeColor = Palette.Ink700;
        Font = Typography.Body;
        TabStop = false;
    }
    protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Palette.Canvas);
    protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); AccessibleName = Text; Invalidate(); }
    protected override void Dispose(bool disposing)
    {
        if (disposing) MotionSystem.Animator.Cancel(this);
        base.Dispose(disposing);
    }
}
