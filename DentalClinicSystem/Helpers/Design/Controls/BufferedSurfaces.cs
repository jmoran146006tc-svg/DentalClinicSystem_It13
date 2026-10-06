namespace DentalClinicSystem.Helpers.Design.Controls;

// Buffer the whole child-window tree. Per-control buffering alone leaves stale
// strips when WinForms scrolls a partly clipped chart or card with ScrollWindowEx.
public class BufferedPage : UserControl
{
    public BufferedPage() => DesignPaint.EnableContainer(this);
    protected override CreateParams CreateParams
    {
        get { var parameters = base.CreateParams; parameters.ExStyle |= 0x02000000; return parameters; }
    }
    protected override void OnScroll(ScrollEventArgs e) { base.OnScroll(e); Refresh(); }
    protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); Refresh(); }
}

public sealed class BufferedPanel : Panel
{
    public BufferedPanel() => DesignPaint.EnableContainer(this);
    protected override CreateParams CreateParams
    {
        get { var parameters = base.CreateParams; parameters.ExStyle |= 0x02000000; return parameters; }
    }
    protected override void OnScroll(ScrollEventArgs e) { base.OnScroll(e); Refresh(); }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        var position = -AutoScrollPosition.Y;
        var maximum = Math.Max(0, DisplayRectangle.Height - ClientSize.Height);
        if (AutoScroll && (e.Delta > 0 ? position <= 0 : position >= maximum)) WheelRouting.Forward(this, e);
        else base.OnMouseWheel(e);
        Refresh();
    }
}
