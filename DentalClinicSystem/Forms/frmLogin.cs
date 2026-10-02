using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Forms;

namespace DentalClinicSystem
{
    public partial class frmLogin : Form
    {

        private readonly AppServices _services;

        public frmLogin(
            AppServices services)
        {
            InitializeComponent();
            _services = services;
        }

        private async Task OnLoginAttempt(string username, string password)
        {
            var result = await _services.Auth.LoginAsync(username, password);
            if (!result.Success || result.Data is null)
            {
                UiMessages.ShowError(result);
                return;
            }

            var dashboard = new frmDashboard(
                result.Data, this, _services);
            dashboard.Show();
            Hide();
        }

        private async void btnLogin_Click(object? sender, EventArgs e) =>
            await UiAction.RunAsync(this, () => OnLoginAttempt(txtUsername.Text, txtPassword.Text));

        public void ShowAfterLogout()
        {
            txtPassword.Clear();
            Show();
            Activate();
        }

    }
}
