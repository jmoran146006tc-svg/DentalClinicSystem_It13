using DentalClinicSystem.Models;
using DentalClinicSystem.Repositories;

namespace DentalClinicSystem.Services
{
    public class TreatmentTypeService : ITreatmentTypeService
    {
        private readonly ITreatmentTypeRepository _repository;

        public TreatmentTypeService(ITreatmentTypeRepository repository) => _repository = repository;

        public Task<IReadOnlyList<TreatmentType>> GetAllTreatmentTypesAsync() => _repository.GetAllAsync();

        public async Task<ServiceResult> AddTreatmentTypeAsync(TreatmentType treatmentType)
        {
            var validation = Validate(treatmentType);
            if (!validation.Success)
                return validation;

            await _repository.AddAsync(treatmentType);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateTreatmentTypeAsync(TreatmentType treatmentType)
        {
            var validation = Validate(treatmentType);
            if (!validation.Success)
                return validation;

            await _repository.UpdateAsync(treatmentType);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteTreatmentTypeAsync(int treatmentTypeId)
        {
            try
            {
                await _repository.DeleteAsync(treatmentTypeId);
                return ServiceResult.Ok();
            }
            catch (RepositoryConstraintException ex)
            {
                return ServiceResult.Fail(ex.Message);
            }
        }

        private static ServiceResult Validate(TreatmentType treatmentType)
        {
            if (string.IsNullOrWhiteSpace(treatmentType.Name))
                return ServiceResult.Fail("Treatment type name is required.");

            if (treatmentType.DefaultCost < 0)
                return ServiceResult.Fail("Default cost cannot be negative.");

            return ServiceResult.Ok();
        }
    }
}
