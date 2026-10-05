using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public partial class ucDentistRecords : UserControl
{
    private readonly User _currentUser;
    private readonly IDentistService _dentistService;
    private readonly CrudPageLayout _layout;
    private readonly AppButton _timeOff = UiFactory.Button("Time off", ButtonVariant.Ghost);
    private IReadOnlyList<Dentist> _dentists = [];
    private Dentist? _selected;
    private Dentist? _savedDentist;
    private sealed record DentistRow(int DentistId, string Name, string? Specialization, string? ContactNumber, string? LicenseNumber, Dentist Record);
    public ucDentistRecords(IDentistService dentistService, User currentUser)
    {
        InitializeComponent(); _currentUser = currentUser; _dentistService = dentistService;
        _layout = new(this, "Dentists", "dentist", "Clinic practitioners and contact details", dgvDentists, btnAdd, btnClear, ClearForm);
        _layout.AddRow(UiFactory.Field(txtFirstName, "First name"), UiFactory.Field(txtLastName, "Last name"));
        _layout.AddRow(UiFactory.Field(cboSpecialization, "Specialization", FieldKind.Choice));
        cboSpecialization.DropDownStyle = ComboBoxStyle.DropDown; cboSpecialization.MaxLength = FieldLimits.Specialization;
        cboSpecialization.Items.AddRange(DentalSpecializations.All);
        _layout.AddRow(UiFactory.Field(txtContactNumber, "Contact"), UiFactory.Field(txtLicenseNumber, "License"));
        InputRules.ApplyMaxLengths((txtFirstName, FieldLimits.Name), (txtLastName, FieldLimits.Name), (txtContactNumber, FieldLimits.ContactNumber), (txtLicenseNumber, FieldLimits.LicenseNumber));
        txtContactNumber.VerifyChar += InputRules.PhoneVerifyChar;
        btnDelete.Text = "Deactivate"; btnDelete.Visible = true; btnDelete.Height = Metrics.ControlHeight; ButtonStyler.Attach(btnDelete, ButtonVariant.Danger);
        _layout.Actions.Controls.Add(btnDelete);
        _layout.Toolbar.Controls.Add(_timeOff);
        _timeOff.Click += (_, _) => { if (_selected is { } dentist) { using var dialog = new frmDentistTimeOff(dentist, _dentistService, _currentUser); dialog.ShowDialog(FindForm()); } };
        dgvDentists.SelectionChanged += SelectionChanged; _layout.Search.TextChanged += (_, _) => BindRows();
        btnClear.Click += (_, _) => ClearForm();
        btnDelete.Click += async (_, _) => await UiAction.RunAsync(this, DeactivateAsync, btnDelete);
        Enabled = RoleAccess.Can(currentUser, Permission.ManageDentists); ClearForm();
        _layout.UseModal(SaveAsync, AfterSaveAsync);
    }
    private async void ucDentistRecords_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, async () => { using var loading = _layout.Loading(); await RefreshGridAsync(); });
    private async Task RefreshGridAsync() { _dentists = UiMessages.Items(await _dentistService.GetAllDentistsAsync(_currentUser)); if (!IsDisposed) BindRows(); }
    private void BindRows()
    {
        var rows = _dentists.Where(d => new[] { d.FullName, d.Specialization ?? "", d.ContactNumber ?? "", d.LicenseNumber ?? "" }.Any(v => v.Contains(_layout.Search.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            .Select(d => new DentistRow(d.DentistId, d.FullName, d.Specialization, d.ContactNumber, d.LicenseNumber, d));
        GridHelper.Bind(dgvDentists, rows, row => row.DentistId, "No dentists match your search.", "DentistId", "Record");
        GridHelper.IdentityColumn<DentistRow>(dgvDentists, "Name", row => (row.Name, row.Specialization ?? "Dentist")); ClearForm();
    }
    private void SelectionChanged(object? sender, EventArgs e)
    {
        _timeOff.Enabled = dgvDentists.SelectedRecord is not null;
        if (dgvDentists.SelectedRecord is null || dgvDentists.SelectedRecord is not DentistRow row) return;
        _selected = row.Record; txtFirstName.Text = _selected.FirstName ?? ""; txtLastName.Text = _selected.LastName ?? ""; cboSpecialization.Text = _selected.Specialization ?? "";
        txtContactNumber.Text = _selected.ContactNumber ?? ""; txtLicenseNumber.Text = _selected.LicenseNumber ?? ""; _layout.SetEditing(true); btnDelete.Enabled = true; _timeOff.Enabled = true;
    }
    private async Task<bool> SaveAsync()
    {
        var dentist = new Dentist { DentistId = _selected?.DentistId ?? 0, FirstName = txtFirstName.Text.Trim(), LastName = txtLastName.Text.Trim(),
            Specialization = InputRules.NullIfBlank(cboSpecialization.Text), ContactNumber = InputRules.NullIfBlank(txtContactNumber.Text), LicenseNumber = InputRules.NullIfBlank(txtLicenseNumber.Text) };
        var result = _selected is null ? await _dentistService.AddDentistAsync(_currentUser, dentist) : await _dentistService.UpdateDentistAsync(_currentUser, dentist);
        if (!result.Success) { UiMessages.ShowError(result); return false; }
        _savedDentist = dentist; return true;
    }
    private async Task AfterSaveAsync()
    {
        var dentist = _savedDentist!;
        await RefreshGridAsync(); UiMessages.ShowSuccess("Dentist saved.");
        var saved = _dentists.Where(d => d.FullName == dentist.FullName && d.LicenseNumber == dentist.LicenseNumber).MaxBy(d => d.DentistId);
        if (saved is not null) GridTheme.SelectAndFlash(dgvDentists, dentist.DentistId > 0 ? dentist.DentistId : saved.DentistId);
    }
    private async Task DeactivateAsync()
    {
        if (_selected is not { } dentist || !UiMessages.Confirm("Deactivate this dentist? Their appointment history will be kept.", "Deactivate dentist")) return;
        var result = await _dentistService.DeleteDentistAsync(_currentUser, dentist.DentistId);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await RefreshGridAsync(); ClearForm(); UiMessages.ShowSuccess("Dentist deactivated.");
    }
    private void ClearForm() { _selected = null; txtFirstName.Clear(); txtLastName.Clear(); cboSpecialization.SelectedIndex = -1; cboSpecialization.Text = ""; txtContactNumber.Clear(); txtLicenseNumber.Clear(); dgvDentists.ClearSelection(); btnDelete.Enabled = false; _timeOff.Enabled = false; _layout?.SetEditing(false); _layout?.Alert.Dismiss(); }
}
