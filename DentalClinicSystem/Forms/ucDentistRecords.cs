using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucDentistRecords : UserControl
    {
        private readonly IDentistService _dentistService;

        public ucDentistRecords(IDentistService dentistService)
        {
            InitializeComponent();
            _dentistService = dentistService;
        }

        private void ucDentistRecords_Load(object sender, EventArgs e)
        {

        }
    }
}