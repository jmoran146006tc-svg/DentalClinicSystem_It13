using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IDentistService
    {
        Task<ServiceResult<IReadOnlyList<Dentist>>> GetAllDentistsAsync(User actor);
        Task<ServiceResult<Dentist>> GetDentistByIdAsync(User actor, int dentistId);
        Task<ServiceResult> AddDentistAsync(User actor, Dentist dentist);
        Task<ServiceResult> UpdateDentistAsync(User actor, Dentist dentist);
        Task<ServiceResult> DeleteDentistAsync(User actor, int dentistId);
    }
}
