using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

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

        private async void ucUserManagement_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, ucUserManagement_LoadAsync);

        private async Task ucUserManagement_LoadAsync()
        {
            // frmDashboard already hides the Users button for
            // non-admins, but if this control is ever reached another way, lock
            // it down here too rather than trusting the caller.
            if (!RoleAccess.Can(_currentUser, Permission.ManageUsers))
            {
                foreach (Control control in Controls)
                    control.Enabled = false;

                lblAdminOnly.Text = "Only an Admin account can manage users.";
                lblAdminOnly.Visible = true;
                return;
            }

            cboRole.Items.AddRange([Roles.Admin, Roles.Receptionist, Roles.Dentist]);
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
            var dentists = UiMessages.Items(await _dentistService.GetAllDentistsAsync(_currentUser));
            cboDentist.DataSource = dentists.ToList();
            cboDentist.DisplayMember = nameof(Dentist.FullName);
            cboDentist.ValueMember = nameof(Dentist.DentistId);
        }

        private async Task RefreshGridAsync()
        {
            var users = UiMessages.Items(await _userService.GetAllUsersAsync(_currentUser));
            GridHelper.Bind(dgvUsers, users.ToList());

            // Never display the password hash, even to an admin.
            if (dgvUsers.Columns["PasswordHash"] is { } hashCol) hashCol.Visible = false;
        }

        private void cboRole_SelectedIndexChanged(object? sender, EventArgs e)
        {
            cboDentist.Enabled = RoleAccess.RequiresDentist(cboRole.SelectedItem as string ?? string.Empty);
        }

        private void dgvUsers_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow?.DataBoundItem is not User user)
                return;

            _selectedUserId = user.UserId;
            txtUsername.Text = user.Username;
            txtPassword.Clear(); // never show or prefill a password/hash
            cboRole.SelectedItem = user.Role;
            cboDentist.Enabled = RoleAccess.RequiresDentist(user.Role);
            if (user.DentistId is int dentistId)
                cboDentist.SelectedValue = dentistId;

            btnUpdate.Enabled = true;
            btnDeactivate.Enabled = true;
        }

        private async void btnAdd_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, btnAdd_ClickAsync);

        private async Task btnAdd_ClickAsync()
        {
            var user = BuildUserFromForm();
            var result = await _userService.AddUserAsync(_currentUser, user, txtPassword.Text);
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
            if (_selectedUserId is null)
            {
                MessageBox.Show("Select a user from the grid first.", "No User Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var user = BuildUserFromForm();
            user.UserId = _selectedUserId.Value;
            user.IsActive = dgvUsers.CurrentRow?.DataBoundItem is not User selected || selected.IsActive;

            // Leave the password box blank to keep the existing password unchanged.
            var newPassword = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text;

            var result = await _userService.UpdateUserAsync(_currentUser, user, newPassword);
            if (!result.Success)
            {
                UiMessages.ShowError(result);
                return;
            }

            await RefreshGridAsync();
            ClearForm();
        }

        private async void btnDeactivate_Click(object? sender, EventArgs e) => await UiAction.RunAsync(this, btnDeactivate_ClickAsync);

        private async Task btnDeactivate_ClickAsync()
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

            var result = await _userService.DeactivateUserAsync(_currentUser, _selectedUserId.Value);
            if (!result.Success) { UiMessages.ShowError(result); return; }
            await RefreshGridAsync();
            ClearForm();
        }

        private User BuildUserFromForm() => new()
        {
            Username = txtUsername.Text.Trim(),
            Role = cboRole.SelectedItem as string ?? string.Empty,
            DentistId = cboDentist.SelectedValue is int id ? id : null
        };

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
