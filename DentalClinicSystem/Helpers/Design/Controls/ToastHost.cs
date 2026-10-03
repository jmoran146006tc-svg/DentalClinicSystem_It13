using System.Runtime.CompilerServices;
using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Helpers.Native;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ToastHost : IDisposable
{
    private sealed record Message(string Text, Semantic Semantic);
    private static readonly ConditionalWeakTable<Form, ToastHost> Hosts = new();
    private readonly Form _owner;
    private readonly Queue<Message> _queue = new();
    private ToastWindow? _window;
    private bool _disposed;
    private ToastHost(Form owner)
    {
        _owner = owner; owner.Disposed += OwnerDisposed; owner.VisibleChanged += OwnerVisibility;
    }
    public static void Show(Form owner, string text, Semantic semantic) => Hosts.GetValue(owner, form => new ToastHost(form)).Enqueue(new(text, semantic));
    private void Enqueue(Message message)
    {
        if (_disposed || !_owner.Visible) return;
        _queue.Enqueue(message); Next();
    }
    private void Next()
    {
        if (_window is not null || _queue.Count == 0 || _disposed || !_owner.Visible) return;
        _window = new ToastWindow(_owner, _queue.Dequeue());
        _window.FormClosed += (_, _) => { _window = null; Next(); };
        _window.Show(_owner);
    }
    private void OwnerDisposed(object? sender, EventArgs e) => Dispose();
    private void OwnerVisibility(object? sender, EventArgs e) { if (!_owner.Visible) { _queue.Clear(); _window?.Dispose(); _window = null; } }
    public void Dispose()
    {
        _disposed = true; _queue.Clear(); _window?.Dispose(); _window = null;
        _owner.Disposed -= OwnerDisposed; _owner.VisibleChanged -= OwnerVisibility;
    }

    private sealed class ToastWindow : Form
    {
        private const int NoActivate = 0x08000000;
        private readonly Form _owner;
        private readonly Message _message;
        private Point _target;
        private float _remaining = 1;
        private bool _closing;
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams { get { var parameters = base.CreateParams; parameters.ExStyle |= NoActivate; return parameters; } }
        public ToastWindow(Form owner, Message message)
        {
            _owner = owner; _message = message; DesignPaint.Enable(this);
            FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; StartPosition = FormStartPosition.Manual;
            Size = new(Metrics.ToastWidth, Metrics.ToastHeight); BackColor = Palette.Canvas;
            AccessibleName = message.Text; AccessibleRole = AccessibleRole.Alert; WindowChrome.Apply(this);
            owner.LocationChanged += Reposition; owner.SizeChanged += Reposition;
            MouseEnter += (_, _) => MotionSystem.Animator.Cancel(this, "toast-lifetime");
            MouseLeave += (_, _) => Lifetime();
            MouseClick += (_, _) => Dismiss();
        }
        private void Reposition(object? sender, EventArgs e)
        {
            var screen = _owner.RectangleToScreen(_owner.ClientRectangle);
            _target = new(screen.Right - Width - Space.Xl, screen.Top + Space.Xl); Location = _target;
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e); Reposition(this, EventArgs.Empty);
            MotionSystem.Animator.Run(this, "toast-enter", 0, 1, MotionSystem.Base, Easing.EaseOutBack,
                t => { Opacity = Math.Clamp(t, 0, 1); Left = _target.X + (int)(Metrics.Slide * (1 - t)); }, Lifetime);
        }
        private void Lifetime()
        {
            if (_closing) return;
            var start = _remaining;
            MotionSystem.Animator.Schedule(this, "toast-lifetime", MotionSystem.ToastLifetime * start, Dismiss,
                t => { _remaining = start * (1 - t); Invalidate(new Rectangle(0, Height - Space.Sm, Width, Space.Sm)); });
        }
        private void Dismiss()
        {
            if (_closing) return; _closing = true;
            MotionSystem.Animator.Cancel(this, "toast-lifetime"); MotionSystem.Animator.Cancel(this, "toast-enter");
            MotionSystem.Animator.Run(this, "toast-exit", 0, 1, MotionSystem.Fast, Easing.EaseOutCubic,
                t => { Opacity = 1 - Math.Clamp(t, 0, 1); Left = _target.X + (int)(Metrics.ExitSlide * t); }, Close);
        }
        protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Palette.Canvas);
        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) return;
            DesignPaint.Begin(e.Graphics, this);
            var style = Theme.SemanticStyle(_message.Semantic);
            DesignPaint.Surface(e.Graphics, ClientRectangle, Metrics.CardRadius, Palette.Surface, Palette.Line);
            var tile = new Rectangle(Space.Lg, Space.Lg, Metrics.NavHeight, Metrics.NavHeight);
            DesignPaint.Surface(e.Graphics, tile, Metrics.ControlRadius, style.Background);
            Icons.Draw(e.Graphics, _message.Semantic == Semantic.Success ? IconKind.Check : _message.Semantic == Semantic.Danger ? IconKind.Warning : IconKind.Info, Rectangle.Inflate(tile, -Space.Sm, -Space.Sm), style.Text);
            TextRenderer.DrawText(e.Graphics, _message.Text, Typography.Body, new Rectangle(tile.Right + Space.Md, Space.Lg, Math.Max(0, Width - tile.Right - Space.Xxl), Height - Space.Xxl), Palette.Ink700, TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            using var brush = new SolidBrush(style.Text); e.Graphics.FillRectangle(brush, Space.Sm, Height - Space.Xs, Math.Max(0, (Width - Space.Lg) * _remaining), Metrics.FocusRing);
            base.OnPaint(e);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { MotionSystem.Animator.Cancel(this); _owner.LocationChanged -= Reposition; _owner.SizeChanged -= Reposition; }
            base.Dispose(disposing);
        }
    }
}
