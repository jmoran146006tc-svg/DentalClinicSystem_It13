using DentalClinicSystem.Models;
using DentalClinicSystem.Repositories;

namespace DentalClinicSystem.Services
{
    public class DentistService : IDentistService
    {
        private readonly IDentistRepository _repository;

        public DentistService(IDentistRepository repository) => _repository = repository;

        public Task<IReadOnlyList<Dentist>> GetAllDentistsAsync() => _repository.GetAllAsync();

        public Task<Dentist?> GetDentistByIdAsync(int dentistId) => _repository.GetByIdAsync(dentistId);

        public async Task<ServiceResult> AddDentistAsync(Dentist dentist)
        {
            var validation = Validate(dentist);
            if (!validation.Success)
                return validation;

            await _repository.AddAsync(dentist);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateDentistAsync(Dentist dentist)
        {
            var validation = Validate(dentist);
            if (!validation.Success)
                return validation;

            await _repository.UpdateAsync(dentist);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteDentistAsync(int dentistId)
        {
            await _repository.DeleteAsync(dentistId);
            return ServiceResult.Ok();
        }

        private static ServiceResult Validate(Dentist dentist)
        {
            if (string.IsNullOrWhiteSpace(dentist.FirstName) || string.IsNullOrWhiteSpace(dentist.LastName))
                return ServiceResult.Fail("First and last name are required.");

            return ServiceResult.Ok();
        }
    }
}
