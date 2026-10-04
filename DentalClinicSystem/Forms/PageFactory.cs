using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms
{
    public static class PageFactory
    {
        public static Control CreateDashboard(AppServices services, User currentUser) =>
            new ucDashboardHome(services.Appointments, services.Patients, services.Dentists, services.Reports, services.PatientHistory, currentUser);

        public static Control CreatePatientHistory(IPatientHistoryService history, User currentUser, int appointmentId, DateTime now) =>
            new ucPatientHistory(history, currentUser, appointmentId, now);

        public static Control CreatePatients(IPatientService patients, User currentUser) =>
            new ucPatientRecords(patients, currentUser);

        public static Control CreateDentists(IDentistService dentists, User currentUser) =>
            new ucDentistRecords(dentists, currentUser);

        public static Control CreateAppointments(IAppointmentService appointments, IPatientService patients,
            IDentistService dentists, ITreatmentTypeService treatmentTypes, User currentUser) =>
            new ucAppointmentScheduler(appointments, patients, dentists, treatmentTypes, currentUser);

        public static Control CreateTreatments(ITreatmentService treatments, IAppointmentService appointments,
            ITreatmentTypeService treatmentTypes, User currentUser) =>
            new ucTreatmentRecords(treatments, appointments, treatmentTypes, currentUser);

        public static Control CreateUsers(IUserService users, IDentistService dentists, User currentUser) =>
            new ucUserManagement(users, dentists, currentUser);

        public static Control CreateReports(IReportService reports, User currentUser) =>
            new ucReports(reports, currentUser);
    }
}
