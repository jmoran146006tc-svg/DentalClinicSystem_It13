using System.ComponentModel;
using DentalClinicSystem.Models;
using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucPatientRecords : UserControl
    {
        private readonly IPatientService _patientService;
        private int? _selectedPatientId;

        public ucPatientRecords(IPatientService patientService)
        {
            InitializeComponent();
            _patientService = patientService;
        }

        private async void ucPatientRecords_Load(object sender, EventArgs e)
        {
            dtpDateOfBirth.MaxDate = DateTime.Today;

            dgvPatients.SelectionChanged += dgvPatients_SelectionChanged;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += (_, _) => ClearForm();

            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;

            await RefreshGridAsync();
        }

        private async Task RefreshGridAsync()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            dgvPatients.DataSource = new BindingList<Patient>(patients.ToList());

            if (dgvPatients.Columns["IsActive"] is { } isActiveCol) isActiveCol.Visible = false;
            if (dgvPatients.Columns["CreatedAt"] is { } createdAtCol) createdAtCol.Visible = false;
        }

        private void dgvPatients_SelectionChanged(object sender, EventArgs e)
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

            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
        }

        private async void btnAdd_Click(object sender, EventArgs e)
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

            var result = await _patientService.AddPatientAsync(patient);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshGridAsync();
            ClearForm();
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
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

            var result = await _patientService.UpdatePatientAsync(patient);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshGridAsync();
            ClearForm();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedPatientId is null)
            {
                MessageBox.Show("Select a patient from the grid first.", "No Patient Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                "This deactivates the patient - their appointment and treatment history is kept. Continue?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            await _patientService.DeletePatientAsync(_selectedPatientId.Value);
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
            btnDelete.Enabled = false;
        }
    }
}