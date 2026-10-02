using DentalClinicSystem.Interfaces;

namespace DentalClinicSystem.Service
{
    public sealed record AppServices(
        IAuthService Auth,
        IPatientService Patients,
        IDentistService Dentists,
        IAppointmentService Appointments,
        ITreatmentService Treatments,
        ITreatmentTypeService TreatmentTypes,
        IUserService Users,
        IReportService Reports,
        IPatientHistoryService PatientHistory);
}
