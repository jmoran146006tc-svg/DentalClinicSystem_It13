using DentalClinicSystem.DBContent;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Service;

namespace DentalClinicSystem
{
    internal static class Program
    {
        [STAThread]
        static async Task Main()
        {
            ApplicationConfiguration.Initialize();

            if (!await DbConnectionHelper.CanConnectAsync())
            {
                MessageBox.Show("Could not connect to the database. Make sure MySQL is running and that you have run the scripts in the Database folder (see README), then check the connection settings in DBContent/DbConnectionHelper.cs.",
                    "Database connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ---- Composition root ---------------------------------------------
            IPatientRepository patientRepository = new MySqlPatientRepository();
            IDentistRepository dentistRepository = new MySqlDentistRepository();
            IAppointmentRepository appointmentRepository = new MySqlAppointmentRepository();
            ITreatmentRepository treatmentRepository = new MySqlTreatmentRepository();
            ITreatmentTypeRepository treatmentTypeRepository = new MySqlTreatmentTypeRepository();
            IUserRepository userRepository = new MySqlUserRepository();

            IPatientService patientService = new PatientService(patientRepository);
            IDentistService dentistService = new DentistService(dentistRepository);
            IAppointmentService appointmentService = new AppointmentService(appointmentRepository);
            ITreatmentService treatmentService = new TreatmentService(treatmentRepository);
            ITreatmentTypeService treatmentTypeService = new TreatmentTypeService(treatmentTypeRepository);
            IAuthService authService = new AuthService(userRepository);
            IUserService userService = new UserService(userRepository);
            // ---------------------------------------------------------------------

            var services = new AppServices(authService, patientService, dentistService,
                appointmentService, treatmentService, treatmentTypeService, userService);
            var loginForm = new frmLogin(services);

            Application.Run(loginForm);
        }
    }
}
