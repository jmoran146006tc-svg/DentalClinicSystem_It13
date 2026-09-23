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

            try
            {
                // Creates the database, all six tables, and every stored procedure if
                // they don't already exist yet, then seeds starter data (dentists,
                // treatment types, a demo patient, and one admin login) only if those
                // tables are still empty. See DBContent/DatabaseInitializer.cs.
                await DatabaseInitializer.EnsureDatabaseReadyAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not set up the database automatically:\n\n{ex.Message}\n\n" +
                    "Check DBContent/DbConnectionHelper.cs - the Server/UserId/Password " +
                    "constants there need to match a MySQL server you can actually reach.",
                    "Database Setup Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            var loginForm = new frmLogin(
                authService, patientService, dentistService,
                appointmentService, treatmentService, treatmentTypeService, userService);

            Application.Run(loginForm);
        }
    }
}
