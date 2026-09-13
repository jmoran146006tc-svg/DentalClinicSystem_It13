
using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucPatientRecords : UserControl
    {
        private readonly IPatientService _patientService;

        public ucPatientRecords(IPatientService patientService)
        {
            InitializeComponent();
            _patientService = patientService;
        }
    }
}