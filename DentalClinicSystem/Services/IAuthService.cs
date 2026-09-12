using DentalClinicSystem.Models;

namespace DentalClinicSystem.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<User>> LoginAsync(string username, string password);
    }
}
