using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public partial class frmUserManagement : Form
    {
        public frmUserManagement(IUserService userService, IDentistService dentistService, User currentUser)
        {
            InitializeComponent();

            var userControl = new ucUserManagement(userService, dentistService, currentUser)
            {
                Dock = DockStyle.Fill
            };
            pnlHost.Controls.Add(userControl);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}