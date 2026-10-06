using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class AppointmentService(IAppointmentRepository appointments, IPatientRepository patients,
        IDentistRepository dentists, IDentistTimeOffRepository timeOff, TimeProvider? timeProvider = null) : IAppointmentService
    {
        private readonly TimeProvider time = timeProvider ?? TimeProvider.System;
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
            var validation = Validator.Appointment(appointment, time.GetLocalNow().DateTime);
            if (!validation.Success) return validation;
            var patient = await patients.GetByIdAsync(appointment.PatientId);
            if (patient is null)
                return ServiceResult.Fail("Select an active patient.");
            var slot = await ValidateSlotAsync(appointment.DentistId, appointment.AppointmentDateTime, appointment.DurationMinutes);
            if (!slot.Success) return slot;
            appointment.Status = AppointmentStatus.Scheduled;
            appointment.CancellationReason = null;
            return await ServiceOperation.SaveAsync(() => appointments.AddAsync(appointment));
        }
        public async Task<ServiceResult> RescheduleAppointmentAsync(User actor, int appointmentId, DateTime newDateTime, int newDentistId)
        {
            if (!RoleAccess.Can(actor, Permission.ManageAppointments)) return RoleAccess.Denied();
            var current = await appointments.GetByIdAsync(appointmentId);
            if (current is null) return ServiceResult.Fail("Appointment not found.");
            if (current.Status != AppointmentStatus.Scheduled) return ServiceResult.Fail("Only scheduled appointments can be rescheduled.");
            var changed = new Appointment
            {
                AppointmentId = current.AppointmentId, PatientId = current.PatientId, DentistId = newDentistId,
                AppointmentDateTime = newDateTime, Status = current.Status, Reason = current.Reason,
                DurationMinutes = current.DurationMinutes, Notes = current.Notes, CancellationReason = current.CancellationReason, CreatedAt = current.CreatedAt
            };
            var validation = Validator.Appointment(changed, time.GetLocalNow().DateTime);
            if (!validation.Success) return validation;
            var slot = await ValidateSlotAsync(newDentistId, newDateTime, changed.DurationMinutes, appointmentId);
            if (!slot.Success) return slot;
            return await ServiceOperation.SaveAsync(() => appointments.UpdateAsync(changed));
        }
        public async Task<ServiceResult> ValidateSlotAsync(int dentistId, DateTime start, int durationMinutes, int? excludeAppointmentId = null)
        {
            var duration = Validator.Duration(durationMinutes);
            if (!duration.Success) return duration;
            if (!ClinicRules.IsOpen(start, durationMinutes))
                return ServiceResult.Fail($"The clinic is closed at that time ({ClinicRules.HoursDescription}).");
            var dentist = await dentists.GetByIdAsync(dentistId);
            if (dentist is not { IsActive: true }) return ServiceResult.Fail("Select an active dentist.");
            if ((await timeOff.GetOverlappingAsync(dentistId, start.Date, start.Date)).Count > 0)
                return ServiceResult.Fail(dentist.FullName + ClinicRules.TimeOffSuffix);
            var end = start.AddMinutes(durationMinutes);
            var nearby = await appointments.GetByDentistAndRangeAsync(dentistId, start.AddMinutes(-ClinicRules.MaxDurationMinutes), end);
            var conflict = nearby.Where(a => a.AppointmentId != excludeAppointmentId && a.Status != AppointmentStatus.Cancelled &&
                a.AppointmentDateTime < end && a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > start)
                .OrderBy(a => a.AppointmentDateTime).FirstOrDefault();
            return conflict is null ? ServiceResult.Ok() : ServiceResult.Fail(
                $"This dentist is already booked from {conflict.AppointmentDateTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture)} to {conflict.AppointmentDateTime.AddMinutes(conflict.DurationMinutes).ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture)}.");
        }
        public async Task<bool> IsDentistAvailableAsync(int dentistId, DateTime when, int durationMinutes, int? excludeAppointmentId = null) =>
            (await ValidateSlotAsync(dentistId, when, durationMinutes, excludeAppointmentId)).Success;
        public async Task<ServiceResult> UpdateAppointmentStatusAsync(User actor, int appointmentId, string status, string? cancellationReason = null)
        {
            var appointment = await appointments.GetByIdAsync(appointmentId);
            if (appointment is null) return ServiceResult.Fail("Appointment not found.");
            if (!RoleAccess.CanChangeStatus(actor, appointment, status)) return RoleAccess.Denied();
            var validation = Validator.StatusChange(appointment.Status, status, cancellationReason, appointment.AppointmentDateTime, time.GetLocalNow().DateTime);
            if (!validation.Success) return validation;
            appointment.Status = status;
            appointment.CancellationReason = Validator.Optional(cancellationReason);
            return await ServiceOperation.SaveAsync(() => appointments.UpdateAsync(appointment));
        }
        public Task<ServiceResult> CancelAppointmentAsync(User actor, int appointmentId, bool isNoShow, string reason) =>
            UpdateAppointmentStatusAsync(actor, appointmentId,
                isNoShow ? AppointmentStatus.NoShow : AppointmentStatus.Cancelled, reason);

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
