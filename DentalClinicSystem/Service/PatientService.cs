using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository) => _repository = repository;

        public Task<IReadOnlyList<Patient>> GetAllPatientsAsync() => _repository.GetAllAsync();

        public Task<Patient?> GetPatientByIdAsync(int patientId) => _repository.GetByIdAsync(patientId);

        public async Task<ServiceResult> AddPatientAsync(Patient patient)
        {
            var validation = Validate(patient);
            if (!validation.Success)
                return validation;

            await _repository.AddAsync(patient);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdatePatientAsync(Patient patient)
        {
            var validation = Validate(patient);
            if (!validation.Success)
                return validation;

            await _repository.UpdateAsync(patient);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeletePatientAsync(int patientId)
        {
            await _repository.DeleteAsync(patientId);
            return ServiceResult.Ok();
        }

        // Shared by AddPatientAsync and UpdatePatientAsync so these rules live in one place.
        private static ServiceResult Validate(Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patient.FirstName) || string.IsNullOrWhiteSpace(patient.LastName))
                return ServiceResult.Fail("First and last name are required.");

            if (string.IsNullOrWhiteSpace(patient.ContactNumber))
                return ServiceResult.Fail("Contact number is required.");

            if (patient.DateOfBirth > DateTime.Today)
                return ServiceResult.Fail("Date of birth cannot be in the future.");

            return ServiceResult.Ok();
        }
    }
}
