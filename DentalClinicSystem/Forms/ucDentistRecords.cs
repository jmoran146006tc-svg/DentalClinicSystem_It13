using System.ComponentModel;
using DentalClinicSystem.Models;
using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucDentistRecords : UserControl
    {
        private readonly IDentistService _dentistService;
        private int? _selectedDentistId;

        public ucDentistRecords(IDentistService dentistService)
        {
            InitializeComponent();
            _dentistService = dentistService;
        }

        private async void ucDentistRecords_Load(object sender, EventArgs e)
        {
            dgvDentists.SelectionChanged += dgvDentists_SelectionChanged;
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
            var dentists = await _dentistService.GetAllDentistsAsync();
            dgvDentists.DataSource = new BindingList<Dentist>(dentists.ToList());

            if (dgvDentists.Columns["IsActive"] is { } isActiveCol) isActiveCol.Visible = false;
        }

        private void dgvDentists_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDentists.CurrentRow?.DataBoundItem is not Dentist dentist)
                return;

            _selectedDentistId = dentist.DentistId;
            txtFirstName.Text = dentist.FirstName;
            txtLastName.Text = dentist.LastName;
            txtSpecialization.Text = dentist.Specialization;
            txtContactNumber.Text = dentist.ContactNumber;
            txtLicenseNumber.Text = dentist.LicenseNumber;

            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var dentist = new Dentist
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Specialization = string.IsNullOrWhiteSpace(txtSpecialization.Text) ? null : txtSpecialization.Text.Trim(),
                ContactNumber = string.IsNullOrWhiteSpace(txtContactNumber.Text) ? null : txtContactNumber.Text.Trim(),
                LicenseNumber = string.IsNullOrWhiteSpace(txtLicenseNumber.Text) ? null : txtLicenseNumber.Text.Trim()
            };

            var result = await _dentistService.AddDentistAsync(dentist);
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
            if (_selectedDentistId is null)
            {
                MessageBox.Show("Select a dentist from the grid first.", "No Dentist Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dentist = new Dentist
            {
                DentistId = _selectedDentistId.Value,
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Specialization = string.IsNullOrWhiteSpace(txtSpecialization.Text) ? null : txtSpecialization.Text.Trim(),
                ContactNumber = string.IsNullOrWhiteSpace(txtContactNumber.Text) ? null : txtContactNumber.Text.Trim(),
                LicenseNumber = string.IsNullOrWhiteSpace(txtLicenseNumber.Text) ? null : txtLicenseNumber.Text.Trim()
            };

            var result = await _dentistService.UpdateDentistAsync(dentist);
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
            if (_selectedDentistId is null)
            {
                MessageBox.Show("Select a dentist from the grid first.", "No Dentist Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                "This deactivates the dentist - their appointment history is kept. Continue?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            await _dentistService.DeleteDentistAsync(_selectedDentistId.Value);
            await RefreshGridAsync();
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedDentistId = null;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtSpecialization.Clear();
            txtContactNumber.Clear();
            txtLicenseNumber.Clear();
            dgvDentists.ClearSelection();
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }
    }
}