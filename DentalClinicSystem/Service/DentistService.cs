using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class DentistService(IDentistRepository repository, IAppointmentRepository appointments, IDentistTimeOffRepository timeOff, TimeProvider? timeProvider = null) : IDentistService
    {
        private readonly TimeProvider time = timeProvider ?? TimeProvider.System;
        private static bool CanRead(User actor) => RoleAccess.Can(actor, Permission.ViewAppointments) || RoleAccess.Can(actor, Permission.ViewDentists);
        public async Task<ServiceResult<IReadOnlyList<Dentist>>> GetAllDentistsAsync(User actor)
        {
            if (!CanRead(actor)) return RoleAccess.Denied<IReadOnlyList<Dentist>>();
            return ServiceResult<IReadOnlyList<Dentist>>.Ok(await repository.GetAllAsync());
        }
        public async Task<ServiceResult<Dentist>> GetDentistByIdAsync(User actor, int dentistId)
        {
            if (!CanRead(actor)) return RoleAccess.Denied<Dentist>();
            var dentist = await repository.GetByIdAsync(dentistId);
            return dentist is null ? ServiceResult<Dentist>.Fail("Dentist not found.") : ServiceResult<Dentist>.Ok(dentist);
        }
        public Task<ServiceResult> AddDentistAsync(User actor, Dentist dentist) => SaveAsync(actor, dentist, false);
        public Task<ServiceResult> UpdateDentistAsync(User actor, Dentist dentist) => SaveAsync(actor, dentist, true);
        private async Task<ServiceResult> SaveAsync(User actor, Dentist dentist, bool update)
        {
            if (!RoleAccess.Can(actor, Permission.ManageDentists)) return RoleAccess.Denied();
            var validation = Validator.Dentist(dentist);
            if (!validation.Success) return validation;
            if (update && await repository.GetByIdAsync(dentist.DentistId) is null) return ServiceResult.Fail("Dentist not found.");
            return await ServiceOperation.SaveAsync(() => update ? repository.UpdateAsync(dentist) : repository.AddAsync(dentist));
        }
        public async Task<ServiceResult<IReadOnlyList<DentistTimeOff>>> GetTimeOffAsync(User actor, int dentistId)
        {
            if (!RoleAccess.Can(actor, Permission.ManageDentists)) return RoleAccess.Denied<IReadOnlyList<DentistTimeOff>>();
            return ServiceResult<IReadOnlyList<DentistTimeOff>>.Ok(await timeOff.GetByDentistAsync(dentistId));
        }
        public async Task<ServiceResult> AddTimeOffAsync(User actor, DentistTimeOff leave)
        {
            if (!RoleAccess.Can(actor, Permission.ManageDentists)) return RoleAccess.Denied();
            var validation = Validator.TimeOff(leave);
            if (!validation.Success) return validation;
            var dentist = await repository.GetByIdAsync(leave.DentistId);
            if (dentist is not { IsActive: true }) return ServiceResult.Fail("Select an active dentist.");
            var rows = await appointments.GetByDentistAndRangeAsync(leave.DentistId, leave.StartDate, leave.EndDate.AddDays(1));
            var count = rows.Count(a => a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn);
            if (count > 0) return ServiceResult.Fail($"{dentist.FullName} has {count} appointment(s) in that range. Reschedule or cancel them first.");
            return await ServiceOperation.SaveAsync(() => timeOff.AddAsync(leave));
        }
        public async Task<ServiceResult> RemoveTimeOffAsync(User actor, int dentistId, int timeOffId)
        {
            if (!RoleAccess.Can(actor, Permission.ManageDentists)) return RoleAccess.Denied();
            if (!(await timeOff.GetByDentistAsync(dentistId)).Any(t => t.TimeOffId == timeOffId)) return ServiceResult.Fail("Time off not found.");
            return await ServiceOperation.SaveAsync(() => timeOff.DeleteAsync(timeOffId));
        }
        public async Task<ServiceResult> DeleteDentistAsync(User actor, int dentistId)
        {
            if (!RoleAccess.Can(actor, Permission.ManageDentists)) return RoleAccess.Denied();
            var dentist = await repository.GetByIdAsync(dentistId);
            if (dentist is null) return ServiceResult.Fail("Dentist not found.");
            var count = await appointments.CountUpcomingByDentistAsync(dentistId, time.GetLocalNow().DateTime);
            if (count > 0) return ServiceResult.Fail($"{dentist.FullName} still has {count} upcoming appointment(s). Reschedule or cancel them first.");
            return await ServiceOperation.SaveAsync(() => repository.DeleteAsync(dentistId));
        }
    }
}
