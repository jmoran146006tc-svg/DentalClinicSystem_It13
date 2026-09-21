using DentalClinicSystem.Forms;
using DentalClinicSystem.Models;
using DentalClinicSystem.Services;

namespace DentalClinicSystem
{
    public partial class frmLogin : Form
    {
        private readonly IAuthService _authService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;
        private readonly IAppointmentService _appointmentService;
        private readonly ITreatmentService _treatmentService;
        private readonly ITreatmentTypeService _treatmentTypeService;
        private readonly IUserService _userService;

        public frmLogin(
            IAuthService authService,
            IPatientService patientService,
            IDentistService dentistService,
            IAppointmentService appointmentService,
            ITreatmentService treatmentService,
            ITreatmentTypeService treatmentTypeService,
            IUserService userService)
        {
            InitializeComponent();
            _authService = authService;
            _patientService = patientService;
            _dentistService = dentistService;
            _appointmentService = appointmentService;
            _treatmentService = treatmentService;
            _treatmentTypeService = treatmentTypeService;
            _userService = userService;
        }

        private async void OnLoginAttempt(string username, string password)
        {
            var result = await _authService.LoginAsync(username, password);
            if (!result.Success)
            {
                MessageBox.Show(result.ErrorMessage, "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dashboard = new frmDashboard(
                result.Data, _authService, _patientService, _dentistService,
                _appointmentService, _treatmentService, _treatmentTypeService, _userService);
            dashboard.Show();
            Hide();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            OnLoginAttempt(txtUsername.Text, txtPassword.Text);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }
    }
}