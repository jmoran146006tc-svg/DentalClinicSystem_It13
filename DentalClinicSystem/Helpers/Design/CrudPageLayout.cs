using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers.Design;

// Layout composition only: pages keep their controls, events, models and services.
public sealed class CrudPageLayout
{
    private readonly string _singular;
    private readonly Button _save;
    private readonly Label _title;
    // Keep bound selections while the same fields move between page and dialog.
    private readonly TableLayoutPanel _fields = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = Palette.Surface, BindingContext = new BindingContext() };
    private int _row;
    private readonly List<Control> _inputs = [];
    private readonly Panel _gridContent;
    private readonly UserControl _page;
    private readonly ClinicTable _grid;
    private Func<Task<bool>>? _saveModal;
    private Func<Task>? _afterSave;
    private Func<Task>? _onEditorOpened;
    private bool _editing;
    private bool _dialogOpen;
    private RecordDialog? _dialog;
    public InlineAlert ActiveAlert => _dialog?.Alert ?? Alert;
    public bool IsEditorOpen => _dialog is { IsDisposed: false };
    private readonly AppButton _edit = UiFactory.Button("Edit", ButtonVariant.Secondary, IconKind.Edit);
    private bool _loaded;
    public FlowLayoutPanel Toolbar { get; }
    public InlineAlert Alert { get; } = new() { Visible = false, Dock = DockStyle.Top };
    public RoundedPanel FormCard { get; }
    public TextBox Search { get; } = new();
    public Toggle ShowInactive { get; } = new();
    public AppButton NewButton { get; }
    public FlowLayoutPanel Actions { get; }
    public CrudPageLayout(UserControl page, string title, string singular, string subtitle, ClinicTable grid, Button save, Button clear, Action clearForm)
    {
        _singular = singular; _save = save; _page = page; _grid = grid;
        page.SuspendLayout(); foreach (Control old in page.Controls) old.Visible = false;
        page.BackColor = Palette.Canvas; page.AutoScroll = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = Space.Page, BackColor = Palette.Canvas };
        Theme.MarkPrimitive(root); root.ColumnStyles.Add(new(SizeType.Percent, 100));
        root.RowStyles.Add(new(SizeType.AutoSize)); root.RowStyles.Add(new(SizeType.AutoSize)); root.RowStyles.Add(new(SizeType.Percent, 100));
        NewButton = UiFactory.Button($"New {singular}", icon: IconKind.Plus);
        NewButton.Click += async (_, _) => { clearForm(); await OpenEditorAsync(); };
        root.Controls.Add(new PageHeader(title, subtitle, NewButton), 0, 0);
        Toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, BackColor = Palette.Canvas, Margin = new Padding(0, 0, 0, Space.Lg) };
        Search.PlaceholderText = $"Search {title.ToLowerInvariant()}"; var search = UiFactory.Search(Search); search.Width = Metrics.FormWidth - Space.Xxxl; Toolbar.Controls.Add(search);
        root.Controls.Add(Toolbar, 0, 1);
        var gridCard = UiFactory.Card(); gridCard.Dock = DockStyle.Fill; gridCard.Margin = Padding.Empty; grid.Visible = true; grid.Dock = DockStyle.Fill;
        grid.MultipleRows = false;
        GridTheme.Apply(grid); gridCard.Content.Controls.Add(grid); _gridContent = gridCard.Content;
        FormCard = UiFactory.Card(); FormCard.Content.AutoScroll = true; FormCard.Visible = false;
        _title = new Label { Text = $"New {singular}", Font = Typography.Heading, ForeColor = Palette.Ink900, AutoSize = true, Margin = new Padding(0, 0, 0, Space.Lg) };
        _fields.ColumnStyles.Add(new(SizeType.Percent, 50)); _fields.ColumnStyles.Add(new(SizeType.Percent, 50));
        _row = 0; _title.Visible = false;
        Actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, BackColor = Palette.Surface };
        save.Visible = clear.Visible = true; save.Dock = clear.Dock = DockStyle.None; save.Height = clear.Height = Metrics.ControlHeight;
        ButtonStyler.Attach(save, ButtonVariant.Primary); ButtonStyler.Attach(clear, ButtonVariant.Ghost);
        clear.Text = "Clear"; save.Visible = clear.Visible = false; SetEditing(false);
        var formBody = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 2, BackColor = Palette.Surface };
        formBody.ColumnStyles.Add(new(SizeType.Percent, 100)); formBody.RowStyles.Add(new(SizeType.AutoSize)); formBody.RowStyles.Add(new(SizeType.AutoSize));
        formBody.Controls.Add(_fields, 0, 0); FormCard.Content.Controls.Add(formBody);
        gridCard.Content.Controls.Add(Alert); grid.BringToFront();
        root.Controls.Add(gridCard, 0, 2);
        page.Controls.Add(root); root.BringToFront(); UiMessages.RegisterAlertHost(page, Alert);
        var keys = new KeyboardShortcuts(page); keys.Register(Keys.Control | Keys.F, () => Search.Focus());
        keys.Register(Keys.Control | Keys.N, () => NewButton.PerformClick()); keys.Register(Keys.Control | Keys.S, _edit.PerformClick);
        keys.Register(Keys.Escape, () => { if (Search.ContainsFocus) Search.Clear(); else clearForm(); });
        Tooltips.Attach(NewButton, "New record · Ctrl+N"); Tooltips.Attach(save, "Save · Ctrl+S"); Tooltips.Attach(Search, "Search · Ctrl+F; Esc to clear");
        page.ResumeLayout(true);
        page.Disposed += (_, _) => { FormCard.Dispose(); _title.Dispose(); };
        Toolbar.Name = "recordsToolbar"; ToolbarLayout.Attach(Toolbar);
    }
    public void UseModal(Func<Task<bool>> save, Func<Task> afterSave, bool allowEdit = true, Func<Task>? onOpen = null)
    {
        _saveModal = save; _afterSave = afterSave; _onEditorOpened = onOpen; FormCard.Visible = false;
        _edit.Text = $"Edit {_singular}"; _edit.Visible = allowEdit && NewButton.Visible; _edit.Enabled = false;
        Toolbar.Controls.Add(_edit); Actions.BackColor = Palette.Canvas; Actions.Dock = DockStyle.None; Toolbar.Controls.Add(Actions);
        _edit.Click += async (_, _) => { if (_editing) await OpenEditorAsync(); };
        _grid.SelectionChanged += (_, _) => _edit.Enabled = _grid.SelectedRecord is not null;
        if (allowEdit) _grid.CellDoubleClick += async (_, e) => { if (_grid.Records.Contains(e.Record) && _editing && NewButton.Visible) await OpenEditorAsync(); };
    }
    public void UseRefresh(Func<Task> reload)
    {
        var refresh = UiFactory.Button("Refresh", ButtonVariant.Secondary); refresh.Name = "pageRefresh";
        refresh.Click += async (_, _) =>
        {
            await UiAction.RunAsync(_page, async () => { Alert.Dismiss(); await reload(); }, refresh);
            if (!refresh.IsDisposed) refresh.Text = Alert.Visible ? "Retry" : "Refresh";
        };
        Alert.VisibleChanged += (_, _) => { if (!refresh.IsBusy) refresh.Text = Alert.Visible ? "Retry" : "Refresh"; };
        Toolbar.Controls.Add(refresh);
    }
    public async Task OpenEditorAsync()
    {
        if (_dialogOpen || _saveModal is null || !_page.Enabled || !NewButton.Visible) return;
        _dialogOpen = true;
        try
        {
            using RecordDialog dialog = _singular == "patient" ? new PatientDialog(_fields, _saveModal, _editing)
                : new RecordDialog($"{(_editing ? "Edit" : "New")} {_singular}", _fields, _saveModal);
            _dialog = dialog;
            if (_onEditorOpened is { } onOpen)
                dialog.Shown += async (_, _) => await UiAction.RunAsync(dialog, onOpen);
            var result = dialog.ShowDialog(_page.FindForm());
            // The reusable fields belong to the page, not to the temporary dialog.
            _fields.Parent?.Controls.Remove(_fields); FormCard.Content.Controls.Add(_fields);
            _dialog = null;
            if (result == DialogResult.OK && _afterSave is not null)
                await UiAction.RunAsync(_page, _afterSave);
        }
        finally { _dialogOpen = false; _dialog = null; }
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
        _grid.Visible = false;
        _gridContent.Controls.Add(skeleton); skeleton.BringToFront();
        return new LoadScope(() =>
        {
            skeleton.Dispose();
            if (!_gridContent.IsDisposed) { _grid.Visible = true; _gridContent.Invalidate(true); }
        });
    }
    private sealed class LoadScope(Action finish) : IDisposable { public void Dispose() => finish(); }
    public void SetEditing(bool editing)
    {
        _editing = editing;
        _title.Text = $"{(editing ? "Edit" : "New")} {_singular}";
        _save.Text = editing ? $"Update {_singular}" : $"Add {_singular}";
    }
}
