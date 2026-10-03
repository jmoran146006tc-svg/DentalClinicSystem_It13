using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms
{
    public static class PageFactory
    {
        public static Control CreateDashboard(User currentUser) => new ucDashboardHome(currentUser);

        public static Control CreatePatients(IPatientService patients, User currentUser) =>
            new ucPatientRecords(patients, currentUser);

        public static Control CreateDentists(IDentistService dentists, User currentUser) =>
            new ucDentistRecords(dentists, currentUser);

        public static Control CreateAppointments(IAppointmentService appointments, IPatientService patients,
            IDentistService dentists, User currentUser) =>
            new ucAppointmentScheduler(appointments, patients, dentists, currentUser);

        public static Control CreateTreatments(ITreatmentService treatments, IAppointmentService appointments,
            ITreatmentTypeService treatmentTypes, User currentUser) =>
            new ucTreatmentRecords(treatments, appointments, treatmentTypes, currentUser);

        public static Control CreateUsers(IUserService users, IDentistService dentists, User currentUser) =>
            new ucUserManagement(users, dentists, currentUser);

        public static Control CreateReports(IReportService reports, User currentUser) =>
            new ucReports(reports, currentUser);
    }
}
