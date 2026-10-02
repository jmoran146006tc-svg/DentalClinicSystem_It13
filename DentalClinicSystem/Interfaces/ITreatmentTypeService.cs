using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentTypeService
    {
        Task<ServiceResult<IReadOnlyList<TreatmentType>>> GetAllTreatmentTypesAsync(User actor);
        Task<ServiceResult> AddTreatmentTypeAsync(User actor, TreatmentType treatmentType);
        Task<ServiceResult> UpdateTreatmentTypeAsync(User actor, TreatmentType treatmentType);
        Task<ServiceResult> DeleteTreatmentTypeAsync(User actor, int treatmentTypeId);
    }
}
