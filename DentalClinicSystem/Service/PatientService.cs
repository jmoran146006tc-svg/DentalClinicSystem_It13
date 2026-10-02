using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class PatientService(IPatientRepository repository) : IPatientService
    {
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
        public async Task<ServiceResult<Patient>> GetPatientByIdAsync(User actor, int patientId)
        {
            if (!RoleAccess.Can(actor, Permission.ViewPatients)) return RoleAccess.Denied<Patient>();
            var patient = await repository.GetByIdAsync(patientId);
            return patient is null ? ServiceResult<Patient>.Fail("Patient not found.") : ServiceResult<Patient>.Ok(patient);
        }
        public Task<ServiceResult> AddPatientAsync(User actor, Patient patient) => SaveAsync(actor, patient, false);
        public Task<ServiceResult> UpdatePatientAsync(User actor, Patient patient) => SaveAsync(actor, patient, true);

        private async Task<ServiceResult> SaveAsync(User actor, Patient patient, bool update)
        {
            if (!RoleAccess.Can(actor, Permission.ManagePatients)) return RoleAccess.Denied();
            var validation = Validator.Patient(patient);
            if (!validation.Success) return validation;
            if (update && await repository.GetByIdAsync(patient.PatientId) is null) return ServiceResult.Fail("Patient not found.");
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
