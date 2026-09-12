using DentalClinicSystem.Models;

namespace DentalClinicSystem.Services
{
    public interface IAppointmentService
    {
        Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);

        Task<ServiceResult> ScheduleAppointmentAsync(Appointment appointment);

        Task<ServiceResult> UpdateAppointmentStatusAsync(int appointmentId, string status);
    }
}
