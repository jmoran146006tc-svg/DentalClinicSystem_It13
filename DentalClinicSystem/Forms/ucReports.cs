using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class ucReports : UserControl
{
    private readonly IReportService _reports;
    private readonly User _actor;
    private readonly TimeProvider _time;
    private readonly DateTimePicker _from = new() { Name = "reportFrom", Format = DateTimePickerFormat.Short };
    private readonly DateTimePicker _to = new() { Name = "reportTo", Format = DateTimePickerFormat.Short };
    private readonly InlineAlert _alert = new() { Name = "reportAlert", Visible = false };
    private readonly AppButton _retry = UiFactory.Button("Retry", ButtonVariant.Secondary);
    private readonly AppButton _export = UiFactory.Button("Export CSV", ButtonVariant.Secondary);
    private readonly TableLayoutPanel _cards = new() { Name = "reportCards", Dock = DockStyle.Top, AutoSize = true, BackColor = Palette.Canvas };
    private readonly DonutChart _status = new() { Name = "reportStatus", Dock = DockStyle.Top, Height = Metrics.ChartHeight };
    private readonly LineChart _daily = new() { Name = "reportDaily", Dock = DockStyle.Fill };
    private readonly BarChart _typesChart = new() { Name = "reportTypesChart", Dock = DockStyle.Top, Height = Metrics.ChartHeight };
    private readonly ClinicTable _days = new() { Name = "reportDays", Dock = DockStyle.Fill };
    private readonly ClinicTable _types = new() { Name = "reportTypes", Dock = DockStyle.Fill };
    private readonly ClinicTable _workload = new() { Name = "reportWorkload", Dock = DockStyle.Fill };
    private readonly KpiCard _billed = new("Billed for this range", "Net after discounts · not money collected");
    private readonly List<RoundedPanel> _sections = [];
    private readonly List<Label> _counts = [];
    private readonly List<Skeleton> _skeletons = [];
    private ReportSnapshot? _snapshot;
    internal ReportSnapshot? Snapshot => _snapshot;

    public ucReports(IReportService reports, User currentUser) : this(reports, currentUser, TimeProvider.System) { }
    internal ucReports(IReportService reports, User currentUser, TimeProvider time)
    {
        _reports = reports; _actor = currentUser; _time = time;
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this);
        Dock = DockStyle.Fill; AutoScroll = true; Padding = Space.Page; BackColor = Palette.Canvas;
        SetRange(ReportRange.ThisMonth); BuildLayout(); _export.Enabled = false; _retry.Visible = false;
        _retry.Name = "reportRetry"; _export.Name = "reportExport";
        UiMessages.RegisterAlertHost(this, _alert);
        Load += async (_, _) => await RefreshPageAsync();
        _retry.Click += async (_, _) => await RefreshPageAsync();
        _export.Click += async (_, _) => await UiAction.RunAsync(this, ExportAsync, _export);
        _cards.SizeChanged += (_, _) => ResizeCards(); DpiChangedAfterParent += (_, _) => ResizeCards(); ResizeCards();
    }
    private void BuildLayout()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas };
        layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.Controls.Add(new PageHeader("Reports", "Billed amounts are not money collected"));
        layout.Controls.Add(Toolbar()); layout.Controls.Add(_alert); layout.Controls.Add(_retry); layout.Controls.Add(_cards); Controls.Add(layout);
        AddSection("Appointments by status", StatusContent()); AddSection("Billed by day", DailyContent());
        var types = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Surface }; types.Controls.Add(_types); types.Controls.Add(_typesChart);
        AddSection("Top treatment types", types); AddSection("Dentist workload", _workload);
    }
    private Control Toolbar()
    {
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, BackColor = Palette.Canvas, WrapContents = true };
        toolbar.Controls.Add(UiFactory.Field(_from, "From", FieldKind.Date)); toolbar.Controls.Add(UiFactory.Field(_to, "To", FieldKind.Date));
        foreach (var (range, text) in new[] { (ReportRange.ThisWeek, "This week"), (ReportRange.ThisMonth, "This month"), (ReportRange.LastThirtyDays, "Last 30 days") })
        {
            var button = UiFactory.Button(text, ButtonVariant.Ghost);
            button.Click += async (_, _) => { SetRange(range); await RefreshPageAsync(); }; toolbar.Controls.Add(button);
        }
        var refresh = UiFactory.Button("Refresh", ButtonVariant.Secondary); refresh.Name = "reportRefresh";
        refresh.Click += async (_, _) => await RefreshPageAsync(); toolbar.Controls.Add(refresh); toolbar.Controls.Add(_export); return toolbar;
    }
    private Control StatusContent()
    {
        var content = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Surface };
        var legend = new TableLayoutPanel { Name = "reportLegend", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = AppointmentStatus.All.Length, BackColor = Palette.Surface };
        legend.ColumnStyles.Add(new(SizeType.Percent, 65)); legend.ColumnStyles.Add(new(SizeType.Percent, 35));
        foreach (var status in AppointmentStatus.All)
        {
            var index = _counts.Count;
            var count = new Label { Name = "reportCount" + status, Text = "0", Dock = DockStyle.Fill, Font = Typography.Body, TextAlign = ContentAlignment.MiddleRight, AccessibleName = AppointmentStatus.Display(status) + " count" };
            _counts.Add(count); legend.RowStyles.Add(new(SizeType.Absolute, Metrics.ControlHeight)); legend.Controls.Add(UiFactory.Status(status), 0, index); legend.Controls.Add(count, 1, index);
        }
        _status.ValuesChanged += values => { for (var i = 0; i < values.Count; i++) _counts[i].Text = values[i].ToString("N0"); };
        content.Controls.Add(legend); content.Controls.Add(_status); return content;
    }
    private Control DailyContent()
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Palette.Surface };
        content.ColumnStyles.Add(new(SizeType.Percent, 100));
        content.RowStyles.Add(new(SizeType.Absolute, Metrics.KpiHeight)); content.RowStyles.Add(new(SizeType.Absolute, Metrics.ChartHeight)); content.RowStyles.Add(new(SizeType.Percent, 100));
        _billed.Dock = DockStyle.Fill; content.Controls.Add(_billed, 0, 0); content.Controls.Add(_daily, 0, 1); content.Controls.Add(_days, 0, 2); return content;
    }
    private void AddSection(string title, Control content)
    {
        var card = UiFactory.Card(); card.Name = "reportSection" + _sections.Count; card.Dock = DockStyle.Fill;
        var heading = new Label { Text = title, Dock = DockStyle.Top, Height = Metrics.ControlHeight, Font = Typography.Heading, ForeColor = Palette.Ink900 };
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Surface }; body.Controls.Add(content);
        var skeleton = new Skeleton { Dock = DockStyle.Fill }; body.Controls.Add(skeleton); skeleton.BringToFront(); _skeletons.Add(skeleton);
        card.Content.Controls.Add(body); card.Content.Controls.Add(heading); _sections.Add(card); _cards.Controls.Add(card);
    }
    private void ResizeCards() => ResponsiveCards.Arrange(_cards, _sections.Cast<Control>().ToArray(),
        ClientSize.Width >= Metrics.Scale(this, Metrics.FormWidth * 3) ? 2 : 1, Metrics.ReportCardHeight);
    private void SetRange(ReportRange range)
    {
        var dates = ReportsPresentation.Range(range, _time.GetLocalNow().DateTime); _from.Value = dates.From; _to.Value = dates.To;
    }
    internal Task RefreshPageAsync() => UiAction.RunAsync(this, RefreshAsync);
    private async Task RefreshAsync()
    {
        var success = false; _alert.Dismiss(); _retry.Visible = false;
        try
        {
            var from = _from.Value.Date; var to = _to.Value.Date;
            var validation = Validator.DateRange(from, to);
            if (!validation.Success) { UiMessages.ShowError(validation); return; }
            var status = await _reports.GetAppointmentStatusCountsAsync(_actor, from, to);
            if (IsDisposed) return;
            if (!status.Success) { UiMessages.ShowError(status); return; }
            var daysTask = _reports.GetRevenueByDayAsync(_actor, from, to);
            var typesTask = _reports.GetTopTreatmentTypesAsync(_actor, from, to);
            var workloadTask = _reports.GetDentistWorkloadAsync(_actor, from, to);
            await Task.WhenAll(daysTask, typesTask, workloadTask);
            if (IsDisposed) return;
            var days = await daysTask; if (IsDisposed) return;
            var types = await typesTask; if (IsDisposed) return;
            var workload = await workloadTask; if (IsDisposed) return;
            var failure = new ServiceResult[] { days, types, workload }.FirstOrDefault(result => !result.Success);
            if (failure is not null) { UiMessages.ShowError(failure); return; }
            Bind(new(from, to, ReportsPresentation.Statuses(status.Data ?? []), days.Data ?? [], types.Data ?? [], workload.Data ?? [])); success = true;
        }
        finally
        {
            if (!IsDisposed && !success)
            {
                HideSkeletons(); _retry.Visible = true;
                if (_snapshot is null || !RoleAccess.Can(_actor, Permission.ViewReports)) { _cards.Visible = false; _export.Enabled = false; }
                if (!_alert.Visible) _alert.ShowMessage("Reports could not be loaded. Try again.");
            }
        }
    }
    private void Bind(ReportSnapshot snapshot)
    {
        using var before = _snapshot is null ? ContentReveal.Snapshot(_cards) : null;
        _snapshot = snapshot; _cards.Visible = true; SuspendLayout();
        try
        {
            HideSkeletons();
            _status.SetData(ReportsPresentation.StatusChart(snapshot.Status), value => value.ToString("N0"), "No appointments in this range");
            _daily.SetData(ReportsPresentation.DailyChart(snapshot.Days), ChartCurrency, "No billed treatments in this range");
            _typesChart.SetData(ReportsPresentation.TypeChart(snapshot.Types), value => value.ToString("N0"), "No treatments in this range");
            _billed.SetValue((double)snapshot.Days.Sum(row => row.Billed), ChartCurrency);
            GridHelper.Bind(_days, snapshot.Days, key: null, emptyMessage: "No billed treatments in this range");
            GridHelper.Bind(_types, snapshot.Types, key: null, emptyMessage: "No treatments in this range"); GridHelper.Bind(_workload, snapshot.Workload, key: null, emptyMessage: "No dentist activity in this range");
            _export.Enabled = true;
        }
        finally { ResumeLayout(true); }
        ContentReveal.Play(_cards, before is null ? null : (Bitmap)before.Clone());
    }
    private static string ChartCurrency(double value) => value >= (double)decimal.MaxValue ? value.ToString("N2") : DisplayFormat.Currency((decimal)value);
    private void HideSkeletons() { foreach (var skeleton in _skeletons) skeleton.Visible = false; }
    private async Task ExportAsync()
    {
        if (_snapshot is not { } snapshot) return;
        using var dialog = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv", DefaultExt = "csv", AddExtension = true,
            FileName = $"dental-reports-{snapshot.From:yyyy-MM-dd}_to_{snapshot.To:yyyy-MM-dd}.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            await File.WriteAllBytesAsync(dialog.FileName, ReportsPresentation.Csv(snapshot));
            if (IsDisposed) return;
            UiMessages.ShowSuccess("Reports exported.");
        }
        catch (IOException) { if (!IsDisposed) UiMessages.ShowError(ServiceResult.Fail("The CSV could not be saved. Close any open copy and try again.")); }
        catch (UnauthorizedAccessException) { if (!IsDisposed) UiMessages.ShowError(ServiceResult.Fail("Choose a folder where you have permission to save the CSV.")); }
    }
}
