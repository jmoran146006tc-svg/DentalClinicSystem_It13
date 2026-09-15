using DentalClinicSystem.Forms;
using DentalClinicSystem.Repositories;
using DentalClinicSystem.Services;

namespace DentalClinicSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // ---- Composition root ---------------------------------------------
            // Every MySql*Repository is built exactly once, right here, and handed
            // to its matching service. Nothing past this point - no form, no user
            // control - ever constructs a repository or touches MySqlConnector.
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

            var loginForm = new frmLogin(
                authService, patientService, dentistService,
                appointmentService, treatmentService, treatmentTypeService);

            Application.Run(loginForm);
        }
    }
}