using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Native;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem
{
    public partial class frmLogin : Form
    {

        private readonly AppServices _services;
        private readonly FlowLayoutPanel _formColumn = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false, Width = Metrics.LoginFormWidth, BackColor = Palette.Surface };
        private readonly InlineAlert _alert = new() { Visible = false, Dock = DockStyle.None, Width = Metrics.LoginFormWidth };

        public frmLogin(
            AppServices services)
        {
            InitializeComponent();
            AcceptButton = btnLogin;
            _services = services;
            BuildLoginLayout();
            AcceptButton = btnLogin;
            WindowChrome.Apply(this);
        }
        private void BuildLoginLayout()
        {
            SuspendLayout(); AutoSize = false; AutoScaleMode = AutoScaleMode.Dpi; BackgroundImage = null;
            pictureBox1.Visible = false; panel1.Visible = false;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new(Metrics.LoginWidth, Metrics.LoginHeight); BackColor = Palette.Surface;
            var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
            split.ColumnStyles.Add(new(SizeType.Percent, 50)); split.ColumnStyles.Add(new(SizeType.Percent, 50));
            var right = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Surface };
            _formColumn.MaximumSize = new(Metrics.LoginFormWidth, 0);
            _formColumn.Controls.Add(new Label { Text = "Welcome back", Font = Typography.Title, ForeColor = Palette.Ink900, AutoSize = true });
            _formColumn.Controls.Add(new Label { Text = "Sign in to continue", Font = Typography.Caption, ForeColor = Palette.Ink500, AutoSize = true, Margin = new Padding(0, 0, 0, Space.Xl) });
            var username = UiFactory.Field(txtUsername, "Username"); var password = UiFactory.Field(txtPassword, "Password", FieldKind.Password);
            username.Width = password.Width = Metrics.LoginFormWidth;
            txtUsername.TabIndex = 0; txtPassword.TabIndex = 1;
            InputRules.ApplyMaxLengths((txtUsername, FieldLimits.Username), (txtPassword, FieldLimits.Password));
            _formColumn.Controls.Add(username); _formColumn.Controls.Add(password);
            var caps = new Label { Text = "Caps Lock is on", Font = Typography.Caption, ForeColor = Palette.Warning.Text, AutoSize = true, Visible = Control.IsKeyLocked(Keys.CapsLock) };
            txtPassword.KeyUp += (_, _) => caps.Visible = Control.IsKeyLocked(Keys.CapsLock);
            txtPassword.GotFocus += (_, _) => caps.Visible = Control.IsKeyLocked(Keys.CapsLock);
            _formColumn.Controls.Add(caps); _formColumn.Controls.Add(_alert);
            btnLogin.Text = "Sign in"; btnLogin.Dock = DockStyle.None; btnLogin.Size = new(Metrics.LoginFormWidth, Metrics.ControlHeight); btnLogin.TabIndex = 2;
            ButtonStyler.Attach(btnLogin, ButtonVariant.Primary); _formColumn.Controls.Add(btnLogin);
            right.Controls.Add(_formColumn);
            void Center() { _formColumn.Location = new(Math.Max(Space.Xl, (right.ClientSize.Width - _formColumn.Width) / 2), Math.Max(Space.Xl, (right.ClientSize.Height - _formColumn.Height) / 2)); }
            right.SizeChanged += (_, _) => Center(); _formColumn.SizeChanged += (_, _) => Center();
            split.Controls.Add(new LoginBrand(), 0, 0); split.Controls.Add(right, 1, 0); Controls.Add(split); split.BringToFront();
            UiMessages.RegisterAlertHost(this, _alert); ResumeLayout(true); Center();
            Shown += (_, _) => txtUsername.Focus();
        }

        private async Task OnLoginAttempt(string username, string password)
        {
            var result = await _services.Auth.LoginAsync(username, password);
            if (!result.Success || result.Data is null)
            {
                UiMessages.ShowError(ServiceResult.Fail("Sign in failed. Check your credentials and try again."));
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
