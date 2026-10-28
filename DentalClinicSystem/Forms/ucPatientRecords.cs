using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public partial class ucPatientRecords : UserControl
{
    private readonly IPatientService _patientService;
    private readonly User _currentUser;
    private readonly CrudPageLayout _layout;
    private readonly FormField _guardianName, _guardianContact;
    private readonly FormField _firstNameField, _lastNameField, _contactField, _emailField, _dobField;
    private readonly HashSet<FormField> _fieldErrors = [];
    private readonly AppButton _reactivate = UiFactory.Button("Reactivate", ButtonVariant.Secondary);
    private IReadOnlyList<Patient> _patients = [];
    private Patient? _selected;
    private Patient? _savedPatient;
    private sealed record PatientRow(int PatientId, string FullName, string ContactNumber, string? Email, DateTime DateOfBirth, string? Address, Patient Record);
    public ucPatientRecords(IPatientService patientService, User currentUser)
    {
        InitializeComponent(); _patientService = patientService; _currentUser = currentUser; btnDelete.Dispose();
        _layout = new(this, "Patients", "patient", "Contact details and patient records", dgvPatients, btnAdd, btnClear, ClearForm);
        txtAddress.Multiline = true;
        _firstNameField = UiFactory.Field(txtFirstName, "First name"); _lastNameField = UiFactory.Field(txtLastName, "Last name");
        _contactField = UiFactory.Field(txtContactNumber, "Contact"); _emailField = UiFactory.Field(txtEmail, "Email");
        _dobField = UiFactory.Field(dtpDateOfBirth, "Date of birth", FieldKind.Date);
        _layout.AddRow(_firstNameField, _lastNameField);
        _layout.AddRow(_contactField, _emailField);
        _layout.AddRow(_dobField);
        _layout.AddRow(UiFactory.Field(txtAddress, "Address"));
        _guardianName = UiFactory.Field(txtGuardianName, "Guardian name"); _guardianContact = UiFactory.Field(txtGuardianContact, "Guardian contact");
        _layout.AddRow(_guardianName, _guardianContact);
        _layout.AddRow(UiFactory.Field(txtAllergies, "Allergies")); _layout.AddRow(UiFactory.Field(txtMedicalNotes, "Medical notes"));
        InputRules.ApplyMaxLengths((txtGuardianName, FieldLimits.GuardianName), (txtGuardianContact, FieldLimits.GuardianContact), (txtAllergies, FieldLimits.Allergies), (txtMedicalNotes, FieldLimits.MedicalNotes));
        txtGuardianContact.KeyPress += InputRules.PhoneKeyPress;
        dtpDateOfBirth.ValueChanged += (_, _) => UpdateGuardianHelpers(); UpdateGuardianHelpers();
        dtpDateOfBirth.MaxDate = DateTime.Today; dtpDateOfBirth.Format = DateTimePickerFormat.Custom; dtpDateOfBirth.CustomFormat = DisplayFormat.DatePattern;
        InputRules.ApplyMaxLengths((txtFirstName, FieldLimits.Name), (txtLastName, FieldLimits.Name), (txtContactNumber, FieldLimits.ContactNumber), (txtEmail, FieldLimits.Email), (txtAddress, FieldLimits.Address));
        txtContactNumber.KeyPress += InputRules.PhoneKeyPress;
        WireValidation();
        _layout.Toolbar.Controls.Add(_layout.ShowInactive);
        _layout.ShowInactive.Visible = RoleAccess.Can(currentUser, Permission.ManagePatients);
        _layout.ShowInactive.CheckedChanged += async (_, _) => await UiAction.RunAsync(this, RefreshGridAsync);
        _layout.Search.TextChanged += (_, _) => BindRows();
        _reactivate.Visible = false; _layout.Actions.Controls.Add(_reactivate);
        _reactivate.Click += async (_, _) => await UiAction.RunAsync(this, ReactivateAsync, _reactivate);
        dgvPatients.SelectionChanged += SelectionChanged;
        GridTheme.MuteInactive<PatientRow>(dgvPatients, row => !row.Record.IsActive);
        btnClear.Click += (_, _) => ClearForm();
        _layout.NewButton.Visible = RoleAccess.Can(currentUser, Permission.ManagePatients);
        _layout.UseModal(SaveAsync, AfterSaveAsync);
        if (!RoleAccess.Can(currentUser, Permission.ViewPatients)) Enabled = false;
    }
    private async void ucPatientRecords_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, async () => { using var loading = _layout.Loading(); await RefreshGridAsync(); });
    private async Task RefreshGridAsync()
    {
        var result = _layout.ShowInactive.Checked ? await _patientService.GetAllIncludingInactiveAsync(_currentUser) : await _patientService.GetAllPatientsAsync(_currentUser);
        _patients = UiMessages.Items(result); if (!IsDisposed) BindRows();
    }
    private void BindRows()
    {
        var rows = PatientFilter.Apply(_patients, _layout.Search.Text, _layout.ShowInactive.Checked)
            .Select(p => new PatientRow(p.PatientId, p.FullName, p.ContactNumber, p.Email, p.DateOfBirth, p.Address, p));
        GridHelper.Bind(dgvPatients, rows, row => row.PatientId, "No patients match your search.", "PatientId", "Record");
        GridHelper.IdentityColumn<PatientRow>(dgvPatients, "FullName", row => (row.FullName, row.Record.IsActive ? "Active patient" : "Inactive patient"));
        ClearForm();
    }
    private void SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvPatients.SelectedRows.Count == 0 || dgvPatients.CurrentRow?.DataBoundItem is not PatientRow row) return;
        _selected = row.Record; txtFirstName.Text = _selected.FirstName; txtLastName.Text = _selected.LastName;
        txtContactNumber.Text = _selected.ContactNumber; txtEmail.Text = _selected.Email; txtAddress.Text = _selected.Address;
        txtGuardianName.Text = _selected.GuardianName; txtGuardianContact.Text = _selected.GuardianContact; txtAllergies.Text = _selected.Allergies; txtMedicalNotes.Text = _selected.MedicalNotes;
        InputRules.SetDate(dtpDateOfBirth, _selected.DateOfBirth); ClearFieldErrors(); _layout.SetEditing(true); _reactivate.Visible = !_selected.IsActive;
    }
    private async Task<bool> SaveAsync()
    {
        if (!ValidateAll()) return false;
        var patient = new Patient
        {
            PatientId = _selected?.PatientId ?? 0,
            FirstName = txtFirstName.Text.Trim(),
            LastName = txtLastName.Text.Trim(),
            ContactNumber = txtContactNumber.Text.Trim(),
            Email = InputRules.NullIfBlank(txtEmail.Text),
            Address = InputRules.NullIfBlank(txtAddress.Text),
            DateOfBirth = dtpDateOfBirth.Value.Date,
            GuardianName = InputRules.NullIfBlank(txtGuardianName.Text),
            GuardianContact = InputRules.NullIfBlank(txtGuardianContact.Text),
            Allergies = InputRules.NullIfBlank(txtAllergies.Text),
            MedicalNotes = InputRules.NullIfBlank(txtMedicalNotes.Text)
        };
        var result = _selected is null ? await _patientService.AddPatientAsync(_currentUser, patient) : await _patientService.UpdatePatientAsync(_currentUser, patient);
        if (!result.Success) { UiMessages.ShowError(result); return false; }
        _savedPatient = patient; return true;
    }
    // ---- Inline input validation (red border + message under each field) ----
    private void WireValidation()
    {
        static void NameKeys(object? sender, KeyPressEventArgs e) =>
            e.Handled = !char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !" -'.".Contains(e.KeyChar);
        txtFirstName.KeyPress += NameKeys; txtLastName.KeyPress += NameKeys; txtGuardianName.KeyPress += NameKeys;
        txtEmail.KeyPress += (_, e) => e.Handled = e.KeyChar == ' ';
        Watch(_firstNameField, txtFirstName, () => Validator.Name(Validator.Sanitize(txtFirstName.Text), "First name"));
        Watch(_lastNameField, txtLastName, () => Validator.Name(Validator.Sanitize(txtLastName.Text), "Last name"));
        Watch(_contactField, txtContactNumber, () => Validator.Phone(Validator.Sanitize(txtContactNumber.Text)));
        Watch(_emailField, txtEmail, () => Validator.Email(InputRules.NullIfBlank(txtEmail.Text)));
        Watch(_guardianName, txtGuardianName, GuardianNameCheck);
        Watch(_guardianContact, txtGuardianContact, GuardianContactCheck);
        dtpDateOfBirth.ValueChanged += (_, _) =>
        {
            Show(_dobField, Validator.DatabaseDate(dtpDateOfBirth.Value, "Date of birth"));
            if (_fieldErrors.Contains(_guardianName)) Show(_guardianName, GuardianNameCheck());
            if (_fieldErrors.Contains(_guardianContact)) Show(_guardianContact, GuardianContactCheck());
        };
    }
    private bool IsMinor => Validator.IsMinor(dtpDateOfBirth.Value, DateTime.Today);
    private ServiceResult GuardianNameCheck()
    {
        var name = Validator.Sanitize(txtGuardianName.Text);
        if (name.Length == 0) return IsMinor ? ServiceResult.Fail("Guardian name is required for patients under 18.") : ServiceResult.Ok();
        return Validator.Name(name, "Guardian name");
    }
    private ServiceResult GuardianContactCheck()
    {
        var contact = Validator.Sanitize(txtGuardianContact.Text);
        if (contact.Length == 0) return IsMinor ? ServiceResult.Fail("Guardian contact is required for patients under 18.") : ServiceResult.Ok();
        var phone = Validator.Phone(contact);
        return phone.Success ? phone : ServiceResult.Fail("Guardian contact must be 7 to 15 digits.");
    }
    private void Watch(FormField field, Control input, Func<ServiceResult> check)
    {
        input.Leave += (_, _) => Show(field, check());
        input.TextChanged += (_, _) => { if (_fieldErrors.Contains(field)) Show(field, check()); };
    }
    private void Show(FormField field, ServiceResult result)
    {
        if (result.Success)
        {
            if (!_fieldErrors.Remove(field)) return;
            field.SetError(string.Empty);
            if (field == _guardianName || field == _guardianContact) UpdateGuardianHelpers();
        }
        else { _fieldErrors.Add(field); field.SetError(result.ErrorMessage); }
    }
    private bool ValidateAll()
    {
        var checks = new (FormField Field, Control Input, ServiceResult Result)[]
        {
            (_firstNameField, txtFirstName, Validator.Name(Validator.Sanitize(txtFirstName.Text), "First name")),
            (_lastNameField, txtLastName, Validator.Name(Validator.Sanitize(txtLastName.Text), "Last name")),
            (_contactField, txtContactNumber, Validator.Phone(Validator.Sanitize(txtContactNumber.Text))),
            (_emailField, txtEmail, Validator.Email(InputRules.NullIfBlank(txtEmail.Text))),
            (_dobField, dtpDateOfBirth, Validator.DatabaseDate(dtpDateOfBirth.Value, "Date of birth")),
            (_guardianName, txtGuardianName, GuardianNameCheck()),
            (_guardianContact, txtGuardianContact, GuardianContactCheck()),
        };
        foreach (var (field, _, result) in checks) Show(field, result);
        var firstInvalid = checks.FirstOrDefault(c => !c.Result.Success);
        if (firstInvalid.Input is null) return true;
        firstInvalid.Input.Focus();
        return false;
    }
    private void ClearFieldErrors()
    {
        foreach (var field in _fieldErrors.ToArray()) field.SetError(string.Empty);
        _fieldErrors.Clear();
    }

    private async Task AfterSaveAsync()
    {
        var patient = _savedPatient!;
        await RefreshGridAsync(); UiMessages.ShowSuccess("Patient saved.");
        var saved = _patients.Where(p => p.FullName == patient.FullName && p.ContactNumber == patient.ContactNumber).MaxBy(p => p.PatientId);
        if (saved is not null) GridTheme.SelectAndFlash(dgvPatients, patient.PatientId > 0 ? patient.PatientId : saved.PatientId);
    }
    private async Task ReactivateAsync()
    {
        if (_selected is not { IsActive: false } patient) return;
        var result = await _patientService.ReactivatePatientAsync(_currentUser, patient.PatientId);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await RefreshGridAsync(); ClearForm(); UiMessages.ShowSuccess("Patient reactivated."); GridHelper.FlashRow(dgvPatients, patient.PatientId);
    }
    private void UpdateGuardianHelpers()
    {
        var helper = Validator.IsMinor(dtpDateOfBirth.Value, DateTime.Today) ? "Required for patients under 18" : "Optional for patients 18 or older";
        _guardianName.SetHelper(helper); _guardianContact.SetHelper(helper);
    }
    private void ClearForm()
    {
        _selected = null; txtFirstName.Clear(); txtLastName.Clear(); txtContactNumber.Clear(); txtEmail.Clear(); txtAddress.Clear();
        txtGuardianName.Clear(); txtGuardianContact.Clear(); txtAllergies.Clear(); txtMedicalNotes.Clear();
        dtpDateOfBirth.Value = DateTime.Today; ClearFieldErrors(); UpdateGuardianHelpers(); dgvPatients.ClearSelection(); _reactivate.Visible = false; _layout?.SetEditing(false); _layout?.Alert.Dismiss();
    }
}