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
    protected override void OnMouseWheel(MouseEventArgs e) { base.OnMouseWheel(e); Refresh(); }
}
