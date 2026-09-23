using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentRepository
    {
        Task<IReadOnlyList<Treatment>> GetAllAsync();
        Task<Treatment?> GetByIdAsync(int treatmentId);
        Task<IReadOnlyList<Treatment>> GetByAppointmentIdAsync(int appointmentId);
        Task AddAsync(Treatment treatment);
        Task UpdateAsync(Treatment treatment);
        Task DeleteAsync(int treatmentId);
    }
}
