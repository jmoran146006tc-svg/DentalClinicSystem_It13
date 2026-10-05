using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class TreatmentTypeService(ITreatmentTypeRepository repository) : ITreatmentTypeService
    {
        public async Task<ServiceResult<IReadOnlyList<TreatmentType>>> GetAllTreatmentTypesAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewTreatments)) return RoleAccess.Denied<IReadOnlyList<TreatmentType>>();
            return ServiceResult<IReadOnlyList<TreatmentType>>.Ok(await repository.GetAllAsync());
        }
        public async Task<ServiceResult<IReadOnlyList<VisitReason>>> GetVisitReasonsAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewAppointments) && !RoleAccess.Can(actor, Permission.ViewTreatments))
                return RoleAccess.Denied<IReadOnlyList<VisitReason>>();
            var types = await repository.GetAllAsync();
            return ServiceResult<IReadOnlyList<VisitReason>>.Ok([new(ClinicRules.ConsultationReason, ClinicRules.DefaultDurationMinutes),
                .. types.Where(t => !t.Name.Equals(ClinicRules.ConsultationReason, StringComparison.OrdinalIgnoreCase)).Select(t => new VisitReason(t.Name, t.DefaultDurationMinutes))]);
        }
    }
}
