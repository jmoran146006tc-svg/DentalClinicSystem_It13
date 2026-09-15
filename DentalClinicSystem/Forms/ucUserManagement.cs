
using DentalClinicSystem.Services;

namespace DentalClinicSystem.Forms
{
    public partial class ucUserManagement : UserControl
    {
        private readonly IUserService _userService;
        private readonly IDentistService _dentistService;

        public ucUserManagement(IUserService userService, IDentistService dentistService)
        {
            InitializeComponent();
            _userService = userService;
            _dentistService = dentistService;
        }
    }
}