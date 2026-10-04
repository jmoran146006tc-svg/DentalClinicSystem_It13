using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public enum FieldKind { Text, Search, Password, Choice, Date }

public sealed class FieldBox : Panel
{
    private readonly Panel _viewport = new() { BackColor = Palette.Surface };
    private readonly FieldActionButton _action;
    private readonly FieldKind _kind;
    private Color _border = Palette.LineStrong;
    private float _focus;
    private bool _error;
    private bool _layingOut;
    private ToolStripDropDown? _calendar;
    public Control Input { get; }
    public FieldBox(Control input, FieldKind kind = FieldKind.Text)
    {
        Theme.MarkPrimitive(this);
        DesignPaint.Enable(this);
        Input = input; _kind = kind;
        input.Visible = true;
        Height = Metrics.ControlHeight; Width = Metrics.FormWidth; TabStop = false; BackColor = Palette.Surface;
        input.Font = Typography.Body; input.ForeColor = Palette.Ink700; input.BackColor = Palette.Surface;
        input.Dock = DockStyle.None; input.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        input.Margin = Padding.Empty;
        _action = new FieldActionButton(kind == FieldKind.Password ? IconKind.Eye : kind is FieldKind.Choice or FieldKind.Date ? IconKind.ChevronDown : IconKind.Close)
        { Width = Metrics.CompactHeight, AccessibleName = kind == FieldKind.Password ? "Show password" : kind == FieldKind.Choice ? "Open choices" : kind == FieldKind.Date ? "Open calendar" : "Clear text", TabStop = kind != FieldKind.Choice };
        Tooltips.Attach(_action, _action.AccessibleName ?? "Field action");
        Controls.Add(_viewport); _viewport.Controls.Add(input); Controls.Add(_action);
        if (input is TextBox text)
        {
            text.BorderStyle = BorderStyle.None;
            if (kind == FieldKind.Password) text.UseSystemPasswordChar = true;
            text.TextChanged += (_, _) => LayoutInput();
            text.KeyDown += (_, e) => { if (kind == FieldKind.Search && e.KeyCode == Keys.Escape) { text.Clear(); e.SuppressKeyPress = true; } };
        }
        if (input is ComboBox combo)
        {
            // Clip the standard frame in the viewport. FlatStyle.Flat also paints
            // an interior underline, which survives clipping at scaled DPI.
            combo.FlatStyle = FlatStyle.Standard; combo.DropDownStyle = ComboBoxStyle.DropDownList; combo.DrawMode = DrawMode.OwnerDrawFixed;
            combo.ItemHeight = input.Font.Height + Space.Xs; combo.DrawItem += DrawChoice;
        }
        input.GotFocus += (_, _) => AnimateFocus(true);
        input.LostFocus += (_, _) => AnimateFocus(false);
        _action.GotFocus += (_, _) => AnimateFocus(true);
        _action.LostFocus += (_, _) => AnimateFocus(Input.Focused);
        _action.Click += (_, _) => ActivateAction();
        SizeChanged += (_, _) => LayoutInput();
        input.FontChanged += (_, _) => LayoutInput();
        input.SizeChanged += (_, _) => LayoutInput();
        DpiChangedAfterParent += (_, _) => LayoutInput();
        EnabledChanged += (_, _) => { input.Enabled = Enabled; _action.Enabled = Enabled; AnimateFocus(input.Focused); };
        LayoutInput();
    }
    public static FormField Wrap(Control input, string caption, FieldKind kind = FieldKind.Text) => new(caption, new FieldBox(input, kind));
    private void LayoutInput()
    {
        if (Input.IsDisposed || _layingOut) return;
        _layingOut = true;
        try
        {
        var inset = Metrics.Scale(this, Space.Sm);
        // ComboBox/DateTimePicker constrain their actual native height. Using a
        // larger PreferredSize leaves their bottom frame inside the viewport.
        var nativeHeight = Input is ComboBox or DateTimePicker ? Input.Height : Input.PreferredSize.Height;
        MinimumSize = new(0, Math.Max(Metrics.Scale(this, Metrics.ControlHeight), nativeHeight + inset * 2));
        if (Input is TextBox { Multiline: true }) MinimumSize = new(0, Metrics.Scale(this, Metrics.FieldHeight));
        var leading = _kind == FieldKind.Search ? Metrics.Scale(this, Metrics.IconSize + Space.Sm) : 0;
        var action = _kind is FieldKind.Password or FieldKind.Choice or FieldKind.Date || (_kind == FieldKind.Search && Input.Text.Length > 0);
        _action.Visible = action;
        var actionSize = Metrics.Scale(this, Metrics.CompactHeight);
        var rightInset = Metrics.Scale(this, Space.Md);
        _action.SetBounds(Math.Max(0, Width - actionSize - rightInset), (Height - actionSize) / 2, actionSize, actionSize);
        var chrome = Input is ComboBox or DateTimePicker ? Metrics.Scale(this, Space.Xs) : 0;
        var viewportHeight = Input is TextBox { Multiline: true } ? Height - inset * 2 : nativeHeight - chrome * 2;
        _viewport.SetBounds(rightInset + leading, Math.Max(inset, (Height - viewportHeight) / 2), Math.Max(0, Width - rightInset * 2 - leading - (action ? actionSize + inset : 0)), Math.Max(0, viewportHeight));
        var nativeExtra = Input is ComboBox or DateTimePicker ? SystemInformation.VerticalScrollBarWidth + inset : 0;
        Input.SetBounds(-chrome, -chrome, _viewport.Width + nativeExtra + chrome * 2, Input is TextBox { Multiline: true } ? _viewport.Height : nativeHeight);
        Invalidate();
        }
        finally { _layingOut = false; }
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
                _action.Icon = text.UseSystemPasswordChar ? IconKind.Eye : IconKind.EyeOff; _action.Invalidate();
            }
            else text.Clear();
            text.Focus();
        }
        else if (Input is ComboBox combo) { combo.Focus(); combo.DroppedDown = true; }
        else if (Input is DateTimePicker date)
        {
            _calendar?.Dispose();
            var calendar = new MonthCalendar { MaxSelectionCount = 1, MinDate = date.MinDate, MaxDate = date.MaxDate };
            calendar.SetDate(date.Value.Date);
            _calendar = new ToolStripDropDown { Padding = Padding.Empty, BackColor = Palette.Surface };
            _calendar.Items.Add(new ToolStripControlHost(calendar) { Margin = Padding.Empty, Padding = Padding.Empty });
            calendar.DateSelected += (_, e) => { date.Value = e.Start.Date + date.Value.TimeOfDay; _calendar.Close(); date.Focus(); };
            _calendar.Show(this, new Point(0, Height));
        }
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
        TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), combo.Font, Rectangle.Inflate(e.Bounds, -Space.Sm, 0), combo.Enabled ? Palette.Ink700 : Palette.Ink400, DesignPaint.TextFlags);
    }
    protected override void OnPaintBackground(PaintEventArgs e) => e.Graphics.Clear(DesignPaint.ParentBackground(this));
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        DesignPaint.Surface(e.Graphics, Rectangle.Inflate(ClientRectangle, -Metrics.FocusRing, -Metrics.FocusRing), Metrics.ControlRadius, Enabled ? Palette.Surface : Palette.SurfaceAlt, _border);
        if (_focus > 0)
        {
            using var path = DesignPaint.RoundedRect(Rectangle.Inflate(ClientRectangle, -Metrics.Border, -Metrics.Border), Metrics.ControlRadius + Metrics.FocusRing);
            using var pen = new Pen(Palette.WithAlpha(_error ? Palette.Danger.Text : Palette.BrandAccent, .4f * _focus), Metrics.FocusRing);
            e.Graphics.DrawPath(pen, path);
        }
        if (_kind == FieldKind.Search) Icons.Draw(e.Graphics, IconKind.Search, new(Space.Md, (Height - Metrics.IconSize) / 2, Metrics.IconSize, Metrics.IconSize), Palette.Ink500);
        base.OnPaint(e);
    }
    protected override void Dispose(bool disposing) { if (disposing) { _calendar?.Dispose(); MotionSystem.Animator.Cancel(this); } base.Dispose(disposing); }
}

// This action belongs inside a field: no independent surface or focus outline
// may overpaint the field's continuous border.
internal sealed class FieldActionButton : Button
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public IconKind Icon { get; set; }
    public FieldActionButton(IconKind icon)
    {
        Icon = icon; Theme.MarkPrimitive(this); DesignPaint.Enable(this);
        FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; BackColor = Palette.Surface;
        MouseEnter += (_, _) => Invalidate(); MouseLeave += (_, _) => Invalidate();
        GotFocus += (_, _) => Invalidate(); LostFocus += (_, _) => Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Enabled ? Palette.Surface : Palette.SurfaceAlt);
        var size = Metrics.Scale(this, Metrics.IconSize);
        Icons.Draw(e.Graphics, Icon, new((Width - size) / 2, (Height - size) / 2, size, size),
            !Enabled ? Palette.Ink400 : Focused || ClientRectangle.Contains(PointToClient(MousePosition)) ? Palette.Brand : Palette.Ink500);
    }
}

public sealed class FormField : TableLayoutPanel
{
    private readonly Label _message = new() { Dock = DockStyle.Fill, Font = Typography.Caption, ForeColor = Palette.Ink500, AutoSize = true };
    private Label? _caption;
    private bool _resizing;
    public FieldBox Box { get; }
    public FormField(string caption, FieldBox box)
    {
        Theme.MarkPrimitive(this);
        Box = box; ColumnCount = 1; RowCount = 3; Width = Metrics.FormWidth; BackColor = Color.Transparent;
        Margin = new Padding(0, Space.Sm, 0, Space.Sm);
        AutoSize = false;
        ColumnStyles.Add(new(SizeType.Percent, 100));
        RowStyles.Add(new(SizeType.AutoSize)); RowStyles.Add(new(SizeType.AutoSize)); RowStyles.Add(new(SizeType.AutoSize));
        var label = new Label { Text = caption, Font = Typography.Label, ForeColor = Palette.Ink500, AutoSize = true, Margin = new Padding(Space.Xs, 0, 0, Space.Xs) }; _caption = label;
        box.Input.AccessibleName = caption; box.Dock = DockStyle.Fill; box.Margin = Padding.Empty;
        label.BackColor = _message.BackColor = Color.Transparent; _message.Margin = Padding.Empty; _message.Visible = false;
        Controls.Add(label, 0, 0); Controls.Add(box, 0, 1); Controls.Add(_message, 0, 2);
        box.SizeChanged += (_, _) => FitHeight(); FitHeight();
        if (box.Input is TextBox { Multiline: true } text) text.TextChanged += (_, _) => SetHelper(text.TextLength >= text.MaxLength * .8 ? $"{text.TextLength}/{text.MaxLength}" : string.Empty);
    }
    private void FitHeight()
    {
        if (_resizing || _caption is null) return;
        _resizing = true;
        try { Height = _caption.PreferredHeight + _caption.Margin.Vertical + Math.Max(Box.Height, Box.MinimumSize.Height)
                + (_message.Text.Length > 0 ? _message.PreferredHeight : 0) + Padding.Vertical; }
        finally { _resizing = false; }
    }
    protected override void OnLayout(LayoutEventArgs e) { FitHeight(); base.OnLayout(e); }
    public void SetHelper(string message) { _message.Text = message; _message.Visible = message.Length > 0; _message.ForeColor = Palette.Ink500; FitHeight(); PerformLayout(); }
    public void SetError(string message) { _message.Text = message; _message.Visible = message.Length > 0; _message.ForeColor = Palette.Danger.Text; Box.SetError(message.Length > 0); FitHeight(); PerformLayout(); }
}
