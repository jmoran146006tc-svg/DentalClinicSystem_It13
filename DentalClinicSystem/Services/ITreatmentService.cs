using DentalClinicSystem.Models;

namespace DentalClinicSystem.Services
{
    public interface ITreatmentService
    {
        Task<IReadOnlyList<Treatment>> GetAllTreatmentsAsync();
        Task<IReadOnlyList<Treatment>> GetTreatmentsForAppointmentAsync(int appointmentId);
        Task<ServiceResult> AddTreatmentAsync(Treatment treatment);
    }
}
