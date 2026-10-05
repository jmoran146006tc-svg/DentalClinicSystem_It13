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
    private readonly AppButton _export = UiFactory.Button("Export CSV", ButtonVariant.Secondary, IconKind.Download);
    private readonly TableLayoutPanel _cards = new() { Name = "reportCards", Dock = DockStyle.Top, AutoSize = true, BackColor = Palette.Canvas, Margin = Padding.Empty };
    private readonly DonutChart _status = new() { Name = "reportStatus", Dock = DockStyle.Fill, Margin = Padding.Empty };
    private readonly LineChart _daily = new() { Name = "reportDaily", Dock = DockStyle.Fill };
    private readonly BarChart _typesChart = new() { Name = "reportTypesChart", Dock = DockStyle.Fill };
    private readonly ClinicTable _types = new() { Name = "reportTypes", Dock = DockStyle.Fill };
    private readonly ClinicTable _workload = new() { Name = "reportWorkload", Dock = DockStyle.Fill };
    private readonly KpiCard _billed = new("Billed in range", "Net billed, not collected");
    private readonly KpiCard _appointments = new("Appointments in range", "All statuses", IconKind.Appointments);
    private readonly KpiCard _completion = new("Completion rate", "Completed appointments", IconKind.Check);
    private readonly KpiCard _treatments = new("Treatments performed", "Treatments in range", IconKind.Treatments);
    private readonly List<ReportSection> _sections = [];
    private readonly List<Skeleton> _skeletons = [];
    private readonly KpiStrip _kpis;
    private readonly ReportToolbar _toolbar;
    private readonly ReportTypesContent _typesContent;
    private bool _settingRange, _arranging;
    private ReportSnapshot? _snapshot;
    internal ReportSnapshot? Snapshot => _snapshot;

    public ucReports(IReportService reports, User currentUser) : this(reports, currentUser, TimeProvider.System) { }
    internal ucReports(IReportService reports, User currentUser, TimeProvider time)
    {
        _reports = reports; _actor = currentUser; _time = time;
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this);
        Dock = DockStyle.Fill; AutoScroll = true; Padding = Space.Page; BackColor = Palette.Canvas;
        var refresh = UiFactory.Button("Refresh", ButtonVariant.Secondary); refresh.Name = "reportRefresh";
        _toolbar = new(_from, _to, refresh, _export);
        _typesContent = new(_typesChart, _types);
        _kpis = new(_billed, _appointments, _completion, _treatments) { Name = "reportKpis" };
        SetRange(ReportRange.ThisMonth); BuildLayout(); _export.Enabled = false; _retry.Visible = false;
        refresh.Click += async (_, _) => await RefreshPageAsync();
        _toolbar.Ranges.SelectIndexChanged += async (_, _) =>
        {
            if (_settingRange || _toolbar.Ranges.SelectIndex == 3) return;
            SetRange((ReportRange)_toolbar.Ranges.SelectIndex); await RefreshPageAsync();
        };
        _from.ValueChanged += (_, _) => SelectCustom(); _to.ValueChanged += (_, _) => SelectCustom();
        _retry.Name = "reportRetry"; _export.Name = "reportExport";
        UiMessages.RegisterAlertHost(this, _alert);
        Load += async (_, _) => await RefreshPageAsync();
        _retry.Click += async (_, _) => await RefreshPageAsync();
        _export.Click += async (_, _) => await UiAction.RunAsync(this, ExportAsync, _export);
        _cards.SizeChanged += (_, _) => ResizeCards(); DpiChangedAfterParent += (_, _) => ResizeCards(); ResizeCards();
    }
    private void BuildLayout()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.Controls.Add(new PageHeader("Reports", "Billed amounts are not money collected"));
        layout.Controls.Add(_toolbar); layout.Controls.Add(_alert); layout.Controls.Add(_retry); layout.Controls.Add(_kpis); layout.Controls.Add(_cards); Controls.Add(layout);
        AddSection("Appointments by status", StatusContent(), "No appointments in this range", IconKind.Appointments);
        AddSection("Billed by day", _daily, "No billed treatments in this range", IconKind.Reports);
        AddSection("Top treatment types", _typesContent, "No treatments in this range", IconKind.Treatments);
        AddSection("Dentist workload", _workload, "No dentist activity in this range", IconKind.Dentist);
        foreach (var card in new[] { _billed, _appointments, _completion, _treatments })
        {
            var skeleton = new Skeleton { Dock = DockStyle.Fill }; card.Content.Controls.Add(skeleton);
            skeleton.BringToFront(); _skeletons.Add(skeleton);
        }
    }
    private Control StatusContent()
    {
        var content = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Palette.Surface };
        content.ColumnStyles.Add(new(SizeType.Percent, 40)); content.ColumnStyles.Add(new(SizeType.Percent, 60));
        content.RowStyles.Add(new(SizeType.Percent, 100));
        var legend = new ReportLegend(); _status.ValuesChanged += legend.SetValues;
        content.Controls.Add(_status, 0, 0); content.Controls.Add(legend, 1, 0); return content;
    }
    private void AddSection(string title, Control content, string emptyText, IconKind icon)
    {
        var card = new ReportSection(title, content, emptyText, icon) { Name = "reportSection" + _sections.Count };
        _sections.Add(card); _cards.Controls.Add(card);
    }
    private void ResizeCards()
    {
        if (_arranging || _sections.Count != 4) return;
        _arranging = true;
        try
        {
            var columns = ClientSize.Width >= Metrics.Scale(this, Metrics.FormWidth * 3) ? 2 : 1;
            var rows = Math.Clamp(_snapshot?.Workload.Count ?? 3, 3, 8);
            var workloadHeight = Space.Lg * 2 + Metrics.ControlHeight + Metrics.GridHeaderHeight + rows * Metrics.NavHeight;
            ResponsiveCards.Arrange(_cards, _sections.Cast<Control>().ToArray(), columns, Metrics.ReportCardHeight, spanFrom: 2, workloadHeight);
        }
        finally { _arranging = false; }
    }
    private void SetRange(ReportRange range)
    {
        _settingRange = true;
        try
        {
            var dates = ReportsPresentation.Range(range, _time.GetLocalNow().DateTime); _from.Value = dates.From; _to.Value = dates.To;
            _toolbar.Ranges.SelectIndex = (int)range;
        }
        finally { _settingRange = false; }
    }
    private void SelectCustom() { if (!_settingRange) _toolbar.Ranges.SelectIndex = 3; }
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
                if (_snapshot is null || !RoleAccess.Can(_actor, Permission.ViewReports)) { _cards.Visible = _kpis.Visible = false; _export.Enabled = false; }
                if (!_alert.Visible) _alert.ShowMessage("Reports could not be loaded. Try again.");
            }
        }
    }
    private void Bind(ReportSnapshot snapshot)
    {
        using var before = _snapshot is null ? ContentReveal.Snapshot(_cards) : null;
        _snapshot = snapshot; _cards.Visible = _kpis.Visible = true; SuspendLayout();
        try
        {
            HideSkeletons();
            _status.SetData(ReportsPresentation.StatusChart(snapshot.Status), value => value.ToString("N0"), "No appointments in this range");
            _daily.SetData(ReportsPresentation.DailyChart(snapshot.Days), ChartCurrency, "No billed treatments in this range");
            _typesContent.SetData(snapshot.Types);
            BindKpis(snapshot); BindTables(snapshot);
            _sections[0].SetEmpty(snapshot.Status.Sum(row => row.Total) == 0); _sections[1].SetEmpty(snapshot.Days.Count == 0);
            _sections[2].SetEmpty(snapshot.Types.Count == 0); _sections[3].SetEmpty(snapshot.Workload.Count == 0); ResizeCards();
            _export.Enabled = true;
        }
        finally { ResumeLayout(true); }
        ContentReveal.Play(_cards, before is null ? null : (Bitmap)before.Clone());
    }
    private void BindKpis(ReportSnapshot snapshot)
    {
        var total = snapshot.Status.Sum(row => row.Total);
        var completed = snapshot.Status.Where(row => row.Status == AppointmentStatus.Completed).Sum(row => row.Total);
        _billed.SetValue((double)snapshot.Days.Sum(row => row.Billed), ChartCurrency); _appointments.SetValue(total);
        _completion.SetValue(total == 0 ? 0 : (double)completed / total, value => total == 0 ? "n/a" : value.ToString("P0"));
        // The existing query returns at most ten types, so completeness is unknown at its limit.
        var complete = snapshot.Types.Count < 10;
        _treatments.SetValue(snapshot.Types.Sum(row => row.Total), value => complete ? value.ToString("N0") : "n/a");
        _treatments.SetSubtitle(complete ? "Treatments in range" : "Total unavailable from top list");
    }
    private void BindTables(ReportSnapshot snapshot)
    {
        GridHelper.Bind(_types, snapshot.Types.Select(row => new TreatmentRow(row.Name, row.Total, row.Billed)), key: null, emptyMessage: "No treatments in this range");
        GridHelper.Bind(_workload, snapshot.Workload.Select(row => new WorkloadRow(row.Dentist, row.Total, row.Completed,
            row.Total == 0 ? "n/a" : ((double)row.Completed / row.Total).ToString("P0"), row.Billed)), key: null, emptyMessage: "No dentist activity in this range");
        _workload.Columns.First(column => column.Key == nameof(WorkloadRow.Completion)).Title = "Completion %";
    }
    private sealed record TreatmentRow(string Treatment, int Count, decimal Billed);
    private sealed record WorkloadRow(string Dentist, int Appointments, int Completed, string Completion, decimal Billed);
    private static string ChartCurrency(double value) => value >= (double)decimal.MaxValue ? value.ToString("N2") : DisplayFormat.Currency((decimal)value);
    private void HideSkeletons()
    {
        foreach (var skeleton in _skeletons) skeleton.Visible = false;
        foreach (var section in _sections) section.StopLoading();
    }
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
