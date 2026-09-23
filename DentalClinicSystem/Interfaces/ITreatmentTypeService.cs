using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentTypeService
    {
        Task<IReadOnlyList<TreatmentType>> GetAllTreatmentTypesAsync();
        Task<ServiceResult> AddTreatmentTypeAsync(TreatmentType treatmentType);
        Task<ServiceResult> UpdateTreatmentTypeAsync(TreatmentType treatmentType);
        Task<ServiceResult> DeleteTreatmentTypeAsync(int treatmentTypeId);
    }
}
