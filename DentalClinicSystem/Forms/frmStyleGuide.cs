#if DEBUG
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Helpers.Native;
using DentalClinicSystem.Models;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Forms;

public sealed class frmStyleGuide : Form
{
    private readonly FlowLayoutPanel _sections = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = Space.Page, BackColor = Palette.Canvas };
    private readonly Label _activity = new() { Width = Metrics.FormWidth / 2, Height = Metrics.ControlHeight, ForeColor = Palette.Ink500, Font = Typography.Caption };
    public frmStyleGuide()
    {
        Text = "Dental Care — Phase 6 style guide"; Size = Metrics.MinimumWindow; MinimumSize = Metrics.MinimumWindow; BackColor = Palette.Canvas;
        Controls.Add(_sections);
        _sections.Controls.Add(new Label { Text = "Design system", Font = Typography.Display, ForeColor = Palette.Ink900, AutoSize = true });
        _sections.Controls.Add(new Label { Text = "Review at 100%, 125% and 150%. Use Tab and Shift+Tab to inspect keyboard focus.", Font = Typography.Body, ForeColor = Palette.Ink500, AutoSize = true });
        Colors(); TypeScale(); Buttons(); Fields(); Components(); IconsSection(); Grids(); MotionSection();
        _sections.SizeChanged += (_, _) => ResizeSections(); ResizeSections();
        WindowChrome.Apply(this); MotionSystem.Animator.ActivityChanged += OnActivity;
        OnActivity(this, EventArgs.Empty);
    }
    private FlowLayoutPanel Section(string title)
    {
        _sections.Controls.Add(new Label { Text = title, AutoSize = true, Font = Typography.Heading, ForeColor = Palette.Ink900, Margin = new Padding(0, Space.Xl, 0, Space.Sm) });
        var contents = new FlowLayoutPanel { AutoSize = true, WrapContents = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Palette.Canvas, Margin = new Padding(0, 0, 0, Space.Lg) };
        _sections.Controls.Add(contents); return contents;
    }
    private void ResizeSections()
    {
        var width = Math.Max(Metrics.FormWidth, _sections.ClientSize.Width - Space.Xxl * 2 - SystemInformation.VerticalScrollBarWidth);
        _sections.SuspendLayout();
        try { foreach (Control control in _sections.Controls) { control.MaximumSize = new(width, 0); control.Width = width; } }
        finally { _sections.ResumeLayout(true); }
    }
    private void Colors()
    {
        var colors = Section("Colors and contrast");
        (string Name, Color Value)[] tokens = [
            ("Canvas", Palette.Canvas), ("Surface", Palette.Surface), ("SurfaceAlt", Palette.SurfaceAlt), ("Line", Palette.Line), ("LineStrong", Palette.LineStrong),
            ("Ink900", Palette.Ink900), ("Ink700", Palette.Ink700), ("Ink500", Palette.Ink500), ("Ink400", Palette.Ink400), ("BrandAccent", Palette.BrandAccent),
            ("Brand", Palette.Brand), ("BrandHover", Palette.BrandHover), ("BrandPressed", Palette.BrandPressed), ("BrandSoft", Palette.BrandSoft), ("BrandSoftText", Palette.BrandSoftText), ("SidebarBg", Palette.SidebarBg)];
        foreach (var (name, value) in tokens)
        {
            var tile = new Panel { Size = new(Metrics.FormWidth / 3, Metrics.FieldHeight), Margin = new Padding(Space.Xs), BackColor = Palette.Surface };
            tile.Controls.Add(new Panel { BackColor = value, Dock = DockStyle.Top, Height = Metrics.ControlHeight });
            tile.Controls.Add(new Label { Text = $"{name}\n#{value.R:X2}{value.G:X2}{value.B:X2}", Dock = DockStyle.Bottom, Height = Metrics.NavHeight, Font = Typography.Caption, ForeColor = Palette.Ink700 }); colors.Controls.Add(tile);
        }
        foreach (var semantic in Enum.GetValues<Semantic>()) colors.Controls.Add(UiFactory.Badge(semantic.ToString(), semantic));
        foreach (var pair in Contrast.RequiredPairs()) colors.Controls.Add(new Label { Text = $"{pair.Name}: {Contrast.Ratio(pair.Text, pair.Background):F2}:1", AutoSize = true, Font = Typography.Caption, ForeColor = Palette.Ink500, Margin = new Padding(Space.Sm) });
    }
    private void TypeScale()
    {
        var typography = Section($"Typography · {Typography.FamilyName}");
        foreach (var (name, font) in new[] { ("Display 24", Typography.Display), ("Title 18", Typography.Title), ("Heading 13", Typography.Heading), ("Body 10", Typography.Body), ("Label 9", Typography.Label), ("Caption 8.5", Typography.Caption), ("KPI 26", Typography.KpiNumber) })
            typography.Controls.Add(new Label { Text = name, Font = font, ForeColor = Palette.Ink900, AutoSize = true, Margin = new Padding(Space.Sm) });
        var spacing = Section("Spacing · control radius 8 · card radius 12 · pill height / 2");
        foreach (var space in new[] { Space.Xs, Space.Sm, Space.Md, Space.Lg, Space.Xl, Space.Xxl, Space.Xxxl })
            spacing.Controls.Add(new Label { Text = space.ToString(), Size = new(space + Space.Xxl, Metrics.NavHeight), BackColor = Palette.BrandSoft, ForeColor = Palette.BrandSoftText, TextAlign = ContentAlignment.MiddleCenter, Font = Typography.Label, Margin = new Padding(Space.Sm) });
    }
    private void Buttons()
    {
        var buttons = Section("Buttons · hover, press, focus, disabled · regular and compact");
        foreach (var variant in Enum.GetValues<ButtonVariant>())
        {
            buttons.Controls.Add(UiFactory.Button(variant.ToString(), variant, IconKind.Plus));
            buttons.Controls.Add(UiFactory.Button("Compact", variant, IconKind.Check, ButtonSize.Compact));
            buttons.Controls.Add(new AppButton("Disabled", variant) { Enabled = false });
        }
        var legacy = new Button { Text = "Legacy button", Size = new(Metrics.FormWidth / 2, Metrics.ControlHeight) };
        ButtonStyler.Attach(legacy, ButtonVariant.Secondary, IconKind.Edit); buttons.Controls.Add(legacy);
    }
    private void Fields()
    {
        var fields = Section("Native fields wrapped at runtime");
        fields.Controls.Add(UiFactory.Field(new TextBox { PlaceholderText = "First and last name" }, "Patient name"));
        fields.Controls.Add(UiFactory.Search(new TextBox { PlaceholderText = "Search patients" }));
        fields.Controls.Add(UiFactory.Field(new TextBox { Text = "Demo only" }, "Password", FieldKind.Password));
        var choices = new ComboBox(); choices.Items.AddRange(["All dentists", "Demo dentist"]); choices.SelectedIndex = 0;
        fields.Controls.Add(UiFactory.Field(choices, "Dentist", FieldKind.Choice));
        fields.Controls.Add(UiFactory.Field(new DateTimePicker { Format = DateTimePickerFormat.Short }, "Date", FieldKind.Date));
        var error = UiFactory.Field(new TextBox(), "Error state"); error.SetError("This field is required."); fields.Controls.Add(error);
        fields.Controls.Add(UiFactory.Field(new TextBox { Multiline = true, MaxLength = Service.FieldLimits.Notes }, "Notes"));
    }
    private void Components()
    {
        var components = Section("Cards, badges, avatars, toggle and empty state");
        foreach (var status in AppointmentStatus.All) components.Controls.Add(UiFactory.Status(status));
        foreach (var name in new[] { "Ana Santos", "Miguel", "" }) components.Controls.Add(new Avatar(name));
        components.Controls.Add(UiFactory.Toggle());
        foreach (var level in Enum.GetValues<ElevationLevel>())
        {
            var card = UiFactory.Card(level.ToString(), level); card.Size = new(Metrics.FormWidth, Metrics.EmptyHeight + Space.Xxxl); components.Controls.Add(card);
        }
        var empty = UiFactory.Card(); empty.Size = new(Metrics.FormWidth, Metrics.EmptyHeight + Space.Xxl); empty.Content.Controls.Add(new EmptyState()); components.Controls.Add(empty);
        var header = new PageHeader("Patients", "Page header", UiFactory.Button("New patient", icon: IconKind.Plus)) { Dock = DockStyle.None, Width = Metrics.FormWidth * 2 }; components.Controls.Add(header);
        var alert = UiFactory.Alert(); alert.Dock = DockStyle.None; alert.Width = Metrics.FormWidth; alert.ShowMessage("A friendly error message appears here."); components.Controls.Add(alert);
    }
    private void IconsSection()
    {
        var icons = Section("Vector icons");
        foreach (var icon in Enum.GetValues<IconKind>())
        {
            var button = UiFactory.Button(icon.ToString(), ButtonVariant.Secondary, icon); Tooltips.Attach(button, icon.ToString()); icons.Controls.Add(button);
        }
    }
    private sealed record Sample(string Patient, string Status, DateTime AppointmentDateTime, decimal Cost);
    private void Grids()
    {
        var grids = Section("Sample grid · statuses · dates · currency · identity column");
        var grid = new DataGridView { Size = new(Metrics.FormWidth * 2, Metrics.EmptyHeight), ReadOnly = true, AllowUserToAddRows = false };
        GridHelper.Bind(grid, AppointmentStatus.All.Select((status, index) => new Sample($"Demo patient {index + 1}", status, DateTime.Today.AddHours(9 + index), 1250m)), row => row.Patient, "No sample records");
        GridHelper.IdentityColumn<Sample>(grid, nameof(Sample.Patient), row => (row.Patient, "Patient identity")); grids.Controls.Add(grid);
        var empty = new DataGridView { Size = new(Metrics.FormWidth, Metrics.EmptyHeight), AllowUserToAddRows = false };
        GridHelper.Bind(empty, Array.Empty<Sample>()); grids.Controls.Add(empty);
    }
    private void MotionSection()
    {
        var motions = Section("Motion · replay and inspect"); motions.Controls.Add(_activity);
        var slow = UiFactory.Toggle("Slow motion ×4"); slow.CheckedChanged += (_, _) => MotionSystem.TimeScale = slow.Checked ? 4 : 1; motions.Controls.Add(slow);
        var reduce = UiFactory.Toggle("Reduce motion"); reduce.CheckedChanged += (_, _) => { if (reduce.Checked) MotionSystem.Enabled = false; else MotionSystem.UseSystemPreference(); }; motions.Controls.Add(reduce);
        var kpi = new KpiCard("Appointments today", "Demo value", IconKind.Appointments); motions.Controls.Add(kpi);
        var count = UiFactory.Button("Replay count-up"); count.Click += (_, _) => { kpi.SetValue(0, animate: false); kpi.SetValue(28); }; motions.Controls.Add(count);
        var toast = UiFactory.Button("Show toast"); toast.Click += (_, _) => { using var scope = UiMessages.UseOwner(this); UiMessages.ShowSuccess("Patient saved. This is a style guide example."); }; motions.Controls.Add(toast);
        var confirm = UiFactory.Button("Confirm dialog"); confirm.Click += (_, _) => { using var scope = UiMessages.UseOwner(this); UiMessages.Confirm("This is a harmless preview. Continue?"); }; motions.Controls.Add(confirm);
        var reason = UiFactory.Button("Reason dialog"); reason.Click += (_, _) => { using var dialog = UiFactory.Reason(); dialog.ShowDialog(this); }; motions.Controls.Add(reason);
        var busy = UiFactory.Button("Busy state"); busy.Click += (_, _) => { busy.IsBusy = true; MotionSystem.Animator.Schedule(busy, "busy-demo", MotionSystem.ToastLifetime, () => busy.IsBusy = false); }; motions.Controls.Add(busy);
        var skeleton = new Skeleton { Visible = false }; motions.Controls.Add(skeleton);
        var shimmer = UiFactory.Button("Toggle skeleton"); shimmer.Click += (_, _) => skeleton.Visible = !skeleton.Visible; motions.Controls.Add(shimmer);
        var shake = UiFactory.Button("Shake feedback"); shake.Click += (_, _) => MotionSystem.Shake(shake); motions.Controls.Add(shake);
        var loading = UiFactory.Button("Loading overlay"); loading.Click += (_, _) =>
        {
            var overlay = new LoadingOverlay(kpi); kpi.Controls.Add(overlay); overlay.BringToFront();
            MotionSystem.Animator.Schedule(overlay, "overlay-demo", MotionSystem.ToastLifetime, overlay.Dispose);
        }; motions.Controls.Add(loading);
    }
    private void OnActivity(object? sender, EventArgs e) => _activity.Text = $"Active animations: {MotionSystem.Animator.ActiveCount}";
    protected override void Dispose(bool disposing)
    {
        if (disposing) { MotionSystem.Animator.ActivityChanged -= OnActivity; MotionSystem.TimeScale = 1; MotionSystem.UseSystemPreference(); }
        base.Dispose(disposing);
    }
}
#endif
