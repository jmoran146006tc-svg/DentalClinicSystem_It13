using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class PatientService(IPatientRepository repository, TimeProvider? timeProvider = null) : IPatientService
    {
        private readonly TimeProvider time = timeProvider ?? TimeProvider.System;
        public async Task<ServiceResult<IReadOnlyList<Patient>>> GetAllPatientsAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewPatients)) return RoleAccess.Denied<IReadOnlyList<Patient>>();
            return ServiceResult<IReadOnlyList<Patient>>.Ok(await repository.GetAllAsync());
        }
        public async Task<ServiceResult<IReadOnlyList<Patient>>> GetAllIncludingInactiveAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewPatients)) return RoleAccess.Denied<IReadOnlyList<Patient>>();
            return ServiceResult<IReadOnlyList<Patient>>.Ok(await repository.GetAllIncludingInactiveAsync());
        }
        public Task<ServiceResult> AddPatientAsync(User actor, Patient patient) => SaveAsync(actor, patient, false);
        public Task<ServiceResult> UpdatePatientAsync(User actor, Patient patient) => SaveAsync(actor, patient, true);

        private async Task<ServiceResult> SaveAsync(User actor, Patient patient, bool update)
        {
            if (!RoleAccess.Can(actor, Permission.ManagePatients)) return RoleAccess.Denied();
            var validation = Validator.Patient(patient, time.GetLocalNow().DateTime);
            if (!validation.Success) return validation;
            if (update && await repository.GetByIdAsync(patient.PatientId) is null) return ServiceResult.Fail("Patient not found.");
            var duplicates = (await repository.FindByNameAndDateOfBirthAsync(patient.FirstName, patient.LastName, patient.DateOfBirth))
                .Where(p => !update || p.PatientId != patient.PatientId).ToList();
            if (duplicates.Any(p => p.IsActive)) return ServiceResult.Fail("A patient with the same name and date of birth already exists.");
            if (duplicates.Count > 0) return ServiceResult.Fail("That patient exists but is inactive. Reactivate the existing record instead.");
            return await ServiceOperation.SaveAsync(() => update ? repository.UpdateAsync(patient) : repository.AddAsync(patient));
        }
        public async Task<ServiceResult> ReactivatePatientAsync(User actor, int patientId)
        {
            if (!RoleAccess.Can(actor, Permission.ManagePatients)) return RoleAccess.Denied();
            if (await repository.GetByIdAsync(patientId) is null) return ServiceResult.Fail("Patient not found.");
            return await ServiceOperation.SaveAsync(() => repository.ReactivateAsync(patientId));
        }
    }
}
