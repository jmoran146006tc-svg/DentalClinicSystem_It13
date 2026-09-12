using DentalClinicSystem.Models;
using DentalClinicSystem.Repositories;

namespace DentalClinicSystem.Services
{
    public class AppointmentService : IAppointmentService
    {
        private static readonly string[] ValidStatuses = ["Scheduled", "Completed", "Cancelled", "NoShow"];

        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
            => _appointmentRepository = appointmentRepository;

        public Task<IReadOnlyList<Appointment>> GetAllAppointmentsAsync() => _appointmentRepository.GetAllAsync();

        public Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
            => _appointmentRepository.GetByIdAsync(appointmentId);

        public async Task<ServiceResult> ScheduleAppointmentAsync(Appointment appointment)
        {
            if (appointment.AppointmentDateTime < DateTime.Now)
                return ServiceResult.Fail("Appointment time cannot be in the past.");

            var sameDayAppointments = await _appointmentRepository.GetByDentistAndDateAsync(
                appointment.DentistId, appointment.AppointmentDateTime.Date);

            bool hasConflict = sameDayAppointments.Any(existing =>
                existing.AppointmentId != appointment.AppointmentId &&
                Math.Abs((existing.AppointmentDateTime - appointment.AppointmentDateTime).TotalMinutes) < 30);

            if (hasConflict)
                return ServiceResult.Fail("This dentist already has an appointment within 30 minutes of the selected time.");

            await _appointmentRepository.AddAsync(appointment);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateAppointmentStatusAsync(int appointmentId, string status)
        {
            if (!ValidStatuses.Contains(status))
                return ServiceResult.Fail($"Status must be one of: {string.Join(", ", ValidStatuses)}.");

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
            if (appointment is null)
                return ServiceResult.Fail("Appointment not found.");

            appointment.Status = status;
            await _appointmentRepository.UpdateAsync(appointment);
            return ServiceResult.Ok();
        }
    }
}
