using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public partial class ucAppointmentScheduler : UserControl
{
    private readonly User _currentUser;
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly IDentistService _dentistService;
    private readonly CrudPageLayout _layout;
    private readonly TextBox _notes = new() { Multiline = true, MaxLength = FieldLimits.Notes };
    private readonly ComboBox _statusFilter = new();
    private readonly ComboBox _dateFilter = new();
    private readonly AppButton _details = UiFactory.Button("View details", ButtonVariant.Ghost, IconKind.Info);
    private readonly AppButton _clear = UiFactory.Button("Clear", ButtonVariant.Ghost);
    private IReadOnlyList<Appointment> _appointments = [];
    private IReadOnlyList<Dentist> _dentists = [];
    private Dictionary<int, string> _patientNamesById = [];
    private Dictionary<int, string> _dentistNamesById = [];
    private bool _lookupsLoaded;
    private bool _updatingAvailability;
    private int _availabilityVersion;
    private sealed record AppointmentRow(int AppointmentId, string Patient, string Dentist, DateTime AppointmentDateTime, string? Reason, string Status, Appointment Record);
    public ucAppointmentScheduler(IAppointmentService appointmentService, IPatientService patientService, IDentistService dentistService, User currentUser)
    {
        InitializeComponent(); _currentUser = currentUser; _appointmentService = appointmentService; _patientService = patientService; _dentistService = dentistService;
        cboStatus.Dispose(); btnUpdateStatus.Dispose(); lblStatus.Visible = false;
        _layout = new(this, "Appointments", "appointment", "Scheduling and appointment status", dgvAppointments, btnSchedule, _clear, ClearForm);
        _layout.AddRow(UiFactory.Field(cboPatient, "Patient", FieldKind.Choice)); _layout.AddRow(UiFactory.Field(cboDentist, "Dentist", FieldKind.Choice));
        _layout.AddRow(UiFactory.Field(dtpAppointmentDateTime, "Date and time", FieldKind.Date));
        _layout.AddRow(UiFactory.Field(cmbReason, "Reason", FieldKind.Choice)); _layout.AddRow(UiFactory.Field(_notes, "Notes (optional)"));
        cmbReason.DropDownStyle = ComboBoxStyle.DropDown; cmbReason.MaxLength = FieldLimits.Reason;
        cmbReason.Items.Clear(); cmbReason.Items.AddRange(["Oral Prophylaxis Package", "Consultation & Check up", "Promo Bundles", "Tooth Extraction", "Filling / Restoration", "Root Canal", "Braces Adjustment", "Dentures", "Teeth Whitening", "Other"]);
        dtpAppointmentDateTime.Format = DateTimePickerFormat.Custom; dtpAppointmentDateTime.CustomFormat = DisplayFormat.DateTimePattern;
        _statusFilter.Items.AddRange(["All statuses", .. AppointmentStatus.All]); _statusFilter.SelectedIndex = 0;
        _dateFilter.Items.AddRange(["All dates", "Today", "This week"]); _dateFilter.SelectedIndex = 0;
        var status = UiFactory.Field(_statusFilter, "Status", FieldKind.Choice); status.Width = Metrics.FormWidth / 2;
        var dates = UiFactory.Field(_dateFilter, "Date range", FieldKind.Choice); dates.Width = Metrics.FormWidth / 2;
        _layout.Toolbar.Controls.Add(status); _layout.Toolbar.Controls.Add(dates); _layout.Toolbar.Controls.Add(_details);
        _layout.Search.TextChanged += (_, _) => BindRows(); _statusFilter.SelectedIndexChanged += (_, _) => BindRows(); _dateFilter.SelectedIndexChanged += (_, _) => BindRows();
        _clear.Click += (_, _) => ClearForm();
        dgvAppointments.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await UiAction.RunAsync(this, ShowDetailsAsync); };
        _details.Click += async (_, _) => await UiAction.RunAsync(this, ShowDetailsAsync);
        dgvAppointments.SelectionChanged += (_, _) => _details.Enabled = dgvAppointments.SelectedRows.Count > 0;
        dtpAppointmentDateTime.ValueChanged += AvailabilityChanged; cboDentist.SelectedIndexChanged += AvailabilityChanged;
        _layout.FormCard.Visible = _layout.NewButton.Visible = RoleAccess.Can(currentUser, Permission.ManageAppointments);
        Enabled = RoleAccess.Can(currentUser, Permission.ViewAppointments);
        ClearForm();
    }
    private async void ucAppointmentScheduler_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, async () => { using var loading = _layout.Loading(); await LoadAsync(); });
    private async Task LoadAsync()
    {
        if (RoleAccess.Can(_currentUser, Permission.ManageAppointments))
        {
            var patients = UiMessages.Items(await _patientService.GetAllPatientsAsync(_currentUser));
            if (IsDisposed) return;
            _patientNamesById = patients.ToDictionary(p => p.PatientId, p => p.FullName);
            cboPatient.DataSource = patients.Where(p => p.IsActive).Select(p => new DisplayOption(p.PatientId, p.FullName)).ToList();
            cboPatient.DisplayMember = nameof(DisplayOption.Display); cboPatient.ValueMember = nameof(DisplayOption.Id);
            _dentists = UiMessages.Items(await _dentistService.GetAllDentistsAsync(_currentUser));
            if (IsDisposed) return;
            _dentistNamesById = _dentists.ToDictionary(d => d.DentistId, d => d.FullName);
            cboDentist.DisplayMember = nameof(DisplayOption.Display); cboDentist.ValueMember = nameof(DisplayOption.Id);
            cboDentist.DataSource = _dentists.Where(d => d.IsActive).Select(d => new DisplayOption(d.DentistId, d.FullName)).ToList();
            _lookupsLoaded = true; await RefreshAvailabilityAsync();
        }
        await RefreshGridAsync();
    }
    private async Task RefreshGridAsync()
    {
        _appointments = UiMessages.Items(await _appointmentService.GetAllAppointmentsAsync(_currentUser));
        foreach (var a in _appointments.Where(a => RoleAccess.CanAccessAppointment(_currentUser, a) && (!_patientNamesById.ContainsKey(a.PatientId) || !_dentistNamesById.ContainsKey(a.DentistId))))
        {
            var result = await _appointmentService.GetDetailsAsync(_currentUser, a.AppointmentId);
            if (IsDisposed) return;
            if (result.Data is { } details) { _patientNamesById[a.PatientId] = details.Patient.FullName; _dentistNamesById[a.DentistId] = details.Dentist.FullName; }
        }
        if (!IsDisposed) BindRows();
    }
    private void BindRows()
    {
        if (IsDisposed) return;
        var filtered = AppointmentFilter.Apply(_appointments, _currentUser, _layout.Search.Text, _statusFilter.SelectedIndex > 0 ? _statusFilter.Text : null,
            (AppointmentDateFilter)Math.Max(0, _dateFilter.SelectedIndex), DateTime.Today, id => _patientNamesById.GetValueOrDefault(id, $"Patient #{id}"));
        var rows = filtered.Select(a => new AppointmentRow(a.AppointmentId, _patientNamesById.GetValueOrDefault(a.PatientId, $"Patient #{a.PatientId}"),
            _dentistNamesById.GetValueOrDefault(a.DentistId, $"Dentist #{a.DentistId}"), a.AppointmentDateTime, a.Reason, a.Status, a));
        GridHelper.Bind(dgvAppointments, rows, row => row.AppointmentId, RoleAccess.IsDentist(_currentUser) && _currentUser.DentistId is null ? "Your account has no linked dentist. Ask an Admin to link it." : "No appointments match these filters.", "AppointmentId", "Record");
        GridHelper.IdentityColumn<AppointmentRow>(dgvAppointments, "Patient", row => (row.Patient, row.Reason ?? "Appointment"));
        _details.Enabled = false;
    }
    private async void AvailabilityChanged(object? sender, EventArgs e)
    {
        if (_lookupsLoaded && !_updatingAvailability) await UiAction.RunAsync(this, RefreshAvailabilityAsync);
    }
    private async Task RefreshAvailabilityAsync()
    {
        var version = ++_availabilityVersion; var when = dtpAppointmentDateTime.Value; var selected = cboDentist.SelectedValue as int?;
        var options = await Task.WhenAll(_dentists.Where(d => d.IsActive).Select(async d =>
        {
            var available = await _appointmentService.IsDentistAvailableAsync(d.DentistId, when);
            return new DisplayOption(d.DentistId, d.FullName + (available ? "" : " (busy)"), !available);
        }));
        if (IsDisposed || version != _availabilityVersion) return;
        _updatingAvailability = true;
        try { cboDentist.DataSource = options; if (selected is int id) cboDentist.SelectedValue = id; }
        finally { _updatingAvailability = false; }
        if (cboDentist.SelectedItem is DisplayOption { Busy: true }) _layout.Alert.ShowMessage("This dentist already has an appointment at that time. Choose another time or dentist.", Semantic.Warning);
        else _layout.Alert.Dismiss();
    }
    private async void btnSchedule_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, ScheduleAsync, btnSchedule);
    private async Task ScheduleAsync()
    {
        if (cboPatient.SelectedValue is not int patientId || cboDentist.SelectedValue is not int dentistId) { UiMessages.ShowError(ServiceResult.Fail("Pick a patient and a dentist first.")); return; }
        var appointment = new Appointment { PatientId = patientId, DentistId = dentistId, AppointmentDateTime = dtpAppointmentDateTime.Value, Reason = InputRules.NullIfBlank(cmbReason.Text), Notes = InputRules.NullIfBlank(_notes.Text) };
        var result = await _appointmentService.ScheduleAppointmentAsync(_currentUser, appointment);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await RefreshGridAsync(); ClearForm(); UiMessages.ShowSuccess("Appointment scheduled.");
        var saved = _appointments.Where(a => a.PatientId == patientId && a.DentistId == dentistId && a.AppointmentDateTime == appointment.AppointmentDateTime).MaxBy(a => a.AppointmentId);
        if (saved is not null) GridHelper.FlashRow(dgvAppointments, saved.AppointmentId);
        await RefreshAvailabilityAsync();
    }
    private async Task ShowDetailsAsync()
    {
        if (dgvAppointments.CurrentRow?.DataBoundItem is not AppointmentRow row || dgvAppointments.SelectedRows.Count == 0) return;
        var result = await _appointmentService.GetDetailsAsync(_currentUser, row.AppointmentId);
        if (!result.Success || result.Data is null) { UiMessages.ShowError(result); return; }
        if (IsDisposed) return;
        using var dialog = new frmAppointmentDetails(result.Data, _appointmentService, _currentUser, async id => { await RefreshGridAsync(); GridHelper.FlashRow(dgvAppointments, id); });
        dialog.ShowDialog(FindForm());
    }
    private void ClearForm() { cmbReason.SelectedIndex = -1; cmbReason.Text = ""; _notes.Clear(); dgvAppointments.ClearSelection(); _layout?.SetEditing(false); if (_layout is not null) _layout.NewButton.Text = "Schedule"; btnSchedule.Text = "Schedule appointment"; }
    private void btnUpdateStatus_Click(object? sender, EventArgs e) { } // Disposed legacy Designer control.
}
