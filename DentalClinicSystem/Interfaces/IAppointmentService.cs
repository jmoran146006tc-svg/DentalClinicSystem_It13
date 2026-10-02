using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IAppointmentService
    {
        Task<ServiceResult<IReadOnlyList<Appointment>>> GetAllAppointmentsAsync(User actor);
        Task<ServiceResult<AppointmentDetails>> GetDetailsAsync(User actor, int appointmentId);
        Task<ServiceResult> ScheduleAppointmentAsync(User actor, Appointment appointment);
        Task<bool> IsDentistAvailableAsync(int dentistId, DateTime when);
        Task<ServiceResult> UpdateAppointmentStatusAsync(User actor, int appointmentId, string status, string? cancellationReason = null);
        Task<ServiceResult> CancelAppointmentAsync(User actor, int appointmentId, string reason);
        Task<ServiceResult<IReadOnlyList<Appointment>>> GetAppointmentsForDentistOnDateAsync(User actor, int dentistId, DateTime date);
        Task<ServiceResult<IReadOnlyList<Appointment>>> GetAppointmentsInRangeAsync(User actor, DateTime from, DateTime to);
    }
}
