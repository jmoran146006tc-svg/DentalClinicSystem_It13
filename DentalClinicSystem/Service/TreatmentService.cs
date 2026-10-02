using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class TreatmentService(ITreatmentRepository treatments, IAppointmentRepository appointments,
        ITreatmentTypeRepository types) : ITreatmentService
    {
        public async Task<ServiceResult<IReadOnlyList<Treatment>>> GetAllTreatmentsAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewTreatments)) return RoleAccess.Denied<IReadOnlyList<Treatment>>();
            return await VisibleAsync(actor, await treatments.GetAllAsync());
        }
        public async Task<ServiceResult<IReadOnlyList<Treatment>>> GetTreatmentsForAppointmentAsync(User actor, int appointmentId)
        {
            if (!RoleAccess.Can(actor, Permission.ViewTreatments)) return RoleAccess.Denied<IReadOnlyList<Treatment>>();
            var appointment = await appointments.GetByIdAsync(appointmentId);
            if (appointment is null) return ServiceResult<IReadOnlyList<Treatment>>.Fail("Appointment not found.");
            if (!RoleAccess.CanAccessAppointment(actor, appointment)) return RoleAccess.Denied<IReadOnlyList<Treatment>>();
            return ServiceResult<IReadOnlyList<Treatment>>.Ok(await treatments.GetByAppointmentIdAsync(appointmentId));
        }
        public async Task<ServiceResult<IReadOnlyList<Treatment>>> GetTreatmentsForPatientAsync(User actor, int patientId)
        {
            if (!RoleAccess.Can(actor, Permission.ViewTreatments)) return RoleAccess.Denied<IReadOnlyList<Treatment>>();
            return await VisibleAsync(actor, await treatments.GetByPatientIdAsync(patientId));
        }
        private async Task<ServiceResult<IReadOnlyList<Treatment>>> VisibleAsync(User actor, IReadOnlyList<Treatment> rows)
        {
            var permitted = (await appointments.GetAllAsync()).Where(a => RoleAccess.CanAccessAppointment(actor, a))
                .Select(a => a.AppointmentId).ToHashSet();
            return ServiceResult<IReadOnlyList<Treatment>>.Ok(rows.Where(t => permitted.Contains(t.AppointmentId)).ToList());
        }
        public Task<ServiceResult> AddTreatmentAsync(User actor, Treatment treatment) => SaveAsync(actor, treatment, false);
        public Task<ServiceResult> UpdateTreatmentAsync(User actor, Treatment treatment) => SaveAsync(actor, treatment, true);
        private async Task<ServiceResult> SaveAsync(User actor, Treatment treatment, bool update)
        {
            if (!RoleAccess.Can(actor, Permission.ManageTreatments)) return RoleAccess.Denied();
            var validation = Validator.Treatment(treatment);
            if (!validation.Success) return validation;
            var appointment = await appointments.GetByIdAsync(treatment.AppointmentId);
            if (appointment is null) return ServiceResult.Fail("Appointment not found.");
            if (!RoleAccess.CanAccessAppointment(actor, appointment)) return RoleAccess.Denied();
            if (await types.GetByIdAsync(treatment.TreatmentTypeId) is null) return ServiceResult.Fail("Treatment type not found.");
            if (update)
            {
                var existing = await treatments.GetByIdAsync(treatment.TreatmentId);
                if (existing is null) return ServiceResult.Fail("Treatment not found.");
                var original = await appointments.GetByIdAsync(existing.AppointmentId);
                if (original is null || !RoleAccess.CanAccessAppointment(actor, original)) return RoleAccess.Denied();
            }
            return await ServiceOperation.SaveAsync(() => update ? treatments.UpdateAsync(treatment) : treatments.AddAsync(treatment));
        }
    }
}
