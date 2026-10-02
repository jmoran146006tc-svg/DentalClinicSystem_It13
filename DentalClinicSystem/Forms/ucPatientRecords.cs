using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class ucPatientRecords : UserControl
    {
        private readonly IPatientService _patientService;
        private readonly User _currentUser;
        private int? _selectedPatientId;

        public ucPatientRecords(IPatientService patientService, User currentUser)
        {
            InitializeComponent();
            _patientService = patientService;
            _currentUser = currentUser;
            btnDelete.Dispose();
        }

        private async void ucPatientRecords_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, ucPatientRecords_LoadAsync);

        private async Task ucPatientRecords_LoadAsync()
        {
            dtpDateOfBirth.MaxDate = DateTime.Today;

            dgvPatients.SelectionChanged += dgvPatients_SelectionChanged;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnClear.Click += (_, _) => ClearForm();

            btnAdd.Visible = RoleAccess.Can(_currentUser, Permission.ManagePatients);
            btnUpdate.Visible = RoleAccess.Can(_currentUser, Permission.ManagePatients);

            btnUpdate.Enabled = false;

            await RefreshGridAsync();
        }

        private async Task RefreshGridAsync()
        {
            var patients = UiMessages.Items(await _patientService.GetAllPatientsAsync(_currentUser));
            GridHelper.Bind(dgvPatients, patients.ToList());

            if (dgvPatients.Columns["IsActive"] is { } isActiveCol) isActiveCol.Visible = false;
            if (dgvPatients.Columns["CreatedAt"] is { } createdAtCol) createdAtCol.Visible = false;
        }

        private void dgvPatients_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvPatients.CurrentRow?.DataBoundItem is not Patient patient)
                return;

            _selectedPatientId = patient.PatientId;
            txtFirstName.Text = patient.FirstName;
            txtLastName.Text = patient.LastName;
            txtEmail.Text = patient.Email;
            txtContactNumber.Text = patient.ContactNumber;
            txtAddress.Text = patient.Address;
            dtpDateOfBirth.Value = patient.DateOfBirth;

            btnUpdate.Enabled = RoleAccess.Can(_currentUser, Permission.ManagePatients);
        }

        private async void btnAdd_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, btnAdd_ClickAsync);

        private async Task btnAdd_ClickAsync()
        {
            var patient = new Patient
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                DateOfBirth = dtpDateOfBirth.Value.Date,
                ContactNumber = txtContactNumber.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim()
            };

            var result = await _patientService.AddPatientAsync(_currentUser, patient);
            if (!result.Success)
            {
                UiMessages.ShowError(result);
                return;
            }

            await RefreshGridAsync();
            ClearForm();
        }

        private async void btnUpdate_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, btnUpdate_ClickAsync);

        private async Task btnUpdate_ClickAsync()
        {
            if (_selectedPatientId is null)
            {
                MessageBox.Show("Select a patient from the grid first.", "No Patient Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var patient = new Patient
            {
                PatientId = _selectedPatientId.Value,
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                DateOfBirth = dtpDateOfBirth.Value.Date,
                ContactNumber = txtContactNumber.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim()
            };

            var result = await _patientService.UpdatePatientAsync(_currentUser, patient);
            if (!result.Success)
            {
                UiMessages.ShowError(result);
                return;
            }

            await RefreshGridAsync();
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedPatientId = null;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtContactNumber.Clear();
            txtAddress.Clear();
            dtpDateOfBirth.Value = DateTime.Today;
            dgvPatients.ClearSelection();
            btnUpdate.Enabled = false;
        }
    }
}