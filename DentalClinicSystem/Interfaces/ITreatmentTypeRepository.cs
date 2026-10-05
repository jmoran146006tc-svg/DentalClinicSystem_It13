using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentTypeRepository
    {
        Task<IReadOnlyList<TreatmentType>> GetAllAsync();
        Task<TreatmentType?> GetByIdAsync(int treatmentTypeId);
    }
}
