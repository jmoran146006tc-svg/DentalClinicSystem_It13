using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface IPatientRepository
    {
        Task<IReadOnlyList<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int patientId);
        Task AddAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task<IReadOnlyList<Patient>> GetAllIncludingInactiveAsync();
        Task<IReadOnlyList<Patient>> FindByNameAndDateOfBirthAsync(string first, string last, DateTime dob);
        Task ReactivateAsync(int patientId);
    }
}
