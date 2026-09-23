using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentService
    {
        Task<IReadOnlyList<Treatment>> GetAllTreatmentsAsync();
        Task<IReadOnlyList<Treatment>> GetTreatmentsForAppointmentAsync(int appointmentId);
        Task<ServiceResult> AddTreatmentAsync(Treatment treatment);
    }
}
