using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface ITreatmentTypeService
    {
        Task<ServiceResult<IReadOnlyList<TreatmentType>>> GetAllTreatmentTypesAsync(User actor);
        Task<ServiceResult<IReadOnlyList<VisitReason>>> GetVisitReasonsAsync(User actor);
    }
}
