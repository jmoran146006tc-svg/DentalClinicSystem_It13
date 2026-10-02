using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class frmDashboard : Form
    {
        private readonly User _currentUser;
        private readonly frmLogin _login;
        private readonly Panel _contentHost = new() { Dock = DockStyle.Fill };
        private bool _loggingOut;
        private readonly IAuthService _authService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;
        private readonly IAppointmentService _appointmentService;
        private readonly ITreatmentService _treatmentService;
        private readonly ITreatmentTypeService _treatmentTypeService;
        private readonly IUserService _userService;

        public frmDashboard(
            User currentUser,
            frmLogin login,
            IAuthService authService,
            IPatientService patientService,
            IDentistService dentistService,
            IAppointmentService appointmentService,
            ITreatmentService treatmentService,
            ITreatmentTypeService treatmentTypeService,
            IUserService userService)
        {
            InitializeComponent();

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
            _authService = authService;
            _patientService = patientService;
            _dentistService = dentistService;
            _appointmentService = appointmentService;
            _treatmentService = treatmentService;
            _treatmentTypeService = treatmentTypeService;
            _userService = userService;

            btnDashboard.Click += (_, _) => ShowDashboardHome();
            btnPatients.Click += (_, _) => ShowPage(new ucPatientRecords(_patientService, canEdit: _currentUser.Role != "Dentist"));
            btnDentists.Click += (_, _) => ShowPage(new ucDentistRecords(_dentistService));
            btnAppointments.Click += (_, _) => ShowPage(
                new ucAppointmentScheduler(_appointmentService, _patientService, _dentistService));
            btnTreatments.Click += (_, _) => ShowPage(
                new ucTreatmentRecords(_treatmentService, _appointmentService, _treatmentTypeService, canEdit: _currentUser.Role != "Receptionist"));
            btnUsers.Click += (_, _) => ShowPage(new ucUserManagement(_userService, _dentistService, _currentUser));
            btnLogout.Click += (_, _) => Logout();

            // Admin sees every tab. Receptionist sees Patients/Appointments/Treatments
            // (Treatments is view-only for them - see ucTreatmentRecords). Dentist sees
            // Patients (view-only)/Appointments/Treatments, not Dentists or Users.
            btnPatients.Visible = true;
            btnDentists.Visible = _currentUser.Role == "Admin";
            btnUsers.Visible = _currentUser.Role == "Admin";

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
