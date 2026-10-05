using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<Appointment>> GetAllAsync();
        Task<Appointment?> GetByIdAsync(int appointmentId);
        Task AddAsync(Appointment appointment);
        Task UpdateAsync(Appointment appointment);
        Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(int patientId);
        Task<IReadOnlyList<Appointment>> GetByRangeAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<Appointment>> GetByDentistAndRangeAsync(int dentistId, DateTime from, DateTime to);
        Task<int> CountUpcomingByDentistAsync(int dentistId, DateTime from);
    }
}
