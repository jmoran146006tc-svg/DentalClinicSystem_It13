using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public partial class ucUserManagement : UserControl
{
    private readonly IUserService _userService;
    private readonly IDentistService _dentistService;
    private readonly User _currentUser;
    private readonly CrudPageLayout _layout;
    private IReadOnlyList<User> _users = [];
    private User? _selected;
    private User? _savedUser;
    private sealed record UserRow(int UserId, string Username, string Role, string Dentist, bool IsActive, User Record);
    private Dictionary<int, string> _dentistNames = [];
    public ucUserManagement(IUserService userService, IDentistService dentistService, User currentUser)
    {
        InitializeComponent(); _userService = userService; _dentistService = dentistService; _currentUser = currentUser;
        _layout = new(this, "Users", "user", "Accounts, permissions and dentist links", dgvUsers, btnAdd, btnClear, ClearForm);
        _layout.AddRow(UiFactory.Field(txtUsername, "Username")); _layout.AddRow(UiFactory.Field(txtPassword, "Password", FieldKind.Password));
        _layout.AddRow(UiFactory.Field(cboRole, "Role", FieldKind.Choice)); _layout.AddRow(UiFactory.Field(cboDentist, "Dentist", FieldKind.Choice));
        InputRules.ApplyMaxLengths((txtUsername, FieldLimits.Username), (txtPassword, FieldLimits.Password));
        cboRole.Items.Clear(); cboRole.Items.AddRange([Roles.Admin, Roles.Receptionist, Roles.Dentist]);
        cboRole.SelectedIndexChanged += (_, _) => cboDentist.Enabled = RoleAccess.RequiresDentist(cboRole.SelectedItem as string ?? "");
        btnDeactivate.Text = "Deactivate"; btnDeactivate.Visible = true; btnDeactivate.Height = Metrics.ControlHeight; ButtonStyler.Attach(btnDeactivate, ButtonVariant.Danger); _layout.Actions.Controls.Add(btnDeactivate);
        btnClear.Click += (_, _) => ClearForm();
        btnDeactivate.Click += async (_, _) => await UiAction.RunAsync(this, DeactivateAsync, btnDeactivate);
        dgvUsers.SelectionChanged += SelectionChanged; _layout.Search.TextChanged += (_, _) => BindRows();
        GridTheme.MuteInactive<UserRow>(dgvUsers, row => !row.IsActive);
        Enabled = RoleAccess.Can(currentUser, Permission.ManageUsers); ClearForm();
        _layout.UseModal(SaveAsync, AfterSaveAsync);
    }
    private async void ucUserManagement_Load(object? sender, EventArgs e) => await UiAction.RunAsync(this, async () => { using var loading = _layout.Loading(); await LoadAsync(); });
    private async Task LoadAsync()
    {
        var dentists = UiMessages.Items(await _dentistService.GetAllDentistsAsync(_currentUser));
        _dentistNames = dentists.ToDictionary(d => d.DentistId, d => d.FullName);
        cboDentist.DataSource = dentists.Where(d => d.IsActive).Select(d => new DisplayOption(d.DentistId, d.FullName)).ToList();
        cboDentist.DisplayMember = nameof(DisplayOption.Display); cboDentist.ValueMember = nameof(DisplayOption.Id);
        await RefreshGridAsync();
    }
    private async Task RefreshGridAsync() { _users = UiMessages.Items(await _userService.GetAllUsersAsync(_currentUser)); if (!IsDisposed) BindRows(); }
    private void BindRows()
    {
        var rows = _users.Where(u => new[] { u.Username, u.Role }.Any(v => v.Contains(_layout.Search.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            .Select(u => new UserRow(u.UserId, u.Username, u.Role, u.DentistId is int id ? _dentistNames.GetValueOrDefault(id, "Unlinked") : "n/a", u.IsActive, u));
        GridHelper.Bind(dgvUsers, rows, row => row.UserId, "No users match your search.", "UserId", "Record");
        GridHelper.IdentityColumn<UserRow>(dgvUsers, "Username", row => (row.Username, row.IsActive ? "Active account" : "Inactive account")); ClearForm();
    }
    private void SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRecord is null || dgvUsers.SelectedRecord is not UserRow row) return;
        _selected = row.Record; txtUsername.Text = _selected.Username ?? ""; txtPassword.Clear(); cboRole.SelectedItem = _selected.Role;
        if (_selected.DentistId is int id) cboDentist.SelectedValue = id; else cboDentist.SelectedIndex = -1;
        btnDeactivate.Enabled = _selected.IsActive && _selected.UserId != _currentUser.UserId; _layout.SetEditing(true);
    }
    private async Task<bool> SaveAsync()
    {
        var user = new User { UserId = _selected?.UserId ?? 0, Username = txtUsername.Text.Trim(), Role = cboRole.SelectedItem as string ?? "",
            DentistId = RoleAccess.RequiresDentist(cboRole.SelectedItem as string ?? "") && cboDentist.SelectedValue is int id ? id : null, IsActive = _selected?.IsActive ?? true };
        var result = _selected is null ? await _userService.AddUserAsync(_currentUser, user, txtPassword.Text)
            : await _userService.UpdateUserAsync(_currentUser, user, string.IsNullOrEmpty(txtPassword.Text) ? null : txtPassword.Text);
        if (!result.Success) { UiMessages.ShowError(result); return false; }
        _savedUser = user; return true;
    }
    private async Task AfterSaveAsync()
    {
        var user = _savedUser!;
        await RefreshGridAsync(); UiMessages.ShowSuccess("User saved.");
        var saved = _users.FirstOrDefault(u => u.Username == user.Username); if (saved is not null) GridTheme.SelectAndFlash(dgvUsers, saved.UserId);
    }
    private async Task DeactivateAsync()
    {
        if (_selected is not { } user) return;
        if (user.UserId == _currentUser.UserId) { UiMessages.ShowError(ServiceResult.Fail("You cannot deactivate your own account.")); return; }
        if (!UiMessages.Confirm("Deactivate this account? The user will no longer be able to sign in.", "Deactivate user")) return;
        var result = await _userService.DeactivateUserAsync(_currentUser, user.UserId);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await RefreshGridAsync(); ClearForm(); UiMessages.ShowSuccess("User deactivated."); GridHelper.FlashRow(dgvUsers, user.UserId);
    }
    private void ClearForm() { _selected = null; txtUsername.Clear(); txtPassword.Clear(); cboRole.SelectedIndex = -1; cboDentist.Enabled = false; dgvUsers.ClearSelection(); btnDeactivate.Enabled = false; _layout?.SetEditing(false); _layout?.Alert.Dismiss(); }
}
