using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentService
    {
        Task<ServiceResult<IReadOnlyList<Treatment>>> GetAllTreatmentsAsync(User actor);
        Task<ServiceResult<IReadOnlyList<Treatment>>> GetTreatmentsForAppointmentAsync(User actor, int appointmentId);
        Task<ServiceResult<IReadOnlyList<Treatment>>> GetTreatmentsForPatientAsync(User actor, int patientId);
        Task<ServiceResult> AddTreatmentAsync(User actor, Treatment treatment);
        Task<ServiceResult> UpdateTreatmentAsync(User actor, Treatment treatment);
    }
}
