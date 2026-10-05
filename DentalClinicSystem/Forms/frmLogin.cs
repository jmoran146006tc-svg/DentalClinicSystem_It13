using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Native;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem
{
    public partial class frmLogin : Form
    {
        private readonly AppServices _services;
        private readonly InlineAlert _alert = new() { Visible = false, TabStop = false, Dock = DockStyle.None };
        private readonly ClinicLoginBackdrop _backdrop = new();
        private FieldBox _username = null!, _password = null!;
        private LoginHero _hero = null!;

        public frmLogin(AppServices services)
        {
            InitializeComponent();
            _services = services;
            BuildLoginLayout();
            AcceptButton = btnLogin;
            WindowChrome.Apply(this);
        }

        private void BuildLoginLayout()
        {
            SuspendLayout();
            AutoSize = false; AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            BackgroundImage = null; BackColor = Palette.Canvas;
            pictureBox1.Visible = false; panel1.Visible = false;
            label1.Visible = label2.Visible = label3.Visible = false;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
            _username = new FieldBox(txtUsername) { LeadingIcon = IconKind.User, BorderOverride = Palette.LoginFieldBorder, TabIndex = 0 };
            _password = new FieldBox(txtPassword, FieldKind.Password) { LeadingIcon = IconKind.Lock, BorderOverride = Palette.LoginFieldBorder, TabIndex = 1 };
            txtUsername.AccessibleName = "Username"; txtPassword.AccessibleName = "Password";
            txtUsername.PlaceholderText = "Enter your username"; txtPassword.PlaceholderText = "Enter your password";
            txtUsername.TabIndex = 0; txtPassword.TabIndex = 0;
            InputRules.ApplyMaxLengths((txtUsername, FieldLimits.Username), (txtPassword, FieldLimits.Password));
            btnLogin.Text = "Sign in"; btnLogin.Dock = DockStyle.None; btnLogin.Margin = Padding.Empty; btnLogin.TabIndex = 2;
            ButtonStyler.Attach(btnLogin, ButtonVariant.Primary);
            _hero = new LoginHero(_backdrop, _username, _password, btnLogin, _alert);
            _backdrop.Controls.Add(_hero); Controls.Add(_backdrop); _backdrop.BringToFront();
            _backdrop.SizeChanged += (_, _) => _hero.Bounds = _backdrop.ClientRectangle;
            void UpdateCaps() => _hero.SetCapsLock(Control.IsKeyLocked(Keys.CapsLock));
            txtPassword.KeyUp += (_, _) => UpdateCaps(); txtPassword.GotFocus += (_, _) => UpdateCaps(); UpdateCaps();
            UiMessages.RegisterAlertHost(this, _alert);
            FitWindowToPhoto();
            ResumeLayout(true); _hero.Bounds = _backdrop.ClientRectangle; _hero.Relayout();
            Shown += (_, _) =>
            {
                txtUsername.Focus();
                _backdrop.StartEntrance(); _hero.StartEntrance();
                if (!MotionSystem.Enabled) { Opacity = 1; return; }
                MotionSystem.Animator.Run(this, "login-enter", 0, 1, MotionSystem.Fast, Easing.EaseOutCubic, t => Opacity = Math.Clamp(t, 0, 1));
            };
            VisibleChanged += (_, _) => { if (!Visible) { _backdrop.SettleEntrance(); _hero.SettleEntrance(); } };
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (_hero is null) return;
            _backdrop.Bounds = ClientRectangle;
            _hero.Bounds = _backdrop.ClientRectangle;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e); FitWindowToPhoto(); _hero.Relayout();
            if (StartPosition == FormStartPosition.CenterScreen) CenterToScreen();
        }
        private void FitWindowToPhoto()
        {
            var available = Screen.FromControl(this).WorkingArea;
            var margin = Metrics.Scale(this, Space.Xl);
            var chrome = SizeFromClientSize(Size.Empty);
            var width = Math.Min(Metrics.Scale(this, Metrics.LoginWidth), Math.Max(1, available.Width - margin * 2 - chrome.Width));
            var height = Math.Min((int)Math.Round(width / LoginArtwork.PhotoAspect), Math.Max(1, available.Height - margin * 2 - chrome.Height));
            ClientSize = new((int)Math.Round(height * LoginArtwork.PhotoAspect), height);
        }
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            if (_hero is null) return;
            _backdrop.SettleEntrance(); _hero.SettleEntrance(); FitWindowToPhoto(); _hero.Relayout();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.P)) { _password.TogglePassword(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async Task OnLoginAttempt(string username, string password)
        {
            var result = await _services.Auth.LoginAsync(username, password);
            if (!result.Success || result.Data is null)
            {
                UiMessages.ShowError(ServiceResult.Fail("Sign in failed. Check your credentials and try again."));
                return;
            }

            if (IsDisposed || !Visible || !await FadeOutAsync()) return;
            var dashboard = new frmDashboard(
                result.Data, this, _services);
            dashboard.Show();
            Hide();
        }

        private async void btnLogin_Click(object? sender, EventArgs e) =>
            await UiAction.RunAsync(this, () => OnLoginAttempt(txtUsername.Text, txtPassword.Text), btnLogin);

        private async Task<bool> FadeOutAsync()
        {
            if (!MotionSystem.Enabled || WindowState == FormWindowState.Minimized) return true;
            var complete = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Cancel(object? sender, EventArgs e) { if (IsDisposed || !Visible) complete.TrySetResult(false); }
            void Minimized(object? sender, EventArgs e) { if (WindowState == FormWindowState.Minimized) complete.TrySetResult(true); }
            Disposed += Cancel; VisibleChanged += Cancel; SizeChanged += Minimized;
            try
            {
                MotionSystem.Animator.Run(this, "login-exit", (float)Opacity, 0, MotionSystem.Fast, Easing.EaseOutCubic,
                    value => Opacity = Math.Clamp(value, 0, 1), () => complete.TrySetResult(true));
                return await complete.Task;
            }
            finally { Disposed -= Cancel; VisibleChanged -= Cancel; SizeChanged -= Minimized; MotionSystem.Animator.Cancel(this, "login-exit"); }
        }

        public void ShowAfterLogout()
        {
            txtPassword.Clear();
            _backdrop.SettleEntrance(); _hero.SettleEntrance();
            Opacity = 1; _alert.Dismiss();
            Show();
            Activate();
            txtUsername.Focus();
        }

    }
}
