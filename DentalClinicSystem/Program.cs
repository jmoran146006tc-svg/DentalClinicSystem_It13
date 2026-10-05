using DentalClinicSystem.DBContent;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Service;
using DentalClinicSystem.Helpers.Design;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => ReportFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportFatal(e.ExceptionObject as Exception ?? new InvalidOperationException());
            TaskScheduler.UnobservedTaskException += (_, e) => { ReportFatal(e.Exception); e.SetObserved(); };
            ApplicationConfiguration.Initialize();
            AntdTheme.Initialize();
            Application.ApplicationExit += (_, _) => { MotionSystem.Animator.Dispose(); ShadowCache.Clear(); DesignPaint.ClearPaths(); };
#if DEBUG
            if (args.Contains("--style-guide", StringComparer.Ordinal))
            {
                Application.Run(new Forms.frmStyleGuide());
                return;
            }
#endif
            using var context = new ApplicationContext();
            using var startupOwner = new Control();
            Application.Idle += Start;
            Application.Run(context);

            async void Start(object? sender, EventArgs e) => await UiAction.RunAsync(startupOwner, StartAsync);

            async Task StartAsync()
            {
                Application.Idle -= Start;
                try
                {
                    if (!await DbConnectionHelper.CanConnectAsync())
                    {
                        UiMessages.ShowFatal("Could not connect to the database. Make sure MySQL is running and that you have run the scripts in the Database folder (see README), then check the connection settings in DBContent/DbConnectionHelper.cs.");
                        return;
                    }
                    var login = new frmLogin(BuildServices());
                    context.MainForm = login;
                    login.Show();
                }
                finally
                {
                    if (context.MainForm is null) context.ExitThread();
                }
            }
        }

        private static void ReportFatal(Exception error)
        {
            AppLog.Write(error);
            UiMessages.ShowFatal("An unexpected error occurred. Close and reopen the application. If it happens again, ask your administrator to check the diagnostic log.");
        }

        private static AppServices BuildServices()
        {
            IPatientRepository patientRepository = new MySqlPatientRepository();
            IDentistRepository dentistRepository = new MySqlDentistRepository();
            IAppointmentRepository appointmentRepository = new MySqlAppointmentRepository();
            IDentistTimeOffRepository timeOffRepository = new MySqlDentistTimeOffRepository();
            ITreatmentRepository treatmentRepository = new MySqlTreatmentRepository();
            ITreatmentTypeRepository treatmentTypeRepository = new MySqlTreatmentTypeRepository();
            IUserRepository userRepository = new MySqlUserRepository();

            var time = TimeProvider.System;
            IPatientService patientService = new PatientService(patientRepository, time);
            IDentistService dentistService = new DentistService(dentistRepository, appointmentRepository, timeOffRepository, time);
            IAppointmentService appointmentService = new AppointmentService(appointmentRepository, patientRepository, dentistRepository, timeOffRepository, time);
            ITreatmentService treatmentService = new TreatmentService(treatmentRepository, appointmentRepository, treatmentTypeRepository, time);
            ITreatmentTypeService treatmentTypeService = new TreatmentTypeService(treatmentTypeRepository);
            IAuthService authService = new AuthService(userRepository, dentistRepository);
            IUserService userService = new UserService(userRepository, dentistRepository);

            IReportRepository reportRepository = new MySqlReportRepository();
            IReportService reportService = new ReportService(reportRepository);
            IPatientHistoryService historyService = new PatientHistoryService(appointmentService,
                appointmentRepository, treatmentRepository, treatmentTypeRepository);
            return new AppServices(authService, patientService, dentistService,
                appointmentService, treatmentService, treatmentTypeService, userService, reportService, historyService);
        }
    }
}
