using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Forms;

public sealed class ucDashboardHome : BufferedPage
{
    private readonly IAppointmentService _appointments;
    private readonly IPatientService _patients;
    private readonly IDentistService _dentists;
    private readonly IReportService _reports;
    private readonly IPatientHistoryService _history;
    private readonly User _actor;
    private readonly TimeProvider _time;
    private readonly DateTime _today;
    private readonly PageHeader _header;
    private readonly TableLayoutPanel _body = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas, Margin = Padding.Empty };
    private readonly InlineAlert _alert = new() { Visible = false };
    private readonly AppButton _retry = UiFactory.Button("Retry", ButtonVariant.Secondary);
    private readonly List<KpiCard> _cards = [];
    private readonly Dictionary<int, string> _patientNames = [], _dentistNames = [];
    private ucWeekCalendar? _calendar;
    private IReadOnlyList<Patient> _patientRows = [];
    private bool _loaded;

    public ucDashboardHome(IAppointmentService appointments, IPatientService patients, IDentistService dentists,
        IReportService reports, IPatientHistoryService history, User actor, TimeProvider? time = null)
    {
        _appointments = appointments; _patients = patients; _dentists = dentists; _reports = reports; _history = history;
        _actor = actor; _time = time ?? TimeProvider.System; var now = _time.GetLocalNow().DateTime; _today = now.Date;
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this);
        Name = "dashboardHome"; Dock = DockStyle.Fill; AutoScroll = true; BackColor = Palette.Canvas; Padding = Space.Page;
        var refresh = UiFactory.Button("Refresh", ButtonVariant.Secondary);
        refresh.Click += async (_, _) => await LoadDashboardAsync();
        _header = new PageHeader(RoleAccess.IsDentist(actor) ? $"Your patients today, Dr. {actor.Username}" : DashboardPresentation.Greeting(actor.Username, now),
            DashboardPresentation.TodayLabel(_today), refresh);
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new(SizeType.Percent, 100)); _body.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.Controls.Add(_header); layout.Controls.Add(_alert); layout.Controls.Add(_retry); layout.Controls.Add(_body); Controls.Add(layout);
        _retry.Visible = false; _retry.Click += async (_, _) => await LoadDashboardAsync(); UiMessages.RegisterAlertHost(this, _alert);
        AddSkeletons();
        Load += async (_, _) => { FadeGreeting(); await LoadDashboardAsync(); };
    }
    private void AddSkeletons()
    {
        if (!RoleAccess.IsDentist(_actor))
        {
            var placeholders = Enumerable.Range(0, RoleAccess.Can(_actor, Permission.ViewReports) ? 4 : 3).Select(_ => new KpiCard("")).ToArray();
            foreach (var card in placeholders) { var skeleton = new Skeleton { Dock = DockStyle.Fill }; card.Content.Controls.Add(skeleton); skeleton.BringToFront(); }
            _body.Controls.Add(new KpiStrip(placeholders));
        }
        _body.Controls.Add(new Skeleton { Dock = DockStyle.Top, Height = Metrics.CalendarViewportHeight + Space.Lg * 2, Margin = Padding.Empty });
    }
    private void FadeGreeting()
    {
        foreach (var label in _header.Controls.Cast<Control>().SelectMany(c => c.Controls.Cast<Control>()).OfType<Label>())
        {
            var destination = label.ForeColor;
            MotionSystem.Animator.RunColor(label, "greeting", Palette.Canvas, destination, MotionSystem.Fast, color => label.ForeColor = color);
        }
    }
    private Task LoadDashboardAsync() => UiAction.RunAsync(this, async () =>
    {
        var success = false; _alert.Visible = false; _retry.Visible = false;
        try
        {
            if (!RoleAccess.Can(_actor, Permission.ViewDashboard)) { UiMessages.ShowError(RoleAccess.Denied()); return; }
            success = RoleAccess.IsDentist(_actor) ? await LoadDentistAsync() : await LoadStaffAsync();
        }
        finally
        {
            if (!IsDisposed && !success)
            {
                if (!_loaded) ClearBody();
                _alert.ShowMessage("The dashboard could not be loaded. Try again."); _retry.Visible = true;
            }
        }
    });
    private async Task<bool> LoadStaffAsync()
    {
        var week = DashboardPresentation.WeekStart(_today);
        var patients = await _patients.GetAllIncludingInactiveAsync(_actor);
        if (IsDisposed) return false;
        if (!patients.Success || patients.Data is not { } patientRows) { UiMessages.ShowError(patients); return false; }
        var dentists = await _dentists.GetAllDentistsAsync(_actor);
        if (IsDisposed) return false;
        if (!dentists.Success || dentists.Data is not { } dentistRows) { UiMessages.ShowError(dentists); return false; }
        var appointments = await _appointments.GetAppointmentsInRangeAsync(_actor, week, week.AddDays(DashboardPresentation.DaysInWeek));
        if (IsDisposed) return false;
        if (!appointments.Success || appointments.Data is not { } rows) { UiMessages.ShowError(appointments); return false; }
        decimal billed = 0;
        if (RoleAccess.Can(_actor, Permission.ViewReports))
        {
            var revenue = await _reports.GetRevenueByDayAsync(_actor, week, week.AddDays(6));
            if (IsDisposed) return false;
            if (!revenue.Success || revenue.Data is null) { UiMessages.ShowError(revenue); return false; }
            billed = revenue.Data.Sum(r => r.Billed);
        }
        _patientRows = patientRows; _patientNames.Clear(); _dentistNames.Clear();
        foreach (var patient in patientRows) _patientNames[patient.PatientId] = patient.FullName;
        foreach (var dentist in dentistRows) _dentistNames[dentist.DentistId] = dentist.FullName;
        if (!await AddHistoricalDentistsAsync(rows)) return false;
        if (!_loaded) BuildStaff();
        var counts = DashboardPresentation.Counts(rows, _patientRows, _today);
        _cards[0].SetValue(counts.TodayAppointments); _cards[1].SetValue(counts.ActivePatients); _cards[2].SetValue(counts.WeekCancellations);
        if (_cards.Count > 3) _cards[3].SetValue((double)billed, value => DisplayFormat.Currency((decimal)value));
        var selectedWeek = _loaded ? _calendar?.WeekStart ?? week : week;
        if (selectedWeek == week) _calendar?.SetAppointments(week, _today, rows, _patientNames, _dentistNames);
        else if (!await LoadWeekAsync(selectedWeek, 0)) return false;
        _loaded = true; return true;
    }
    private void BuildStaff()
    {
        ClearBody(); _cards.Clear();
        _cards.Add(new KpiCard("Today's appointments", "All statuses", IconKind.Appointments));
        _cards.Add(new KpiCard("Active patients", "Current patient records", IconKind.Patients));
        _cards.Add(new KpiCard("Cancellations this week", "Appointments dated Monday–Sunday", IconKind.Close));
        if (RoleAccess.Can(_actor, Permission.ViewReports)) _cards.Add(new KpiCard("Billed this week", "Net billed · not collected", IconKind.Reports));
        _body.Controls.Add(new KpiStrip(_cards.ToArray()) { Name = "dashboardKpis" });
        var calendarCard = UiFactory.Card(); calendarCard.Name = "dashboardCalendar"; calendarCard.Dock = DockStyle.Top;
        _calendar = new ucWeekCalendar { Dock = DockStyle.Fill };
        calendarCard.Margin = Padding.Empty; calendarCard.Height = _calendar.Height + calendarCard.Padding.Vertical;
        _calendar.WeekRequested += async (week, direction) => await UiAction.RunAsync(this, async () => { await LoadWeekAsync(week, direction); });
        _calendar.AppointmentActivated += async id => await UiAction.RunAsync(this, () => OpenDetailsAsync(id));
        calendarCard.Content.Controls.Add(_calendar); _body.Controls.Add(calendarCard);
    }
    private async Task<bool> AddHistoricalDentistsAsync(IReadOnlyList<Appointment> rows)
    {
        foreach (var id in rows.Select(a => a.DentistId).Distinct().Where(id => !_dentistNames.ContainsKey(id)))
        {
            var result = await _dentists.GetDentistByIdAsync(_actor, id);
            if (IsDisposed) return false;
            if (!result.Success || result.Data is null) { UiMessages.ShowError(result); return false; }
            _dentistNames[id] = result.Data.FullName;
        }
        return true;
    }
    private async Task<bool> LoadWeekAsync(DateTime week, int direction)
    {
        var result = await _appointments.GetAppointmentsInRangeAsync(_actor, week, week.AddDays(DashboardPresentation.DaysInWeek));
        if (IsDisposed) return false;
        if (!result.Success || result.Data is null) { UiMessages.ShowError(result); return false; }
        if (!await AddHistoricalDentistsAsync(result.Data)) return false;
        _calendar?.SetAppointments(week, _today, result.Data, _patientNames, _dentistNames, direction); return true;
    }
    private async Task OpenDetailsAsync(int id)
    {
        var result = await _appointments.GetDetailsAsync(_actor, id);
        if (IsDisposed) return;
        if (!result.Success || result.Data is null) { UiMessages.ShowError(result); return; }
        using var dialog = new frmAppointmentDetails(result.Data, _appointments, _actor,
            async _ => { await LoadStaffAsync(); }, _dentists);
        dialog.ShowDialog(this);
    }
    private async Task<bool> LoadDentistAsync()
    {
        if (_actor.DentistId is not int dentistId)
        {
            ClearBody(); _body.Controls.Add(new EmptyState("Your account needs a dentist link", "Ask an administrator to link your account to a dentist record.", IconKind.Dentist) { Dock = DockStyle.Top });
            _loaded = true; return true;
        }
        var dentist = await _dentists.GetDentistByIdAsync(_actor, dentistId);
        if (IsDisposed) return false;
        if (!dentist.Success || dentist.Data is null) { UiMessages.ShowError(dentist); return false; }
        var result = await _appointments.GetAppointmentsForDentistOnDateAsync(_actor, dentistId, _today);
        if (IsDisposed) return false;
        if (!result.Success || result.Data is null) { UiMessages.ShowError(result); return false; }
        List<AppointmentDetails> details = [];
        foreach (var a in result.Data.Where(a => a.DentistId == dentistId && a.AppointmentDateTime.Date == _today))
        {
            var detail = await _appointments.GetDetailsAsync(_actor, a.AppointmentId);
            if (IsDisposed) return false;
            if (!detail.Success || detail.Data is null) { UiMessages.ShowError(detail); return false; }
            details.Add(detail.Data);
        }
        ClearBody(); _header.SetTitle($"Your patients today, {dentist.Data.FullName}");
        var worklist = new DentistWorklist(details, _actor, _time.GetLocalNow().DateTime);
        worklist.HistoryRequested += async id => await UiAction.RunAsync(this, () => OpenHistoryAsync(id));
        worklist.CompletionRequested += async id => await UiAction.RunAsync(this, () => CompleteAsync(id));
        _body.Controls.Add(worklist); _loaded = true; return true;
    }
    private Task OpenHistoryAsync(int id)
    {
        using var dialog = new DialogShell("Patient history", size: new(Metrics.FormWidth * 2, Metrics.MinimumHeight));
        dialog.ConfirmButton.Visible = false; dialog.AcceptButton = null; dialog.DismissButton.Text = "Close";
        dialog.Body.Controls.Add(PageFactory.CreatePatientHistory(_history, _actor, id, _time.GetLocalNow().DateTime));
        dialog.ShowDialog(this); return Task.CompletedTask;
    }
    private async Task CompleteAsync(int id)
    {
        var result = await _appointments.UpdateAppointmentStatusAsync(_actor, id, AppointmentStatus.Completed);
        if (IsDisposed) return;
        if (!result.Success) { UiMessages.ShowError(result); return; }
        UiMessages.ShowSuccess("Appointment completed."); await LoadDentistAsync();
    }
    private void ClearBody()
    {
        foreach (var control in _body.Controls.Cast<Control>().ToArray()) control.Dispose();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) MotionSystem.Animator.Cancel(this);
        base.Dispose(disposing);
    }
}
