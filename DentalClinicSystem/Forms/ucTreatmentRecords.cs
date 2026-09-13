using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucTreatmentRecords : UserControl
    {
        private readonly ITreatmentService _treatmentService;
        private readonly IAppointmentService _appointmentService;
        private readonly ITreatmentTypeService _treatmentTypeService;

        public ucTreatmentRecords(
            ITreatmentService treatmentService,
            IAppointmentService appointmentService,
            ITreatmentTypeService treatmentTypeService)
        {
            InitializeComponent();
            _treatmentService = treatmentService;
            _appointmentService = appointmentService;
            _treatmentTypeService = treatmentTypeService;
        }
    }
}