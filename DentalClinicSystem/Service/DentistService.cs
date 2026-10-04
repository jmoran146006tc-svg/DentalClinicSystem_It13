using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class DentistService(IDentistRepository repository, TimeProvider? timeProvider = null) : IDentistService
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
        public async Task<ServiceResult> DeleteDentistAsync(User actor, int dentistId)
        {
            if (!RoleAccess.Can(actor, Permission.ManageDentists)) return RoleAccess.Denied();
            if (await repository.GetByIdAsync(dentistId) is null) return ServiceResult.Fail("Dentist not found.");
            return await ServiceOperation.SaveAsync(() => repository.DeleteAsync(dentistId));
        }
    }
}
