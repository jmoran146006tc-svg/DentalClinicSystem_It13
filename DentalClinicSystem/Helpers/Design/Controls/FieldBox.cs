using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public enum FieldKind { Text, Search, Password, Choice, Date }

public sealed class FieldBox : Panel
{
    private readonly Panel _viewport = new() { BackColor = Palette.Surface };
    private readonly AppButton _action;
    private readonly FieldKind _kind;
    private Color _border = Palette.LineStrong;
    private float _focus;
    private bool _error;
    public Control Input { get; }
    public FieldBox(Control input, FieldKind kind = FieldKind.Text)
    {
        DesignPaint.Enable(this);
        Input = input; _kind = kind;
        Height = Metrics.ControlHeight; Width = Metrics.FormWidth; TabStop = false; BackColor = Palette.Surface;
        input.Font = Typography.Body; input.ForeColor = Palette.Ink700; input.BackColor = Palette.Surface;
        input.Dock = DockStyle.None; input.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
        input.Margin = Padding.Empty;
        _action = new AppButton(string.Empty, ButtonVariant.Ghost, kind == FieldKind.Password ? IconKind.Eye : kind == FieldKind.Choice ? IconKind.ChevronDown : IconKind.Close, ButtonSize.Compact)
        { Width = Metrics.CompactHeight, AccessibleName = kind == FieldKind.Password ? "Show password" : kind == FieldKind.Choice ? "Open choices" : "Clear text", TabStop = kind != FieldKind.Choice };
        Tooltips.Attach(_action, _action.AccessibleName ?? "Field action");
        Controls.Add(_viewport); _viewport.Controls.Add(input); Controls.Add(_action);
        if (input is TextBox text)
        {
            text.BorderStyle = BorderStyle.None;
            if (kind == FieldKind.Password) text.UseSystemPasswordChar = true;
            text.TextChanged += (_, _) => LayoutInput();
            text.KeyDown += (_, e) => { if (kind == FieldKind.Search && e.KeyCode == Keys.Escape) { text.Clear(); e.SuppressKeyPress = true; } };
        }
        if (input is ComboBox combo) { combo.DropDownStyle = ComboBoxStyle.DropDownList; combo.DrawMode = DrawMode.OwnerDrawFixed; combo.DrawItem += DrawChoice; }
        input.GotFocus += (_, _) => AnimateFocus(true);
        input.LostFocus += (_, _) => AnimateFocus(false);
        _action.Click += (_, _) => ActivateAction();
        SizeChanged += (_, _) => LayoutInput();
        EnabledChanged += (_, _) => { input.Enabled = Enabled; _action.Enabled = Enabled; AnimateFocus(input.Focused); };
        LayoutInput();
    }
    public static FormField Wrap(Control input, string caption, FieldKind kind = FieldKind.Text) => new(caption, new FieldBox(input, kind));
    private void LayoutInput()
    {
        if (Input.IsDisposed) return;
        var leading = _kind == FieldKind.Search ? Metrics.IconSize + Space.Sm : 0;
        var action = _kind is FieldKind.Password or FieldKind.Choice || (_kind == FieldKind.Search && Input.Text.Length > 0);
        _action.Visible = action;
        _action.SetBounds(Math.Max(0, Width - Metrics.CompactHeight - Space.Xs), (Height - Metrics.CompactHeight) / 2, Metrics.CompactHeight, Metrics.CompactHeight);
        _viewport.SetBounds(Space.Md + leading, Space.Sm, Math.Max(0, Width - Space.Xl - leading - (action ? Metrics.CompactHeight : 0)), Math.Max(0, Height - Space.Lg));
        var nativeExtra = Input is ComboBox ? SystemInformation.VerticalScrollBarWidth + Space.Sm : 0;
        Input.SetBounds(0, Math.Max(0, (_viewport.Height - Input.PreferredSize.Height) / 2), _viewport.Width + nativeExtra, Input is TextBox { Multiline: true } ? _viewport.Height : Input.PreferredSize.Height);
        Invalidate();
    }
    private void ActivateAction()
    {
        if (Input is TextBox text)
        {
            if (_kind == FieldKind.Password)
            {
                text.UseSystemPasswordChar = !text.UseSystemPasswordChar;
                _action.AccessibleName = text.UseSystemPasswordChar ? "Show password" : "Hide password";
                Tooltips.Attach(_action, _action.AccessibleName);
                ButtonStyler.Attach(_action, ButtonVariant.Ghost, text.UseSystemPasswordChar ? IconKind.Eye : IconKind.EyeOff);
            }
            else text.Clear();
            text.Focus();
        }
        else if (Input is ComboBox combo) { combo.Focus(); combo.DroppedDown = true; }
    }
    private void AnimateFocus(bool focused)
    {
        var border = _error ? Palette.Danger.Text : focused ? Palette.Brand : Palette.LineStrong;
        MotionSystem.Animator.RunColor(this, "field-border", _border, border, MotionSystem.Fast, value => { _border = value; Invalidate(); });
        MotionSystem.Animator.Run(this, "field-focus", _focus, focused ? 1 : 0, MotionSystem.Instant, Easing.EaseOutCubic, value => { _focus = value; Invalidate(); });
    }
    public void SetError(bool error)
    {
        _error = error; AnimateFocus(Input.Focused);
        if (error) MotionSystem.Animator.Run(this, "field-error", 0, 1, MotionSystem.Base, Easing.Linear,
            t => { _border = Theme.Lerp(Palette.Danger.Text, Palette.Danger.Background, MathF.Sin(t * MathF.PI) * .4f); Invalidate(); });
    }
    private void DrawChoice(object? sender, DrawItemEventArgs e)
    {
        if (Input is not ComboBox combo || e.Index < 0) return;
        var selected = e.State.HasFlag(DrawItemState.Selected);
        using var brush = new SolidBrush(selected ? Palette.BrandSoft : Palette.Surface);
        e.Graphics.FillRectangle(brush, e.Bounds);
        TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), Typography.Body, Rectangle.Inflate(e.Bounds, -Space.Sm, 0), Palette.Ink700, DesignPaint.TextFlags);
    }
    protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(Parent?.BackColor ?? Palette.Canvas);
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        DesignPaint.Surface(e.Graphics, Rectangle.Inflate(ClientRectangle, -Metrics.FocusRing, -Metrics.FocusRing), Metrics.ControlRadius, Enabled ? Palette.Surface : Palette.SurfaceAlt, _border);
        if (_focus > 0)
        {
            using var path = DesignPaint.RoundedRect(Rectangle.Inflate(ClientRectangle, -Metrics.Border, -Metrics.Border), Metrics.ControlRadius + Metrics.FocusRing);
            using var pen = new Pen(Palette.WithAlpha(Palette.BrandAccent, .4f * _focus), Metrics.FocusRing);
            e.Graphics.DrawPath(pen, path);
        }
        if (_kind == FieldKind.Search) Icons.Draw(e.Graphics, IconKind.Search, new(Space.Md, (Height - Metrics.IconSize) / 2, Metrics.IconSize, Metrics.IconSize), Palette.Ink500);
        base.OnPaint(e);
    }
    protected override void Dispose(bool disposing) { if (disposing) MotionSystem.Animator.Cancel(this); base.Dispose(disposing); }
}

public sealed class FormField : TableLayoutPanel
{
    private readonly Label _message = new() { Dock = DockStyle.Fill, Font = Typography.Caption, ForeColor = Palette.Ink500, AutoSize = true };
    public FieldBox Box { get; }
    public FormField(string caption, FieldBox box)
    {
        Box = box; ColumnCount = 1; RowCount = 3; Height = Metrics.FieldHeight; Width = Metrics.FormWidth; BackColor = Palette.Surface;
        Margin = new Padding(0, Space.Sm, 0, Space.Sm);
        RowStyles.Add(new(SizeType.AutoSize)); RowStyles.Add(new(SizeType.Absolute, Metrics.ControlHeight)); RowStyles.Add(new(SizeType.AutoSize));
        var label = new Label { Text = caption, Font = Typography.Label, ForeColor = Palette.Ink500, AutoSize = true, Margin = new Padding(Space.Xs, 0, 0, Space.Xs) };
        box.Input.AccessibleName = caption; box.Dock = DockStyle.Fill;
        Controls.Add(label, 0, 0); Controls.Add(box, 0, 1); Controls.Add(_message, 0, 2);
        if (box.Input is TextBox { Multiline: true } text) text.TextChanged += (_, _) => SetHelper(text.TextLength >= text.MaxLength * .8 ? $"{text.TextLength}/{text.MaxLength}" : string.Empty);
    }
    public void SetHelper(string message) { _message.Text = message; _message.ForeColor = Palette.Ink500; }
    public void SetError(string message) { _message.Text = message; _message.ForeColor = Palette.Danger.Text; Box.SetError(message.Length > 0); }
}
