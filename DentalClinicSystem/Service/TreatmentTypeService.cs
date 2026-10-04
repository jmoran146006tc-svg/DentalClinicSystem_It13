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
        public Task<ServiceResult> AddTreatmentTypeAsync(User actor, TreatmentType type) => SaveAsync(actor, type, false);
        public Task<ServiceResult> UpdateTreatmentTypeAsync(User actor, TreatmentType type) => SaveAsync(actor, type, true);
        private async Task<ServiceResult> SaveAsync(User actor, TreatmentType type, bool update)
        {
            if (!RoleAccess.Can(actor, Permission.ManageTreatments)) return RoleAccess.Denied();
            var validation = Validator.TreatmentType(type);
            if (!validation.Success) return validation;
            return await ServiceOperation.SaveAsync(() => update ? repository.UpdateAsync(type) : repository.AddAsync(type));
        }
        public Task<ServiceResult> DeleteTreatmentTypeAsync(User actor, int treatmentTypeId) =>
            !RoleAccess.Can(actor, Permission.ManageTreatments) ? Task.FromResult(RoleAccess.Denied())
                : ServiceOperation.SaveAsync(() => repository.DeleteAsync(treatmentTypeId));
    }
}
