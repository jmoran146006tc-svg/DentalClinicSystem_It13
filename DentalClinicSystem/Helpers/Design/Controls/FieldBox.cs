namespace DentalClinicSystem.Helpers.Design.Controls;

public enum FieldKind { Text, Search, Password, Choice, Date }

// Field captions/helpers stay in FormField; AntdUI paints the complete input,
// focus ring, clear action, dropdown, calendar and password action.
public sealed class FieldBox : Panel
{
    private readonly AntdUI.Input? _editor;
    private IconKind? _leadingIcon;
    private Color? _borderOverride;
    public Control Input { get; }
    [System.ComponentModel.DefaultValue(null)]
    public IconKind? LeadingIcon
    {
        get => _leadingIcon;
        set { _leadingIcon = value; if (_editor is not null) _editor.PrefixSvg = value.HasValue ? AntdTheme.Svg(value.Value) : null; }
    }
    [System.ComponentModel.DefaultValue(null)]
    public Color? BorderOverride { get => _borderOverride; set { _borderOverride = value; if (_editor is not null) _editor.BorderColor = value ?? Palette.LineStrong; } }
    public Rectangle LeadingIconBounds => _leadingIcon.HasValue ? new(Space.Md, (Height - Metrics.IconSize) / 2, Metrics.IconSize, Metrics.IconSize) : Rectangle.Empty;
    public FieldBox(Control input, FieldKind kind = FieldKind.Text)
    {
        _editor = input as AntdUI.Input;
        Theme.MarkPrimitive(this); Input = input;
        Height = Metrics.ControlHeight; Width = Metrics.FormWidth;
        BackColor = Palette.Transparent; TabStop = false;
        input.Visible = true; input.Dock = DockStyle.Fill; input.Margin = Padding.Empty;
        input.Font = Typography.Body;
        Controls.Add(input);
        // Checkbox fields and read-only calculated labels keep their own renderer.
        if (_editor is null) { MinimumSize = new(0, Metrics.ControlHeight); return; }
        _editor.Radius = Metrics.ControlRadius; _editor.BorderColor = Palette.LineStrong;
        _editor.BorderHover = _editor.BorderActive = Palette.Brand;
        _editor.BackColor = Palette.Surface; _editor.ForeColor = Palette.Ink700;
        _editor.AllowClear = kind == FieldKind.Search;
        if (kind == FieldKind.Search)
        {
            LeadingIcon = IconKind.Search;
            input.KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) { _editor.Clear(); e.SuppressKeyPress = true; } };
        }
        if (kind == FieldKind.Password)
        {
            _editor.UseSystemPasswordChar = true;
            _editor.SuffixSvg = AntdTheme.Svg(IconKind.Eye);
            _editor.SuffixClick += (_, _) => TogglePassword();
        }
        if (_editor.Multiline) { Height = Metrics.FieldHeight; MinimumSize = new(0, Metrics.FieldHeight); }
        else MinimumSize = new(0, Metrics.ControlHeight);
    }
    public void TogglePassword()
    {
        if (!Input.Enabled || _editor is null) return;
        _editor.UseSystemPasswordChar = !_editor.UseSystemPasswordChar;
        _editor.SuffixSvg = AntdTheme.Svg(_editor.UseSystemPasswordChar ? IconKind.Eye : IconKind.EyeOff);
        _editor.AccessibleDescription = _editor.UseSystemPasswordChar ? "Password hidden. Alt+P to show." : "Password visible. Alt+P to hide.";
        _editor.Focus();
    }
    public static FormField Wrap(Control input, string caption, FieldKind kind = FieldKind.Text) => new(caption, new FieldBox(input, kind));
    public void SetError(bool error) { if (_editor is not null) _editor.Status = error ? AntdUI.TType.Error : AntdUI.TType.None; }
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
        Box = box; ColumnCount = 1; RowCount = 3; Width = Metrics.FormWidth; BackColor = Palette.Transparent;
        Margin = new Padding(0, Space.Sm, 0, Space.Sm);
        AutoSize = false;
        ColumnStyles.Add(new(SizeType.Percent, 100));
        RowStyles.Add(new(SizeType.AutoSize)); RowStyles.Add(new(SizeType.AutoSize)); RowStyles.Add(new(SizeType.AutoSize));
        var label = new Label { Text = caption, Font = Typography.Label, ForeColor = Palette.Ink500, AutoSize = true, Margin = new Padding(Space.Xs, 0, 0, Space.Xs) }; _caption = label;
        box.Input.AccessibleName = caption; box.Dock = DockStyle.Fill; box.Margin = Padding.Empty;
        label.BackColor = _message.BackColor = Palette.Transparent; _message.Margin = Padding.Empty; _message.Visible = false;
        Controls.Add(label, 0, 0); Controls.Add(box, 0, 1); Controls.Add(_message, 0, 2);
        box.SizeChanged += (_, _) => FitHeight(); FitHeight();
        if (box.Input is TextBox { Multiline: true } text) text.TextChanged += (_, _) => SetHelper(text.Text.Length >= text.MaxLength * .8 ? $"{text.Text.Length}/{text.MaxLength}" : string.Empty);
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
