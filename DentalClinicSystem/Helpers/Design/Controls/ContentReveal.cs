using System.Drawing.Imaging;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

// A mouse-transparent, temporary snapshot keeps native child controls responsive during the fade.
public sealed class ContentReveal : DesignControl
{
    private readonly Bitmap _before;
    private readonly Bitmap _after;
    private float _progress;
    private ContentReveal(Bitmap before, Bitmap after) { _before = before; _after = after; Dock = DockStyle.Fill; }
    public static Bitmap? Snapshot(Control target)
    {
        if (!MotionSystem.Enabled || !target.Visible || target.Width <= 0 || target.Height <= 0) return null;
        try { return DesignPaint.Snapshot(target); }
        catch (ArgumentException) { return null; }
    }
    public static void Play(Control target, Bitmap? before)
    {
        if (before is null) return;
        var after = Snapshot(target);
        if (after is null) { before.Dispose(); return; }
        var reveal = new ContentReveal(before, after);
        target.Controls.Add(reveal); reveal.BringToFront();
        MotionSystem.Animator.Run(reveal, "content-reveal", 0, 1, MotionSystem.Base, Easing.EaseOutCubic, t =>
        {
            reveal._progress = Math.Clamp(t, 0, 1);
            if (t >= 1) reveal.Dispose(); else reveal.Invalidate();
        });
    }
    protected override void WndProc(ref Message message)
    {
        const int hitTest = 0x0084;
        if (message.Msg == hitTest) { message.Result = new IntPtr(-1); return; }
        base.WndProc(ref message);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        e.Graphics.DrawImage(_after, ClientRectangle);
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = 1 - _progress });
        e.Graphics.DrawImage(_before, ClientRectangle, 0, 0, _before.Width, _before.Height, GraphicsUnit.Pixel, attributes);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _before.Dispose(); _after.Dispose(); }
        base.Dispose(disposing);
    }
}
