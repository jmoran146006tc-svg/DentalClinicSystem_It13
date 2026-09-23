using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<User>> LoginAsync(string username, string password);
    }
}
