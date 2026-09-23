using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class frmDashboard : Form
    {
        private readonly User _currentUser;
        private readonly IAuthService _authService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;
        private readonly IAppointmentService _appointmentService;
        private readonly ITreatmentService _treatmentService;
        private readonly ITreatmentTypeService _treatmentTypeService;
        private readonly IUserService _userService;

        public frmDashboard(
            User currentUser,
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
            pnlContent.Controls.Clear();
            page.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(page);
        }

        private void ShowDashboardHome()
        {
            pnlContent.Controls.Clear();
            lblWelcome.Text = $"Welcome, {_currentUser.Username}!";
            pnlContent.Controls.Add(lblWelcome);
        }

        private void Logout()
        {
            var loginForm = new frmLogin(
                _authService, _patientService, _dentistService,
                _appointmentService, _treatmentService, _treatmentTypeService, _userService);
            loginForm.Show();
            Close();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {

        }
    }
}