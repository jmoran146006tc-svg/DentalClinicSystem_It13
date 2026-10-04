using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IAppointmentService
    {
        Task<ServiceResult<IReadOnlyList<Appointment>>> GetAllAppointmentsAsync(User actor);
        Task<ServiceResult<AppointmentDetails>> GetDetailsAsync(User actor, int appointmentId);
        Task<ServiceResult> ScheduleAppointmentAsync(User actor, Appointment appointment);
        Task<bool> IsDentistAvailableAsync(int dentistId, DateTime when, int durationMinutes, int? excludeAppointmentId = null);
        Task<ServiceResult> ValidateSlotAsync(int dentistId, DateTime start, int durationMinutes, int? excludeAppointmentId = null);
        Task<ServiceResult> RescheduleAppointmentAsync(User actor, int appointmentId, DateTime newDateTime, int newDentistId);
        Task<ServiceResult> UpdateAppointmentStatusAsync(User actor, int appointmentId, string status, string? cancellationReason = null);
        Task<ServiceResult> CancelAppointmentAsync(User actor, int appointmentId, bool isNoShow, string reason);
        Task<ServiceResult<IReadOnlyList<Appointment>>> GetAppointmentsForDentistOnDateAsync(User actor, int dentistId, DateTime date);
        Task<ServiceResult<IReadOnlyList<Appointment>>> GetAppointmentsInRangeAsync(User actor, DateTime from, DateTime to);
    }
}
