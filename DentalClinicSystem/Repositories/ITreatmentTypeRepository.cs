using DentalClinicSystem.Models;

namespace DentalClinicSystem.Repositories
{
    public interface ITreatmentTypeRepository
    {
        Task<IReadOnlyList<TreatmentType>> GetAllAsync();
        Task<TreatmentType?> GetByIdAsync(int treatmentTypeId);
        Task AddAsync(TreatmentType treatmentType);
        Task UpdateAsync(TreatmentType treatmentType);
        Task DeleteAsync(int treatmentTypeId);
    }
}
