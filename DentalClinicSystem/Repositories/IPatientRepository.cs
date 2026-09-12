using DentalClinicSystem.Models;

namespace DentalClinicSystem.Repositories
{
    public interface IPatientRepository
    {
        Task<IReadOnlyList<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int patientId);
        Task AddAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task DeleteAsync(int patientId);
    }
}
