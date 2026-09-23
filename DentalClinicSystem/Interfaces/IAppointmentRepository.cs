using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int appointmentId);
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task DeleteAsync(int appointmentId);
        Task<IReadOnlyList<Appointment>> GetByDentistAndDateAsync(int dentistId, DateTime date);
    }
}
