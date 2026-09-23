using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface IDentistRepository
    {
        Task<IReadOnlyList<Dentist>> GetAllAsync();
        Task<Dentist?> GetByIdAsync(int dentistId);
        Task AddAsync(Dentist dentist);
        Task UpdateAsync(Dentist dentist);
        Task DeleteAsync(int dentistId);
    }
}
