using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class AppointmentService(IAppointmentRepository appointments, IPatientRepository patients,
        IDentistRepository dentists) : IAppointmentService
    {
        public const int ConflictMinutes = 30;
        public async Task<ServiceResult<IReadOnlyList<Appointment>>> GetAllAppointmentsAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewAppointments)) return RoleAccess.Denied<IReadOnlyList<Appointment>>();
            return ServiceResult<IReadOnlyList<Appointment>>.Ok((await appointments.GetAllAsync())
                .Where(a => RoleAccess.CanAccessAppointment(actor, a)).ToList());
        }
        public async Task<ServiceResult<AppointmentDetails>> GetDetailsAsync(User actor, int appointmentId)
        {
            if (!RoleAccess.Can(actor, Permission.ViewAppointments)) return RoleAccess.Denied<AppointmentDetails>();
            var appointment = await appointments.GetByIdAsync(appointmentId);
            if (appointment is null) return ServiceResult<AppointmentDetails>.Fail("Appointment not found.");
            if (!RoleAccess.CanAccessAppointment(actor, appointment)) return RoleAccess.Denied<AppointmentDetails>();
            var patient = await patients.GetByIdAsync(appointment.PatientId);
            var dentist = await dentists.GetByIdAsync(appointment.DentistId);
            return patient is null || dentist is null
                ? ServiceResult<AppointmentDetails>.Fail("The linked patient or dentist could not be found.")
                : ServiceResult<AppointmentDetails>.Ok(new(appointment, patient, dentist));
        }
        public async Task<ServiceResult> ScheduleAppointmentAsync(User actor, Appointment appointment)
        {
            if (!RoleAccess.Can(actor, Permission.ManageAppointments)) return RoleAccess.Denied();
            var validation = Validator.Appointment(appointment);
            if (!validation.Success) return validation;
            if (await patients.GetByIdAsync(appointment.PatientId) is not { IsActive: true })
                return ServiceResult.Fail("Select an active patient.");
            if (await dentists.GetByIdAsync(appointment.DentistId) is not { IsActive: true })
                return ServiceResult.Fail("Select an active dentist.");
            if (!await IsDentistAvailableAsync(appointment.DentistId, appointment.AppointmentDateTime))
                return ServiceResult.Fail("This dentist already has an appointment within 30 minutes of the selected time.");
            appointment.Status = AppointmentStatus.Scheduled;
            appointment.CancellationReason = null;
            return await ServiceOperation.SaveAsync(() => appointments.AddAsync(appointment));
        }
        public async Task<bool> IsDentistAvailableAsync(int dentistId, DateTime when)
        {
            if (await dentists.GetByIdAsync(dentistId) is not { IsActive: true }) return false;
            var nearby = await appointments.GetByDentistAndRangeAsync(dentistId,
                when.AddMinutes(-ConflictMinutes), when.AddMinutes(ConflictMinutes));
            return !nearby.Any(a => a.Status != AppointmentStatus.Cancelled &&
                Math.Abs((a.AppointmentDateTime - when).TotalMinutes) < ConflictMinutes);
        }
        public async Task<ServiceResult> UpdateAppointmentStatusAsync(User actor, int appointmentId, string status, string? cancellationReason = null)
        {
            var appointment = await appointments.GetByIdAsync(appointmentId);
            if (appointment is null) return ServiceResult.Fail("Appointment not found.");
            if (!RoleAccess.CanChangeStatus(actor, appointment, status)) return RoleAccess.Denied();
            var validation = Validator.StatusChange(appointment.Status, status, cancellationReason);
            if (!validation.Success) return validation;
            appointment.Status = status;
            appointment.CancellationReason = Validator.Optional(cancellationReason);
            return await ServiceOperation.SaveAsync(() => appointments.UpdateAsync(appointment));
        }
        public Task<ServiceResult> CancelAppointmentAsync(User actor, int appointmentId, string reason) =>
            UpdateAppointmentStatusAsync(actor, appointmentId,
                reason.Trim() == "No Show" ? AppointmentStatus.NoShow : AppointmentStatus.Cancelled, reason);

        public async Task<ServiceResult<IReadOnlyList<Appointment>>> GetAppointmentsForDentistOnDateAsync(User actor, int dentistId, DateTime date)
        {
            if (!RoleAccess.Can(actor, Permission.ViewAppointments) ||
                (RoleAccess.IsDentist(actor) && actor.DentistId != dentistId)) return RoleAccess.Denied<IReadOnlyList<Appointment>>();
            return ServiceResult<IReadOnlyList<Appointment>>.Ok(
                await appointments.GetByDentistAndRangeAsync(dentistId, date.Date, date.Date.AddDays(1)));
        }
        public async Task<ServiceResult<IReadOnlyList<Appointment>>> GetAppointmentsInRangeAsync(User actor, DateTime from, DateTime to)
        {
            if (!RoleAccess.Can(actor, Permission.ViewAppointments)) return RoleAccess.Denied<IReadOnlyList<Appointment>>();
            var validation = Validator.DateRange(from, to);
            if (!validation.Success || from > to) return ServiceResult<IReadOnlyList<Appointment>>.Fail("From must be on or before To.");
            var rows = await appointments.GetByRangeAsync(from, to);
            return ServiceResult<IReadOnlyList<Appointment>>.Ok(rows.Where(a => RoleAccess.CanAccessAppointment(actor, a)).ToList());
        }
    }
}
