namespace DentalClinicSystem.Helpers.Design.Motion;

public sealed class TransitionHost : IDisposable
{
    private readonly Panel _host;
    public TransitionHost(Panel host) => _host = host;
    public void Show(Control page)
    {
        // Lay out the real page before Load starts. Its skeleton stays live while
        // awaiting data; snapshots of native controls can contain stale paint.
        page.Visible = false;
        _host.SuspendLayout();
        try
        {
            foreach (Control control in _host.Controls.Cast<Control>().ToArray()) control.Dispose();
            page.Dock = DockStyle.Fill; page.Bounds = _host.ClientRectangle; Theme.Apply(page); _host.Controls.Add(page);
        }
        finally { _host.ResumeLayout(true); }
        page.Visible = true;
        if (page.Enabled) page.SelectNextControl(null, true, true, true, false);
        _host.Invalidate(true);
    }
    public void Dispose() { }
}
