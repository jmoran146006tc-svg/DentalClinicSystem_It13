using System.Globalization;
using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public partial class ucTreatmentRecords : UserControl
{
    private readonly ITreatmentService _treatmentService;
    private readonly IAppointmentService _appointmentService;
    private readonly ITreatmentTypeService _treatmentTypeService;
    private readonly User _currentUser;
    private readonly CrudPageLayout _layout;
    private readonly AppButton _clear = UiFactory.Button("Clear", ButtonVariant.Ghost);
    private Dictionary<int, TreatmentType> _treatmentTypesById = [];
    private Dictionary<int, Appointment> _appointmentsById = [];
    private Dictionary<int, string> _patientNamesByAppointmentId = [];
    private IReadOnlyList<Treatment> _treatments = [];
    private Treatment? _selected;
    private Treatment? _savedTreatment;
    private sealed record TreatmentRow(int TreatmentId, string Patient, DateTime? AppointmentDateTime, string TreatmentType, string? ToothNumber, decimal Cost, DateTime DatePerformed, string? Notes, Treatment Record);

    public ucTreatmentRecords(ITreatmentService treatmentService, IAppointmentService appointmentService, ITreatmentTypeService treatmentTypeService, User currentUser)
    {
        InitializeComponent(); _treatmentService = treatmentService; _appointmentService = appointmentService; _treatmentTypeService = treatmentTypeService; _currentUser = currentUser;
        _layout = new(this, "Treatments", "treatment", "Clinical records, procedures and costs", dgvTreatments, btnAddTreatment, _clear, ClearForm);
        _layout.AddRow(UiFactory.Field(cboAppointment, "Appointment", FieldKind.Choice)); _layout.AddRow(UiFactory.Field(cboTreatmentType, "Treatment type", FieldKind.Choice));
        var tooth = UiFactory.Field(txtToothNumber, "Tooth #"); tooth.SetHelper("FDI number");
        _layout.AddRow(tooth, UiFactory.Field(txtCost, "Cost"));
        txtToothNumber.KeyPress += (_, e) => { if (!char.IsControl(e.KeyChar) && e.KeyChar is not (>= '0' and <= '9')) e.Handled = true; };
        _layout.AddRow(UiFactory.Field(dtpDatePerformed, "Date performed", FieldKind.Date));
        txtNotes.Multiline = true; _layout.AddRow(UiFactory.Field(txtNotes, "Notes"));
        InputRules.ApplyMaxLengths((txtToothNumber, FieldLimits.ToothNumber), (txtNotes, FieldLimits.Notes), (txtCost, FieldLimits.ContactNumber));
        dtpDatePerformed.Enabled = false; dtpDatePerformed.Format = DateTimePickerFormat.Custom; dtpDatePerformed.CustomFormat = DisplayFormat.DatePattern;
        cboAppointment.SelectedIndexChanged += (_, _) => SetAppointmentDate();
        cboTreatmentType.SelectedIndexChanged += cboTreatmentType_SelectedIndexChanged; dgvTreatments.SelectionChanged += SelectionChanged;
        _layout.Search.TextChanged += (_, _) => BindRows(); _clear.Click += (_, _) => ClearForm();
        Enabled = RoleAccess.Can(currentUser, Permission.ViewTreatments);
        _layout.NewButton.Visible = RoleAccess.Can(currentUser, Permission.ManageTreatments);
        _layout.UseModal(SaveAsync, AfterSaveAsync);
    }
    private async void ucTreatmentRecords_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, async () => { using var loading = _layout.Loading(); await LoadAsync(); });
    private async Task LoadAsync()
    {
        var appointments = UiMessages.Items(await _appointmentService.GetAllAppointmentsAsync(_currentUser)).Where(a => RoleAccess.CanAccessAppointment(_currentUser, a)).ToList();
        _appointmentsById = appointments.ToDictionary(a => a.AppointmentId);
        foreach (var appointment in appointments)
        {
            var result = await _appointmentService.GetDetailsAsync(_currentUser, appointment.AppointmentId);
            if (IsDisposed) return;
            _patientNamesByAppointmentId[appointment.AppointmentId] = result.Data?.Patient.FullName ?? $"Patient #{appointment.PatientId}";
        }
        cboAppointment.DisplayMember = nameof(DisplayOption.Display); cboAppointment.ValueMember = nameof(DisplayOption.Id); BindAppointmentOptions();
        var types = UiMessages.Items(await _treatmentTypeService.GetAllTreatmentTypesAsync(_currentUser));
        if (IsDisposed) return;
        _treatmentTypesById = types.ToDictionary(t => t.TreatmentTypeId);
        cboTreatmentType.DisplayMember = nameof(TreatmentType.Name); cboTreatmentType.ValueMember = nameof(TreatmentType.TreatmentTypeId); cboTreatmentType.DataSource = types.ToList();
        await RefreshGridAsync(); ClearForm();
    }
    private async Task RefreshGridAsync()
    {
        _treatments = UiMessages.Items(await _treatmentService.GetAllTreatmentsAsync(_currentUser));
        if (!IsDisposed) BindRows();
    }
    private void BindRows()
    {
        var rows = _treatments.Where(t => _appointmentsById.ContainsKey(t.AppointmentId))
            .Select(t => new TreatmentRow(t.TreatmentId, _patientNamesByAppointmentId.GetValueOrDefault(t.AppointmentId, "n/a"),
                _appointmentsById.GetValueOrDefault(t.AppointmentId)?.AppointmentDateTime, _treatmentTypesById.GetValueOrDefault(t.TreatmentTypeId)?.Name ?? "n/a", t.ToothNumber, t.Cost, t.DatePerformed, t.Notes, t))
            .Where(t => new[] { t.Patient, t.TreatmentType, t.Notes ?? "" }.Any(v => v.Contains(_layout.Search.Text.Trim(), StringComparison.OrdinalIgnoreCase)));
        GridHelper.Bind(dgvTreatments, rows, row => row.TreatmentId, "No treatments match your search.", "TreatmentId", "Record");
        GridHelper.IdentityColumn<TreatmentRow>(dgvTreatments, "Patient", row => (row.Patient, "Treatment record"));
        ClearForm();
    }
    private void SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvTreatments.SelectedRows.Count == 0 || dgvTreatments.CurrentRow?.DataBoundItem is not TreatmentRow row) return;
        _selected = row.Record; BindAppointmentOptions(); cboAppointment.SelectedValue = _selected.AppointmentId; cboTreatmentType.SelectedValue = _selected.TreatmentTypeId;
        txtCost.Text = _selected.Cost.ToString("0.00", CultureInfo.InvariantCulture); txtToothNumber.Text = _selected.ToothNumber; txtNotes.Text = _selected.Notes;
        SetAppointmentDate(); _layout.SetEditing(true);
    }
    private void BindAppointmentOptions()
    {
        cboAppointment.DataSource = _appointmentsById.Values.Where(a => a.AppointmentId == _selected?.AppointmentId ||
            a.Status is not (AppointmentStatus.Cancelled or AppointmentStatus.NoShow) && a.AppointmentDateTime.Date <= DateTime.Today)
            .Select(a => new DisplayOption(a.AppointmentId, AppointmentLabels.Format(a, _patientNamesByAppointmentId.GetValueOrDefault(a.AppointmentId, $"Patient #{a.PatientId}")))).ToList();
    }
    private void SetAppointmentDate()
    {
        if (cboAppointment.SelectedValue is int id && _appointmentsById.TryGetValue(id, out var appointment))
            InputRules.SetDate(dtpDatePerformed, appointment.AppointmentDateTime.Date);
    }
    private void cboTreatmentType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (cboTreatmentType.SelectedValue is int id && _treatmentTypesById.TryGetValue(id, out var type))
            txtCost.Text = type.DefaultCost.ToString("0.00", CultureInfo.InvariantCulture);
    }
    private async void btnAddTreatment_Click(object? sender, EventArgs e) => await _layout.OpenEditorAsync();
    private async Task<bool> SaveAsync()
    {
        if (cboAppointment.SelectedValue is not int appointmentId || cboTreatmentType.SelectedValue is not int typeId) { UiMessages.ShowError(ServiceResult.Fail("Pick an appointment and a treatment type first.")); return false; }
        if (!InputRules.TryCost(txtCost.Text, out var cost)) { UiMessages.ShowError(ServiceResult.Fail("Enter a cost between 0 and 99,999,999.99 using a decimal point, for example 1250.50.")); txtCost.Focus(); return false; }
        var treatment = new Treatment { TreatmentId = _selected?.TreatmentId ?? 0, AppointmentId = appointmentId, TreatmentTypeId = typeId,
            Cost = cost, ToothNumber = InputRules.NullIfBlank(txtToothNumber.Text), DatePerformed = dtpDatePerformed.Value.Date, Notes = InputRules.NullIfBlank(txtNotes.Text) };
        var result = _selected is null ? await _treatmentService.AddTreatmentAsync(_currentUser, treatment) : await _treatmentService.UpdateTreatmentAsync(_currentUser, treatment);
        if (!result.Success) { UiMessages.ShowError(result); return false; }
        _savedTreatment = treatment; return true;
    }
    private async Task AfterSaveAsync()
    {
        var treatment = _savedTreatment!;
        await RefreshGridAsync(); UiMessages.ShowSuccess("Treatment saved.");
        var saved = _treatments.Where(t => t.AppointmentId == treatment.AppointmentId && t.TreatmentTypeId == treatment.TreatmentTypeId && t.Cost == treatment.Cost).MaxBy(t => t.TreatmentId);
        if (saved is not null) GridTheme.SelectAndFlash(dgvTreatments, treatment.TreatmentId > 0 ? treatment.TreatmentId : saved.TreatmentId);
    }
    private void ClearForm()
    {
        _selected = null; BindAppointmentOptions(); cboAppointment.SelectedIndex = -1; cboTreatmentType.SelectedIndex = -1; txtToothNumber.Clear(); txtCost.Clear(); txtNotes.Clear();
        dtpDatePerformed.Value = DateTime.Today; dgvTreatments.ClearSelection(); _layout?.SetEditing(false); _layout?.Alert.Dismiss();
    }
}
