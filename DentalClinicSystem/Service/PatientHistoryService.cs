using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class PatientHistoryService(IAppointmentService appointmentService, IAppointmentRepository appointments,
        ITreatmentRepository treatments, ITreatmentTypeRepository types) : IPatientHistoryService
    {
        public async Task<ServiceResult<PatientHistory>> GetHistoryAsync(User actor, int appointmentId)
        {
            if (!RoleAccess.Can(actor, Permission.ViewTreatments)) return RoleAccess.Denied<PatientHistory>();
            var details = await appointmentService.GetDetailsAsync(actor, appointmentId);
            if (!details.Success || details.Data is null) return ServiceResult<PatientHistory>.Fail(details.ErrorMessage);
            var patientId = details.Data.Patient.PatientId;
            return ServiceResult<PatientHistory>.Ok(new(details.Data,
                await appointments.GetByPatientIdAsync(patientId), await treatments.GetByPatientIdAsync(patientId),
                await types.GetAllAsync()));
        }
    }
}
