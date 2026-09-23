using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IDentistService
    {
        Task<IReadOnlyList<Dentist>> GetAllDentistsAsync();
        Task<Dentist?> GetDentistByIdAsync(int dentistId);
        Task<ServiceResult> AddDentistAsync(Dentist dentist);
        Task<ServiceResult> UpdateDentistAsync(Dentist dentist);
        Task<ServiceResult> DeleteDentistAsync(int dentistId);
    }
}
