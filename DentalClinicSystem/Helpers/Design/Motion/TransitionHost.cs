using System.Drawing.Imaging;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers.Design.Motion;

public sealed class TransitionHost : IDisposable
{
    private sealed class Overlay(Bitmap previous, Bitmap next) : DesignControl
    {
        private const int HitTestMessage = 0x84, TransparentHit = -1;
        [System.ComponentModel.DefaultValue(0f)]
        public float Progress { get; set; }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;
            e.Graphics.DrawImage(next, ClientRectangle);
            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(new ColorMatrix { Matrix33 = Math.Clamp(1 - Progress, 0, 1) });
            var destination = ClientRectangle; destination.Offset(0, -(int)(Metrics.Settle * Progress));
            e.Graphics.DrawImage(previous, destination, 0, 0, previous.Width, previous.Height, GraphicsUnit.Pixel, attributes);
        }
        protected override void Dispose(bool disposing) { if (disposing) { previous.Dispose(); next.Dispose(); } base.Dispose(disposing); }
        protected override void WndProc(ref Message message)
        {
            if (message.Msg == HitTestMessage) { message.Result = new IntPtr(TransparentHit); return; }
            base.WndProc(ref message);
        }
    }
    private readonly Panel _host;
    private Overlay? _overlay;
    public TransitionHost(Panel host) { _host = host; host.Disposed += HostDisposed; host.VisibleChanged += HostVisibilityChanged; }
    public void Show(Control page)
    {
        Clear();
        var previous = Capture();
        _host.SuspendLayout();
        try
        {
            foreach (Control control in _host.Controls.Cast<Control>().ToArray()) control.Dispose();
            page.Dock = DockStyle.Fill; _host.Controls.Add(page); Theme.Apply(page);
        }
        catch { previous?.Dispose(); throw; }
        finally { _host.ResumeLayout(true); }
        if (previous is null) return;
        var next = Capture();
        if (next is null) { previous.Dispose(); return; }
        _overlay = new Overlay(previous, next) { Dock = DockStyle.Fill, TabStop = false };
        _host.Controls.Add(_overlay); _overlay.BringToFront();
        Motion.Animator.Run(_overlay, "page-transition", 0, 1, Motion.Base, Easing.EaseOutCubic,
            t => { if (_overlay is { IsDisposed: false } overlay) { overlay.Progress = t; overlay.Invalidate(); } }, Clear);
    }
    private Bitmap? Capture()
    {
        if (!Motion.Enabled || !_host.Visible || _host.Width <= 0 || _host.Height <= 0 || _host.Controls.Count == 0) return null;
        Bitmap? snapshot = null;
        try { snapshot = new Bitmap(_host.Width, _host.Height); _host.DrawToBitmap(snapshot, _host.ClientRectangle); return snapshot; }
        catch (Exception) { snapshot?.Dispose(); return null; }
    }
    private void Clear() { _overlay?.Dispose(); _overlay = null; }
    private void HostDisposed(object? sender, EventArgs e) => Dispose();
    private void HostVisibilityChanged(object? sender, EventArgs e) { if (!_host.Visible) Clear(); }
    public void Dispose() { Clear(); _host.Disposed -= HostDisposed; _host.VisibleChanged -= HostVisibilityChanged; }
}
