using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WorklistCard : RoundedPanel
{
    public WorklistCard() : base(elevation: ElevationLevel.E0) { }
    public void AttachHover()
    {
        foreach (var control in Descendants(this).Prepend(this))
        {
            control.MouseEnter += (_, _) => Hover(true);
            control.MouseLeave += (_, _) => { if (!ClientRectangle.Contains(PointToClient(Cursor.Position))) Hover(false); };
        }
    }
    private static IEnumerable<Control> Descendants(Control parent) => parent.Controls.Cast<Control>()
        .SelectMany(control => new[] { control }.Concat(Descendants(control)));
    private void Hover(bool hover) => MotionSystem.Animator.RunColor(this, "worklist-border", FaceBorder,
        hover ? FaceColor == Palette.BrandSoft ? Palette.BrandAccent : Palette.LineStrong : Palette.Line,
        MotionSystem.Fast, color => { FaceBorder = color; Invalidate(); });
}
