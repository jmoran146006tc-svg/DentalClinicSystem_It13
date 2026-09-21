using System.ComponentModel;
using DentalClinicSystem.Models;
using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucUserManagement : UserControl
    {
        private readonly IUserService _userService;
        private readonly IDentistService _dentistService;
        private readonly User _currentUser;

        private int? _selectedUserId;

        public ucUserManagement(IUserService userService, IDentistService dentistService, User currentUser)
        {
            InitializeComponent();
            _userService = userService;
            _dentistService = dentistService;
            _currentUser = currentUser;
        }

        private async void ucUserManagement_Load(object sender, EventArgs e)
        {
            // Defense in depth: frmDashboard already hides the Users button for
            // non-admins, but if this control is ever reached another way, lock
            // it down here too rather than trusting the caller.
            if (_currentUser.Role != "Admin")
            {
                foreach (Control control in Controls)
                    control.Enabled = false;

                lblAdminOnly.Text = "Only an Admin account can manage users.";
                lblAdminOnly.Visible = true;
                return;
            }

            cboRole.Items.AddRange(["Admin", "Receptionist", "Dentist"]);
            cboRole.SelectedIndexChanged += cboRole_SelectedIndexChanged;

            dgvUsers.SelectionChanged += dgvUsers_SelectionChanged;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDeactivate.Click += btnDeactivate_Click;
            btnClear.Click += (_, _) => ClearForm();

            btnUpdate.Enabled = false;
            btnDeactivate.Enabled = false;

            await LoadDentistsAsync();
            await RefreshGridAsync();
        }

        private async Task LoadDentistsAsync()
        {
            var dentists = await _dentistService.GetAllDentistsAsync();
            cboDentist.DataSource = dentists.ToList();
            cboDentist.DisplayMember = nameof(Dentist.FullName);
            cboDentist.ValueMember = nameof(Dentist.DentistId);
        }

        private async Task RefreshGridAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            dgvUsers.DataSource = new BindingList<User>(users.ToList());

            // Never display the password hash, even to an admin.
            if (dgvUsers.Columns["PasswordHash"] is { } hashCol) hashCol.Visible = false;
        }

        private void cboRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboDentist.Enabled = cboRole.SelectedItem as string == "Dentist";
        }

        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is not User user)
                return;

            _selectedUserId = user.UserId;
            txtUsername.Text = user.Username;
            txtPassword.Clear(); // never show or prefill a password/hash
            cboRole.SelectedItem = user.Role;
            cboDentist.Enabled = user.Role == "Dentist";
            if (user.DentistId is int dentistId)
                cboDentist.SelectedValue = dentistId;

            btnUpdate.Enabled = true;
            btnDeactivate.Enabled = true;
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            var user = BuildUserFromForm();
            if (user is null)
                return;

            var result = await _userService.AddUserAsync(user, txtPassword.Text);
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
            if (_selectedUserId is null)
            {
                MessageBox.Show("Select a user from the grid first.", "No User Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var user = BuildUserFromForm();
            if (user is null)
                return;

            user.UserId = _selectedUserId.Value;

            // Leave the password box blank to keep the existing password unchanged.
            var newPassword = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text;

            var result = await _userService.UpdateUserAsync(user, newPassword);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshGridAsync();
            ClearForm();
        }

        private async void btnDeactivate_Click(object sender, EventArgs e)
        {
            if (_selectedUserId is null)
            {
                MessageBox.Show("Select a user from the grid first.", "No User Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_selectedUserId.Value == _currentUser.UserId)
            {
                MessageBox.Show("You can't deactivate your own account while logged in.",
                    "Not Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "This deactivates the account - the user will no longer be able to log in. Continue?",
                "Confirm Deactivate", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            await _userService.DeactivateUserAsync(_selectedUserId.Value);
            await RefreshGridAsync();
            ClearForm();
        }

        private User? BuildUserFromForm()
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            if (cboRole.SelectedItem is not string role)
            {
                MessageBox.Show("Pick a role.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            int? dentistId = null;
            if (role == "Dentist")
            {
                if (cboDentist.SelectedValue is not int selectedDentistId)
                {
                    MessageBox.Show("A Dentist-role account must be linked to a dentist record.",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }
                dentistId = selectedDentistId;
            }

            return new User
            {
                Username = txtUsername.Text.Trim(),
                Role = role,
                DentistId = dentistId
            };
        }

        private void ClearForm()
        {
            _selectedUserId = null;
            txtUsername.Clear();
            txtPassword.Clear();
            cboRole.SelectedIndex = -1;
            cboDentist.Enabled = false;
            dgvUsers.ClearSelection();
            btnUpdate.Enabled = false;
            btnDeactivate.Enabled = false;
        }
    }
}