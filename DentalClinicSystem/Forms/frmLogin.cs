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
        private readonly Panel _formColumn = new() { BackColor = Palette.Surface, TabStop = false };
        private readonly InlineAlert _alert = new() { Visible = false, Dock = DockStyle.None, TabStop = false };
        private readonly LoginBrand _backdrop = new();
        private readonly Panel _loginHost = new() { Dock = DockStyle.Fill, TabStop = false };
        private readonly Label _title = LoginLabel("Welcome back", Typography.Title, Palette.Ink900);
        private readonly Label _caption = LoginLabel("Sign in to your clinic account", Typography.Caption, Palette.Ink500);
        private readonly Label _caps = LoginLabel("Caps Lock is on", Typography.Caption, Palette.Warning.Text);
        private readonly Label _help = LoginLabel("Trouble signing in? Ask your clinic administrator.", Typography.Caption, Palette.Ink500, wrap: true);
        private readonly IconTile _tile = new(IconKind.Dentist);
        private LoginCard _card = null!;
        private FormField _username = null!, _password = null!;
        private bool _relayout;
        private bool _entering;
        private float _entranceProgress = 1;
        private Point _cardDestination;

        public frmLogin(AppServices services)
        {
            InitializeComponent();
            _services = services;
            BuildLoginLayout();
            AcceptButton = btnLogin;
            WindowChrome.Apply(this);
        }

        private static Label LoginLabel(string text, Font font, Color ink, bool wrap = false)
        {
            var label = new Label { Text = text, Font = font, ForeColor = ink, BackColor = Palette.Surface, Margin = Padding.Empty, TabStop = false };
            AlignLabel(label, wrap);
            return label;
        }

        private static void AlignLabel(Label label, bool wrap = false)
        {
            // Native Label adds font-dependent side-bearing padding. Paint all
            // login copy and field captions with the design system's NoPadding.
            label.Paint += (_, e) =>
            {
                e.Graphics.Clear(Palette.Surface);
                TextRenderer.DrawText(e.Graphics, label.Text, label.Font, label.ClientRectangle, label.ForeColor,
                    wrap ? TextFormatFlags.WordBreak | TextFormatFlags.NoPadding : DesignPaint.TextFlags);
            };
        }

        private void BuildLoginLayout()
        {
            SuspendLayout();
            AutoSize = false; AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            BackgroundImage = null; BackColor = Palette.BrandPressed;
            pictureBox1.Visible = false; panel1.Visible = false;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false; StartPosition = FormStartPosition.CenterScreen;
            _username = UiFactory.Field(txtUsername, "Username");
            _password = UiFactory.Field(txtPassword, "Password", FieldKind.Password);
            _username.TabIndex = 0; _password.TabIndex = 1;
            foreach (var field in new[] { _username, _password })
            {
                field.Margin = Padding.Empty; field.BackColor = Palette.Surface;
                var caption = field.Controls.OfType<Label>().Single(label => label.Text.Length > 0);
                caption.Margin = new Padding(0, 0, 0, Metrics.Scale(this, Space.Xs));
                AlignLabel(caption);
                // Eye remains keyboard accessible with Alt+P, while the primary
                // tab sequence is username -> password -> sign in.
                foreach (var action in PageActions(field)) action.TabStop = false;
            }
            txtUsername.TabIndex = 0; txtPassword.TabIndex = 0;
            InputRules.ApplyMaxLengths((txtUsername, FieldLimits.Username), (txtPassword, FieldLimits.Password));
            btnLogin.Text = "Sign in"; btnLogin.Dock = DockStyle.None; btnLogin.Margin = Padding.Empty; btnLogin.TabIndex = 2;
            ButtonStyler.Attach(btnLogin, ButtonVariant.Primary);
            _formColumn.Controls.AddRange([_tile, _title, _caption, _username, _password, _caps, _alert, btnLogin, _help]);
            _card = new LoginCard(_backdrop) { Name = "loginCard", Margin = Padding.Empty, TabStop = false };
            _card.Content.Controls.Add(_formColumn);
            // Nest the card in the backdrop so both native painting and
            // DrawToBitmap compose the background before its child windows.
            _backdrop.Controls.Add(_card); _loginHost.Controls.Add(_backdrop);
            Controls.Add(_loginHost); _loginHost.BringToFront();
            _caps.Visible = Control.IsKeyLocked(Keys.CapsLock);
            void UpdateCaps() { _caps.Visible = Control.IsKeyLocked(Keys.CapsLock); }
            txtPassword.KeyUp += (_, _) => UpdateCaps(); txtPassword.GotFocus += (_, _) => UpdateCaps();
            _alert.SizeChanged += (_, _) => Relayout(); _alert.VisibleChanged += (_, _) => Relayout();
            _loginHost.SizeChanged += (_, _) => Relayout();
            UiMessages.RegisterAlertHost(this, _alert);
            FitWindowToPhoto();
            ResumeLayout(true);
            Relayout();
            Shown += (_, _) => { StartEntrance(); txtUsername.Focus(); };
        }

        private static IEnumerable<Button> PageActions(FormField field) => field.Box.Controls.OfType<Button>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // The handle now has its monitor DPI. Re-clamp before the first
            // entrance, since initial WinForms scaling can enlarge ClientSize.
            FitWindowToPhoto(); Relayout();
            if (StartPosition == FormStartPosition.CenterScreen) CenterToScreen();
        }

        private void FitWindowToPhoto()
        {
            var available = Screen.FromControl(this).WorkingArea;
            var margin = Metrics.Scale(this, Space.Xl);
            var chrome = SizeFromClientSize(Size.Empty);
            var width = Math.Min(Metrics.Scale(this, Metrics.LoginWidth), Math.Max(1, available.Width - margin * 2 - chrome.Width));
            var height = Math.Min((int)Math.Round(width / LoginBrand.PhotoAspect), Math.Max(1, available.Height - margin * 2 - chrome.Height));
            width = (int)Math.Round(height * LoginBrand.PhotoAspect);
            ClientSize = new(width, height);
        }

        private void Relayout()
        {
            if (_relayout || _card is null || _username is null) return;
            _relayout = true;
            try
            {
                int S(int value) => Metrics.Scale(this, value);
                var width = S(Metrics.LoginFormWidth);
                var shadow = S(Elevation.Padding(ElevationLevel.E3));
                var padding = S(Space.Xl);
                var margin = S(Space.Xl);
                var compact = _loginHost.Height < S(Metrics.LoginTargetHeight);
                var gap = compact ? S(Space.Md) : S(Space.Lg);
                var top = 0;
                void Place(Control control, int height, int after = 0)
                {
                    control.SetBounds(0, top, width, height);
                    top += height + after;
                }
                _tile.SetBounds(0, top, S(Metrics.LoginTileSize), S(Metrics.LoginTileSize));
                top += _tile.Height + gap;
                Place(_title, _title.Font.Height + S(Space.Xs), S(Space.Xs));
                Place(_caption, _caption.Font.Height + S(Space.Xs), gap);
                _username.Width = width; _password.Width = width;
                _username.Box.Height = _username.Box.MinimumSize.Height;
                _password.Box.Height = _password.Box.MinimumSize.Height;
                _username.PerformLayout(); _password.PerformLayout();
                Place(_username, _username.Height, gap);
                Place(_password, _password.Height, S(Space.Xs));
                Place(_caps, _caps.Font.Height + S(Space.Xs), S(Space.Sm));
                if (_alert.Visible) Place(_alert, Math.Max(0, _alert.Height), S(Space.Sm));
                else _alert.Width = width;
                Place(btnLogin, S(Metrics.ControlHeight), gap);
                var helpHeight = TextRenderer.MeasureText(_help.Text, _help.Font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding).Height;
                Place(_help, helpHeight);
                _formColumn.Size = new(width, top);
                _card.Size = new(width + (padding + shadow) * 2, top + (padding + shadow) * 2);
                _card.Padding = new Padding(padding + shadow);
                _formColumn.Location = Point.Empty;
                _cardDestination = new(Math.Max(margin, _loginHost.Width - margin - _card.Width), Math.Max(margin, (_loginHost.Height - _card.Height) / 2));
                _card.Location = new(_cardDestination.X, _cardDestination.Y + (_entering ? (int)(S(Metrics.Slide) * (1 - _entranceProgress)) : 0));
                _backdrop.SetStoryRight(_cardDestination.X - S(Space.Lg));
            }
            finally { _relayout = false; }
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);
            if (_card is null) return;
            SettleEntrance(); FitWindowToPhoto(); Relayout();
        }

        private void StartEntrance()
        {
            Relayout();
            if (!MotionSystem.Enabled) { SettleEntrance(); return; }
            _entering = true;
            _card.CaptureEntrance();
            MotionSystem.Animator.Run(_backdrop, "login-brand-enter", 0, 1, MotionSystem.Base, Easing.EaseOutCubic, _backdrop.SetEntranceProgress);
            MotionSystem.Animator.Run(_card, "login-card-enter", 0, 1, MotionSystem.Slow, Easing.EaseOutCubic,
                value =>
                {
                    _entranceProgress = value;
                    _card.SetEntranceProgress(value);
                    _card.Location = new(_cardDestination.X, _cardDestination.Y + (int)(Metrics.Scale(this, Metrics.Slide) * (1 - value)));
                    if (value >= 1) { _entering = false; if (Visible) txtUsername.Focus(); }
                });
        }

        private void SettleEntrance()
        {
            MotionSystem.Animator.Cancel(_card, "login-card-enter");
            MotionSystem.Animator.Cancel(_backdrop, "login-brand-enter");
            _entering = false; _entranceProgress = 1;
            _card.SetEntranceProgress(1); _backdrop.SetEntranceProgress(1);
            Relayout();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.P))
            {
                PageActions(_password).Single().PerformClick();
                return true;
            }
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
            SettleEntrance();
            Opacity = 1; _alert.Dismiss();
            Show();
            Activate();
            txtUsername.Focus();
        }

    }
}
