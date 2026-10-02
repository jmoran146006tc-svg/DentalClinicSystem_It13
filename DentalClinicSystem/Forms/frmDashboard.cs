using DentalClinicSystem.Service;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class frmDashboard : Form
    {
        private readonly User _currentUser;
        private readonly frmLogin _login;
        private readonly Panel _contentHost = new() { Dock = DockStyle.Fill };
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
            tableLayoutPanel2.Dispose();
            lblDateTime.Visible = false;
            tableLayoutPanel1.Dock = DockStyle.Top;
            tableLayoutPanel1.Height = 60;
            lblWc.Text = $"Welcome, {currentUser.Username} ({currentUser.Role})";
            pnlContent.Controls.Add(_contentHost);
            _contentHost.BringToFront();
            timer1_Tick(this, EventArgs.Empty);
            btnLogout.Dock = DockStyle.Bottom;
            var navigation = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
            foreach (var button in new[] { btnDashboard, btnPatients, btnDentists, btnAppointments, btnTreatments, btnUsers })
            {
                button.Margin = Padding.Empty;
                navigation.Controls.Add(button);
            }
            pnlSidebar.Controls.Add(navigation);
            navigation.BringToFront();
            FormClosed += (_, _) => { if (!_loggingOut) Application.Exit(); };

            btnDashboard.Click += (_, _) => ShowDashboardHome();
            btnPatients.Click += (_, _) => ShowPage(new ucPatientRecords(_services.Patients, _currentUser));
            btnDentists.Click += (_, _) => ShowPage(new ucDentistRecords(_services.Dentists, _currentUser));
            btnAppointments.Click += (_, _) => ShowPage(
                new ucAppointmentScheduler(_services.Appointments, _services.Patients, _services.Dentists, _currentUser));
            btnTreatments.Click += (_, _) => ShowPage(
                new ucTreatmentRecords(_services.Treatments, _services.Appointments, _services.TreatmentTypes, _currentUser));
            btnUsers.Click += (_, _) => ShowPage(new ucUserManagement(_services.Users, _services.Dentists, _currentUser));
            btnLogout.Click += (_, _) => Logout();

            btnPatients.Visible = RoleAccess.Can(_currentUser, Permission.ViewPatients);
            btnTreatments.Visible = RoleAccess.Can(_currentUser, Permission.ViewTreatments);
            btnDentists.Visible = RoleAccess.Can(_currentUser, Permission.ViewDentists);
            btnUsers.Visible = RoleAccess.Can(_currentUser, Permission.ViewUsers);

            Load += (_, _) => ShowDashboardHome();
        }

        private void ShowPage(Control page)
        {
            foreach (Control previous in _contentHost.Controls.Cast<Control>().ToArray()) previous.Dispose();
            page.Dock = DockStyle.Fill;
            _contentHost.Controls.Add(page);
        }

        private void ShowDashboardHome()
        {
            ShowPage(new Label { Text = "Dental clinic overview", AutoSize = false });
        }

        private void Logout()
        {
            _loggingOut = true;
            _login.ShowAfterLogout();
            Close();
        }

        private void timer1_Tick(object? sender, EventArgs e)
        {
            _lblClock.Text = DateTime.Now.ToString("dddd, dd MMM yyyy HH:mm:ss");
        }

    }
}
