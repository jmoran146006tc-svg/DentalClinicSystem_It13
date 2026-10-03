using DentalClinicSystem.Service;
using DentalClinicSystem.Models;
using DentalClinicSystem.Helpers;

namespace DentalClinicSystem.Forms
{
    public partial class frmDashboard : Form
    {
        private readonly User _currentUser;
        private readonly frmLogin _login;
        private readonly Panel _contentHost = new() { Dock = DockStyle.Fill };
        private readonly IReadOnlyList<NavItem> _navigationItems;
        private readonly Color _navigationColor;
        private static readonly Color ActiveNavigationColor = Color.FromArgb(47, 169, 174);
        private bool _loggingOut;

        private readonly AppServices _services;

        public frmDashboard(
            User currentUser,
            frmLogin login,
            AppServices services)
        {
            InitializeComponent();
            _services = services;

            _currentUser = currentUser;
            _login = login;
            _navigationColor = btnDashboard.BackColor;
            _navigationItems = CreateNavigation();
            ConfigureContent();
            ConfigureNavigation();
            FormClosed += (_, _) => { if (!_loggingOut) Application.Exit(); };
            Load += (_, _) => Navigate(_navigationItems[0]);
        }

        private void ConfigureContent()
        {
            tableLayoutPanel2.Dispose();
            lblDateTime.Visible = false;
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Height = 60;
            lblWc.Text = $"Welcome, {_currentUser.Username} ({_currentUser.Role})";
            pnlContent.Controls.Add(_contentHost);
            _contentHost.BringToFront();
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
                BackColor = _navigationColor,
                Padding = btnDashboard.Padding,
                Size = btnDashboard.Size,
                TextAlign = btnDashboard.TextAlign,
                UseVisualStyleBackColor = false
            };
            return [
                new("Dashboard", Permission.ViewDashboard, () => PageFactory.CreateDashboard(_currentUser), btnDashboard),
                new("Patients", Permission.ViewPatients, () => PageFactory.CreatePatients(_services.Patients, _currentUser), btnPatients),
                new("Dentists", Permission.ViewDentists, () => PageFactory.CreateDentists(_services.Dentists, _currentUser), btnDentists),
                new("Appointments", Permission.ViewAppointments, () => PageFactory.CreateAppointments(_services.Appointments, _services.Patients, _services.Dentists, _currentUser), btnAppointments),
                new("Treatments", Permission.ViewTreatments, () => PageFactory.CreateTreatments(_services.Treatments, _services.Appointments, _services.TreatmentTypes, _currentUser), btnTreatments),
                new("Users", Permission.ViewUsers, () => PageFactory.CreateUsers(_services.Users, _services.Dentists, _currentUser), btnUsers),
                new("Reports", Permission.ViewReports, () => PageFactory.CreateReports(_services.Reports, _currentUser), btnReports),
                new("Logout", Permission.ViewDashboard, null, btnLogout)
            ];
        }

        private void ConfigureNavigation()
        {
            var navigation = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            foreach (var item in _navigationItems)
            {
                item.Button.Text = item.Text;
                item.Button.Visible = RoleAccess.Can(_currentUser, item.RequiredPermission);
                item.Button.Margin = Padding.Empty;
                item.Button.Click += (_, _) => Navigate(item);
                if (item.CreatePage is null) item.Button.Dock = DockStyle.Bottom;
                else navigation.Controls.Add(item.Button);
            }
            pnlSidebar.Controls.Add(navigation);
            navigation.BringToFront();
        }

        private void Navigate(NavItem item)
        {
            if (!RoleAccess.Can(_currentUser, item.RequiredPermission)) return;
            if (item.CreatePage is not { } createPage) { Logout(); return; }
            ShowPage(createPage());
            foreach (var navigationItem in _navigationItems)
                navigationItem.Button.BackColor = navigationItem == item ? ActiveNavigationColor : _navigationColor;
        }

        private void ShowPage(Control page)
        {
            _contentHost.SuspendLayout();
            try
            {
                foreach (Control previous in _contentHost.Controls.Cast<Control>().ToArray()) previous.Dispose();
                page.Dock = DockStyle.Fill;
                _contentHost.Controls.Add(page);
            }
            finally { _contentHost.ResumeLayout(true); }
        }

        private void Logout()
        {
            _loggingOut = true;
            _login.ShowAfterLogout();
            Close();
        }

        private void timer1_Tick(object? sender, EventArgs e)
        {
            _lblClock.Text = DateTime.Now.ToString(AppointmentLabels.ClockFormat);
        }

    }
}
