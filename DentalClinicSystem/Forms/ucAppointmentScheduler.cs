using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucAppointmentScheduler : UserControl
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDentistService _dentistService;

        public ucAppointmentScheduler(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDentistService dentistService)
        {
            InitializeComponent();
            _appointmentService = appointmentService;
            _patientService = patientService;
            _dentistService = dentistService;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void ucAppointmentScheduler_Load(object sender, EventArgs e)
        {

        }
    }
}