using DentalClinicSystem.Models;
using DentalClinicSystem.Repositories;

namespace DentalClinicSystem.Services
{
    public class TreatmentService : ITreatmentService
    {
        private readonly ITreatmentRepository _repository;

        public TreatmentService(ITreatmentRepository repository) => _repository = repository;

        public Task<IReadOnlyList<Treatment>> GetAllTreatmentsAsync() => _repository.GetAllAsync();

        public Task<IReadOnlyList<Treatment>> GetTreatmentsForAppointmentAsync(int appointmentId)
            => _repository.GetByAppointmentIdAsync(appointmentId);

        public async Task<ServiceResult> AddTreatmentAsync(Treatment treatment)
        {
            if (treatment.Cost < 0)
                return ServiceResult.Fail("Cost cannot be negative.");

            if (treatment.DatePerformed > DateTime.Today)
                return ServiceResult.Fail("Date performed cannot be in the future.");

            await _repository.AddAsync(treatment);
            return ServiceResult.Ok();
        }
    }
}
