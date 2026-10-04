using DentalClinicSystem.Service;
using DentalClinicSystem.Models;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Helpers.Native;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Forms
{
    public partial class frmDashboard : Form
    {
        private readonly User _currentUser;
        private readonly frmLogin _login;
        private readonly Panel _contentHost = new() { Dock = DockStyle.Fill };
        private readonly IReadOnlyList<NavItem> _navigationItems;
        private readonly FlowLayoutPanel _navigation = new() { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(Space.Xl, 0, Space.Xl, 0), BackColor = Palette.Surface };
        private bool _loggingOut;
        private readonly NavIndicator _indicator = new() { Name = "navIndicator", Visible = false };
        private Button? _activeNavigation;

        private readonly AppServices _services;
        private readonly TransitionHost _transitions;
#if DEBUG
        private frmStyleGuide? _styleGuide;
#endif

        public frmDashboard(
            User currentUser,
            frmLogin login,
            AppServices services)
        {
            InitializeComponent();
            _services = services;

            _currentUser = currentUser;
            _login = login;
            _navigationItems = CreateNavigation();
            ConfigureContent();
            ConfigureNavigation();
            MinimumSize = Metrics.MinimumWindow;
            _transitions = new TransitionHost(_contentHost);
            WindowChrome.Apply(this);
#if DEBUG
            KeyPreview = true;
            KeyDown += OpenStyleGuide;
#endif
            FormClosed += (_, _) => { if (!_loggingOut) Application.Exit(); };
            Load += (_, _) => Navigate(_navigationItems[0]);
            Shown += (_, _) => MoveIndicator();
        }

        private void ConfigureContent()
        {
            tableLayoutPanel2.Dispose();
            lblDateTime.Visible = false;
            lblWelcome.Visible = false;
            var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = Metrics.TopBarHeight, ColumnCount = 2, Padding = new Padding(Space.Xl, 0, Space.Xl, 0), BackColor = Palette.Surface };
            header.ColumnStyles.Add(new(SizeType.Percent, 100)); header.ColumnStyles.Add(new(SizeType.AutoSize));
            lblWc.Font = Typography.Heading; lblWc.ForeColor = Palette.Ink900; lblWc.Anchor = AnchorStyles.Left;
            _lblClock.Font = Typography.Caption; _lblClock.ForeColor = Palette.Ink500; _lblClock.Anchor = AnchorStyles.Right;
            header.Controls.Add(lblWc, 0, 0); header.Controls.Add(_lblClock, 1, 0);
            header.Paint += (_, e) => { using var pen = new Pen(Palette.Line); e.Graphics.DrawLine(pen, 0, header.Height - Metrics.Border, header.Width, header.Height - Metrics.Border); };
            tableLayoutPanel1.Dispose();
            pnlContent.Controls.Add(_contentHost);
            pnlContent.Controls.Add(header); _contentHost.BringToFront();
            pnlContent.BackColor = Palette.Canvas;
            timer1_Tick(this, EventArgs.Empty);
        }

        private IReadOnlyList<NavItem> CreateNavigation()
        {
            var btnReports = new Button
            {
                Name = "btnReports",
                FlatStyle = btnDashboard.FlatStyle,
                Font = btnDashboard.Font,
                ForeColor = btnDashboard.ForeColor,
                BackColor = Palette.Surface,
                Padding = btnDashboard.Padding,
                Size = btnDashboard.Size,
                TextAlign = btnDashboard.TextAlign,
                UseVisualStyleBackColor = false
            };
            return [
                new("Dashboard", Permission.ViewDashboard, () => PageFactory.CreateDashboard(_currentUser), btnDashboard),
                new("Patients", Permission.ViewPatients, () => PageFactory.CreatePatients(_services.Patients, _currentUser), btnPatients),
                new("Dentists", Permission.ViewDentists, () => PageFactory.CreateDentists(_services.Dentists, _currentUser), btnDentists),
                new("Appointments", Permission.ViewAppointments, () => PageFactory.CreateAppointments(_services.Appointments, _services.Patients, _services.Dentists, _services.TreatmentTypes, _currentUser), btnAppointments),
                new("Treatments", Permission.ViewTreatments, () => PageFactory.CreateTreatments(_services.Treatments, _services.Appointments, _services.TreatmentTypes, _currentUser), btnTreatments),
                new("Users", Permission.ViewUsers, () => PageFactory.CreateUsers(_services.Users, _services.Dentists, _currentUser), btnUsers),
                new("Reports", Permission.ViewReports, () => PageFactory.CreateReports(_services.Reports, _currentUser), btnReports),
                new("Logout", Permission.ViewDashboard, null, btnLogout)
            ];
        }

        private void ConfigureNavigation()
        {
            pnlSidebar.Width = Metrics.SidebarWidth; pnlSidebar.BackColor = Palette.Surface;
            var brand = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Metrics.EmptyHeight + Space.Xl, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(Space.Xl), BackColor = Palette.Surface };
            brand.Controls.Add(new IconTile(IconKind.Dentist));
            brand.Controls.Add(new Label { Text = "Dental Care", Font = Typography.Title, ForeColor = Palette.Ink900, AutoSize = true });
            brand.Controls.Add(new Label { Text = "Clinic Management", Font = Typography.Caption, ForeColor = Palette.Ink500, AutoSize = true });
            brand.Controls.Add(new Label { Text = "MENU", Font = Typography.Label, ForeColor = Palette.Ink500, AutoSize = true, Margin = new Padding(0, Space.Lg, 0, 0) });
            var account = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = Metrics.NavHeight * 2 + Space.Sm + Space.Md * 3, ColumnCount = 1, RowCount = 3, Padding = new Padding(Space.Xl, Space.Md, Space.Xl, Space.Md), BackColor = Palette.Surface };
            account.ColumnStyles.Add(new(SizeType.Percent, 100));
            account.RowStyles.Add(new(SizeType.Absolute, Metrics.NavHeight + Space.Sm)); account.RowStyles.Add(new(SizeType.Absolute, Space.Md)); account.RowStyles.Add(new(SizeType.Percent, 100));
            var profile = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Margin = Padding.Empty, BackColor = Palette.Surface };
            profile.ColumnStyles.Add(new(SizeType.Absolute, Metrics.NavHeight + Space.Sm)); profile.ColumnStyles.Add(new(SizeType.Percent, 100));
            profile.RowStyles.Add(new(SizeType.Percent, 50)); profile.RowStyles.Add(new(SizeType.Percent, 50));
            var avatar = new Avatar(_currentUser.Username) { Margin = Padding.Empty }; profile.Controls.Add(avatar, 0, 0); profile.SetRowSpan(avatar, 2);
            var username = new Label { Text = _currentUser.Username, Font = Typography.Label, ForeColor = Palette.Ink900, Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.BottomLeft, Margin = Padding.Empty, AccessibleName = _currentUser.Username };
            Tooltips.Attach(username, _currentUser.Username); profile.Controls.Add(username, 1, 0);
            profile.Controls.Add(new Label { Text = _currentUser.Role, Font = Typography.Caption, ForeColor = Palette.Ink500, Dock = DockStyle.Fill, Margin = Padding.Empty }, 1, 1);
            account.Controls.Add(profile, 0, 0);
            var divider = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, (Space.Md - Metrics.Border) / 2, 0, Space.Md / 2), BackColor = Palette.Line }; account.Controls.Add(divider, 0, 1);
            btnLogout.Dock = DockStyle.None; btnLogout.Text = "Logout";
            ButtonStyler.NavigationGhost(btnLogout); btnLogout.Size = new(Metrics.SidebarWidth - Space.Xl * 2, Metrics.NavHeight);
            btnLogout.Dock = DockStyle.Fill; btnLogout.Margin = Padding.Empty; btnLogout.AccessibleName = "Logout";
            account.Controls.Add(btnLogout, 0, 2);
            IconKind[] icons = [IconKind.Dashboard, IconKind.Patients, IconKind.Dentist, IconKind.Appointments, IconKind.Treatments, IconKind.Users, IconKind.Reports];
            var index = 0;
            foreach (var item in _navigationItems)
            {
                item.Button.Text = item.Text;
                item.Button.Visible = RoleAccess.Can(_currentUser, item.RequiredPermission);
                item.Button.Click += (_, _) => Navigate(item);
                if (item.CreatePage is not null) { NavButtonStyler.Attach(item.Button, icons[index++]); _navigation.Controls.Add(item.Button); }
            }
            pnlSidebar.Controls.Add(_navigation); pnlSidebar.Controls.Add(account); pnlSidebar.Controls.Add(brand);
            _navigation.BringToFront();
            pnlSidebar.Controls.Add(_indicator); _indicator.BringToFront();
            _navigation.Scroll += (_, _) => MoveIndicator(); _navigation.Layout += (_, _) => MoveIndicator();
            pnlSidebar.Paint += (_, e) => { using var pen = new Pen(Palette.Line); e.Graphics.DrawLine(pen, pnlSidebar.Width - Metrics.Border, 0, pnlSidebar.Width - Metrics.Border, pnlSidebar.Height); };
        }

        private void Navigate(NavItem item)
        {
            if (!RoleAccess.Can(_currentUser, item.RequiredPermission)) return;
            if (item.CreatePage is not { } createPage) { Logout(); return; }
            ShowPage(createPage());
            lblWc.Text = item.Text;
            Text = $"{item.Text} - Dental Care";
            _activeNavigation = item.Button; MoveIndicator();
            foreach (var navigationItem in _navigationItems)
                NavButtonStyler.Select(navigationItem.Button, navigationItem == item);
        }
        private void MoveIndicator()
        {
            if (_activeNavigation is not { Visible: true } button || !_navigation.Visible) return;
            // The indicator is a sidebar sibling so the FlowLayoutPanel never reflows for its animation.
            var point = pnlSidebar.PointToClient(button.Parent!.PointToScreen(button.Location));
            _indicator.Visible = point.Y >= _navigation.Top && point.Y + button.Height <= _navigation.Bottom;
            _indicator.Left = Space.Sm;
            if (_indicator.Visible) _indicator.MoveTo(point.Y, button.Height);
        }

        private void ShowPage(Control page)
        {
            _transitions.Show(page);
        }
#if DEBUG
        private void OpenStyleGuide(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F12 || !e.Control || !e.Shift) return;
            e.SuppressKeyPress = true;
            if (_styleGuide is null || _styleGuide.IsDisposed) _styleGuide = new frmStyleGuide();
            _styleGuide.Show(this); _styleGuide.Activate();
        }
#endif

        private void Logout()
        {
            _loggingOut = true;
            _login.ShowAfterLogout();
            Close();
        }

        private void timer1_Tick(object? sender, EventArgs e)
        {
            _lblClock.Text = DisplayFormat.DateTime(DateTime.Now);
        }

    }
}
