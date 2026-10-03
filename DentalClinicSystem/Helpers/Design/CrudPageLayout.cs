using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers.Design;

// Layout composition only: pages keep their controls, events, models and services.
public sealed class CrudPageLayout
{
    private readonly string _singular;
    private readonly Button _save;
    private readonly Label _title;
    private readonly TableLayoutPanel _fields = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = Palette.Surface };
    private int _row;
    private readonly List<Control> _inputs = [];
    private readonly Panel _gridContent;
    private bool _loaded;
    public FlowLayoutPanel Toolbar { get; }
    public InlineAlert Alert { get; } = new() { Visible = false, Dock = DockStyle.Fill };
    public RoundedPanel FormCard { get; }
    public TextBox Search { get; } = new();
    public Toggle ShowInactive { get; } = new();
    public AppButton NewButton { get; }
    public FlowLayoutPanel Actions { get; }
    public CrudPageLayout(UserControl page, string title, string singular, string subtitle, DataGridView grid, Button save, Button clear, Action clearForm)
    {
        _singular = singular; _save = save;
        page.SuspendLayout(); foreach (Control old in page.Controls) old.Visible = false;
        page.BackColor = Palette.Canvas;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = Space.Page, BackColor = Palette.Canvas };
        Theme.MarkPrimitive(root); root.ColumnStyles.Add(new(SizeType.Percent, 100));
        root.RowStyles.Add(new(SizeType.Absolute, Metrics.FieldHeight)); root.RowStyles.Add(new(SizeType.AutoSize)); root.RowStyles.Add(new(SizeType.Percent, 100));
        NewButton = UiFactory.Button($"New {singular}", icon: IconKind.Plus);
        NewButton.Click += (_, _) => { clearForm(); FormCard?.SelectNextControl(null, true, true, true, false); };
        root.Controls.Add(new PageHeader(title, subtitle, NewButton), 0, 0);
        Toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, BackColor = Palette.Canvas, Margin = new Padding(0, 0, 0, Space.Lg) };
        Search.PlaceholderText = $"Search {title.ToLowerInvariant()}"; Toolbar.Controls.Add(UiFactory.Search(Search));
        root.Controls.Add(Toolbar, 0, 1);
        var gridCard = UiFactory.Card(); grid.Visible = true; grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.MultiSelect = false;
        GridTheme.Apply(grid); gridCard.Content.Controls.Add(grid); _gridContent = gridCard.Content;
        FormCard = UiFactory.Card(); FormCard.Content.AutoScroll = true;
        _title = new Label { Text = $"New {singular}", Font = Typography.Heading, ForeColor = Palette.Ink900, AutoSize = true, Margin = new Padding(0, 0, 0, Space.Lg) };
        _fields.ColumnStyles.Add(new(SizeType.Percent, 50)); _fields.ColumnStyles.Add(new(SizeType.Percent, 50));
        _fields.Controls.Add(_title, 0, 0); _fields.SetColumnSpan(_title, 2); _fields.Controls.Add(Alert, 0, 1); _fields.SetColumnSpan(Alert, 2); _row = 2;
        Actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = Palette.Surface };
        save.Visible = clear.Visible = true; save.Dock = clear.Dock = DockStyle.None; save.Height = clear.Height = Metrics.ControlHeight;
        ButtonStyler.Attach(save, ButtonVariant.Primary); ButtonStyler.Attach(clear, ButtonVariant.Ghost);
        clear.Text = "Clear"; Actions.Controls.AddRange([save, clear]); SetEditing(false);
        var formBody = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 2, BackColor = Palette.Surface };
        formBody.ColumnStyles.Add(new(SizeType.Percent, 100)); formBody.RowStyles.Add(new(SizeType.AutoSize)); formBody.RowStyles.Add(new(SizeType.AutoSize));
        Actions.Dock = DockStyle.Fill; formBody.Controls.Add(_fields, 0, 0); formBody.Controls.Add(Actions, 0, 1); FormCard.Content.Controls.Add(formBody);
        root.Controls.Add(new ResponsiveSplit(gridCard, FormCard, () => formBody.PreferredSize.Height + FormCard.Padding.Vertical + Space.Xl), 0, 2);
        page.Controls.Add(root); root.BringToFront(); UiMessages.RegisterAlertHost(page, Alert);
        var keys = new KeyboardShortcuts(page); keys.Register(Keys.Control | Keys.F, () => Search.Focus());
        keys.Register(Keys.Control | Keys.N, () => NewButton.PerformClick()); keys.Register(Keys.Control | Keys.S, save.PerformClick);
        keys.Register(Keys.Escape, () => { if (Search.ContainsFocus) Search.Clear(); else clearForm(); });
        keys.RegisterEnterNavigation(_inputs, save.PerformClick);
        Tooltips.Attach(NewButton, "New record · Ctrl+N"); Tooltips.Attach(save, "Save · Ctrl+S"); Tooltips.Attach(Search, "Search · Ctrl+F; Esc to clear");
        page.ResumeLayout(true);
    }
    public void AddRow(params FormField[] fields)
    {
        _inputs.AddRange(fields.Select(field => field.Box.Input));
        foreach (var field in fields) { field.Dock = DockStyle.Fill; field.Margin = new Padding(0, Space.Xs, fields.Length > 1 ? Space.Sm : 0, Space.Sm); }
        for (var col = 0; col < fields.Length; col++) _fields.Controls.Add(fields[col], col, _row);
        if (fields.Length == 1) _fields.SetColumnSpan(fields[0], 2);
        _fields.RowStyles.Add(new(SizeType.AutoSize)); _row++;
    }
    public IDisposable Loading()
    {
        if (_loaded) return new LoadScope(() => { });
        _loaded = true; var skeleton = new Skeleton { Dock = DockStyle.Fill };
        _gridContent.Controls.Add(skeleton); skeleton.BringToFront();
        return new LoadScope(() => Reveal(skeleton));
    }
    private sealed class LoadScope(Action finish) : IDisposable { public void Dispose() => finish(); }
    private void Reveal(Skeleton skeleton)
    {
        Bitmap? previous = null; Bitmap? next = null;
        try
        {
            if (_gridContent.IsDisposed || !_gridContent.Visible || _gridContent.FindForm()?.WindowState == FormWindowState.Minimized || !Motion.Motion.Enabled || _gridContent.Width <= 0 || _gridContent.Height <= 0) return;
            previous = new(_gridContent.Width, _gridContent.Height); _gridContent.DrawToBitmap(previous, _gridContent.ClientRectangle);
            skeleton.Dispose(); next = new(_gridContent.Width, _gridContent.Height); _gridContent.DrawToBitmap(next, _gridContent.ClientRectangle);
            var reveal = new RecordReveal(previous, next) { Dock = DockStyle.Fill }; previous = next = null;
            _gridContent.Controls.Add(reveal); reveal.BringToFront();
            if (reveal.FindForm() is { } form)
            {
                EventHandler minimized = (_, _) => { if (form.WindowState == FormWindowState.Minimized) reveal.Dispose(); };
                form.Resize += minimized; reveal.Disposed += (_, _) => form.Resize -= minimized;
            }
            Motion.Motion.Animator.Run(reveal, "records-reveal", 0, 1, Motion.Motion.Fast, Motion.Easing.EaseOutCubic, value => { reveal.Progress = value; reveal.Invalidate(); }, reveal.Dispose);
        }
        catch (Exception error) { AppLog.Write(error); }
        finally { skeleton.Dispose(); previous?.Dispose(); next?.Dispose(); }
    }
    private sealed class RecordReveal(Bitmap previous, Bitmap next) : DesignControl
    {
        [System.ComponentModel.DefaultValue(0f)] public float Progress { get; set; }
        protected override void OnVisibleChanged(EventArgs e) { base.OnVisibleChanged(e); if (!Visible && !Disposing && !IsDisposed) Dispose(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(next, ClientRectangle);
            using var attributes = new System.Drawing.Imaging.ImageAttributes();
            attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix { Matrix33 = Math.Clamp(1 - Progress, 0, 1) });
            e.Graphics.DrawImage(previous, ClientRectangle, 0, 0, previous.Width, previous.Height, GraphicsUnit.Pixel, attributes);
        }
        protected override void Dispose(bool disposing) { if (disposing) { previous.Dispose(); next.Dispose(); } base.Dispose(disposing); }
    }
    public void SetEditing(bool editing)
    {
        _title.Text = $"{(editing ? "Edit" : "New")} {_singular}";
        _save.Text = editing ? $"Update {_singular}" : $"Add {_singular}";
    }
}
