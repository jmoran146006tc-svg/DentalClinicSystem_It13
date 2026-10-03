using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public enum ButtonVariant { Primary, Secondary, Danger, Ghost }
public enum ButtonSize { Regular, Compact }

public sealed class AppButton : Button
{
    private bool _busy;
    [System.ComponentModel.DefaultValue("Working…")]
    public string BusyText { get; set; } = "Working…";
    [System.ComponentModel.DefaultValue(false)]
    public bool IsBusy
    {
        get => _busy;
        set { if (_busy == value) return; _busy = value; ButtonStyler.SetBusy(this, value); Invalidate(); }
    }
    public AppButton(string text, ButtonVariant variant = ButtonVariant.Primary, IconKind? icon = null, ButtonSize size = ButtonSize.Regular)
    {
        Text = text;
        Height = size == ButtonSize.Compact ? Metrics.CompactHeight : Metrics.ControlHeight;
        Width = Metrics.FormWidth / 2;
        Margin = new Padding(Space.Xs);
        AccessibleName = text;
        ButtonStyler.Attach(this, variant, icon);
    }
    protected override void OnClick(EventArgs e) { if (!IsBusy) base.OnClick(e); }
}

public static class ButtonStyler
{
    private sealed class State(Button button, ButtonVariant variant, IconKind? icon)
    {
        public Button Button { get; } = button;
        public ButtonVariant Variant { get; set; } = variant;
        public IconKind? Icon { get; set; } = icon;
        public bool Hover { get; set; }
        public bool Pressed { get; set; }
        public bool Busy { get; set; }
        public float Focus { get; set; }
        public float Offset { get; set; }
        public float Phase { get; set; }
        public Color Fill { get; set; } = Palette.Surface;
        public Color Border { get; set; } = Palette.LineStrong;
    }
    private static readonly ConditionalWeakTable<Button, State> States = new();
    public static void Attach(Button button, ButtonVariant variant, IconKind? icon = null)
    {
        if (States.TryGetValue(button, out var existing)) { existing.Variant = variant; existing.Icon = icon; Change(existing); return; }
        var state = new State(button, variant, icon);
        States.Add(button, state);
        DesignPaint.Enable(button);
        button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 0;
        button.UseVisualStyleBackColor = false; button.Font = Typography.Label; button.Cursor = Cursors.Hand;
        button.TabStop = true; button.AccessibleRole = AccessibleRole.PushButton;
        button.AccessibleName ??= button.Text;
        button.Paint += (_, e) => Draw(state, e.Graphics);
        button.MouseEnter += (_, _) => { state.Hover = true; Change(state); };
        button.MouseLeave += (_, _) => { state.Hover = false; state.Pressed = false; Change(state); };
        button.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { state.Pressed = true; Change(state); } };
        button.MouseUp += (_, _) => { state.Pressed = false; Change(state); };
        button.KeyDown += (_, e) => { if (e.KeyCode is Keys.Space or Keys.Enter) { state.Pressed = true; Change(state); } };
        button.KeyUp += (_, _) => { state.Pressed = false; Change(state); };
        button.GotFocus += (_, _) => Focus(state, 1);
        button.LostFocus += (_, _) => { state.Pressed = false; Focus(state, 0); Change(state); };
        button.EnabledChanged += (_, _) => Change(state);
        button.VisibleChanged += (_, _) =>
        {
            if (!button.Visible) { state.Hover = false; state.Pressed = false; Focus(state, 0); }
            Change(state);
            if (button.Visible && state.Busy) StartSpinner(state);
        };
        button.Disposed += (_, _) => MotionSystem.Animator.Cancel(button);
        Change(state);
    }
    private static void Change(State state)
    {
        var fill = !state.Button.Enabled ? Palette.SurfaceAlt : state.Variant switch
        {
            ButtonVariant.Primary => state.Pressed ? Palette.BrandPressed : state.Hover ? Palette.BrandHover : Palette.Brand,
            ButtonVariant.Danger => state.Pressed ? Theme.Lerp(Palette.Danger.Text, Palette.Ink900, .2f) : state.Hover ? Theme.Lerp(Palette.Danger.Text, Palette.Ink900, .1f) : Palette.Danger.Text,
            ButtonVariant.Ghost => state.Hover ? Palette.BrandSoft : state.Button.Parent?.BackColor ?? Palette.Surface,
            _ => Palette.Surface
        };
        var border = state.Hover && state.Button.Enabled ? Palette.Brand : Palette.LineStrong;
        var duration = state.Pressed ? MotionSystem.Instant : MotionSystem.Fast;
        MotionSystem.Animator.RunColor(state.Button, "button-fill", state.Fill, fill, duration, c => { state.Fill = c; state.Button.Invalidate(); });
        MotionSystem.Animator.RunColor(state.Button, "button-border", state.Border, border, duration, c => { state.Border = c; state.Button.Invalidate(); });
        MotionSystem.Animator.Run(state.Button, "button-press", state.Offset, state.Pressed ? Metrics.PressOffset : 0, MotionSystem.Instant, Easing.EaseOutCubic, t => { state.Offset = t; state.Button.Invalidate(); });
    }
    private static void Focus(State state, float value) => MotionSystem.Animator.Run(state.Button, "button-focus", state.Focus, value, MotionSystem.Instant, Easing.EaseOutCubic, t => { state.Focus = t; state.Button.Invalidate(); });
    public static void SetBusy(Button button, bool busy)
    {
        if (!States.TryGetValue(button, out var state)) return;
        state.Busy = busy;
        if (busy) StartSpinner(state); else MotionSystem.Animator.Cancel(button, "button-spinner");
    }
    private static void StartSpinner(State state) => MotionSystem.Animator.Loop(state.Button, "button-spinner", MotionSystem.SpinnerPeriod, t => { state.Phase = t; state.Button.Invalidate(); });
    private static void Draw(State state, Graphics graphics)
    {
        var button = state.Button;
        if (button.Width <= 0 || button.Height <= 0) return;
        graphics.Clear(button.Parent?.BackColor ?? Palette.Canvas);
        var bounds = button.ClientRectangle;
        bounds.Inflate(-Metrics.FocusRing, -Metrics.FocusRing);
        var radius = Metrics.Scale(button, Metrics.ControlRadius);
        DesignPaint.Surface(graphics, bounds, radius, state.Fill, state.Variant == ButtonVariant.Secondary ? state.Border : null);
        if (button.Enabled && state.Variant is ButtonVariant.Primary or ButtonVariant.Danger && bounds.Height > 0)
        {
            using var path = DesignPaint.RoundedRect(bounds, radius);
            using var gradient = new LinearGradientBrush(bounds, Theme.Lerp(state.Fill, Palette.Surface, .03f), state.Fill, LinearGradientMode.Vertical);
            graphics.FillPath(gradient, path);
            using var highlight = new Pen(Palette.WithAlpha(Palette.Surface, Metrics.HighlightAlpha));
            graphics.DrawLine(highlight, bounds.Left + radius, bounds.Top + Metrics.Border, bounds.Right - radius, bounds.Top + Metrics.Border);
        }
        if (state.Focus > 0 && button.Enabled)
        {
            using var ring = DesignPaint.RoundedRect(button.ClientRectangle with { Width = button.Width - Metrics.Border, Height = button.Height - Metrics.Border }, radius + Metrics.FocusRing);
            using var pen = new Pen(Palette.WithAlpha(Palette.BrandAccent, .4f * state.Focus), Metrics.FocusRing);
            graphics.DrawPath(pen, ring);
        }
        var textColor = !button.Enabled ? Palette.Ink400 : state.Variant is ButtonVariant.Primary or ButtonVariant.Danger ? Palette.Surface : state.Hover ? Palette.BrandSoftText : Palette.Ink700;
        var content = Rectangle.Inflate(bounds, -Space.Md, -Space.Xs);
        content.Offset(0, (int)Math.Round(state.Offset));
        if (state.Icon.HasValue || state.Busy)
        {
            var iconBounds = new Rectangle(content.Left, content.Top + (content.Height - Metrics.IconSize) / 2, Metrics.IconSize, Metrics.IconSize);
            if (state.Busy) { using var pen = new Pen(textColor, Metrics.FocusRing); graphics.DrawArc(pen, iconBounds, state.Phase * 360, 250); }
            else if (state.Icon is IconKind kind) Icons.Draw(graphics, kind, iconBounds, textColor);
            content.X += Metrics.IconSize + Space.Sm; content.Width -= Metrics.IconSize + Space.Sm;
        }
        var text = state.Busy && button is AppButton app ? app.BusyText : button.Text;
        TextRenderer.DrawText(graphics, text, button.Font, content, textColor, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
    }
}
