using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class NavIndicator : DesignControl
{
    public NavIndicator() { Width = Metrics.Border + Metrics.FocusRing; BackColor = Palette.BrandAccent; }
    public void MoveTo(Control item) => MoveTo(item.Top, item.Height);
    public void MoveTo(int destination, int height)
    {
        MotionSystem.Animator.Run(this, "nav-top", Top, destination, MotionSystem.Base, Easing.EaseInOutCubic, t => Top = (int)Math.Round(t));
        MotionSystem.Animator.Run(this, "nav-height", Height, height, MotionSystem.Base, Easing.EaseInOutCubic, t => Height = Math.Max(0, (int)Math.Round(t)));
    }
    protected override void OnPaint(PaintEventArgs e) { if (Width > 0 && Height > 0) e.Graphics.Clear(Palette.BrandAccent); }
}
