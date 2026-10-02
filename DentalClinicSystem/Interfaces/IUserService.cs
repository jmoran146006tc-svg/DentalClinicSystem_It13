using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<IReadOnlyList<User>>> GetAllUsersAsync(User actor);
        Task<ServiceResult> AddUserAsync(User actor, User user, string password);
        Task<ServiceResult> UpdateUserAsync(User actor, User user, string? newPassword);
        Task<ServiceResult> DeactivateUserAsync(User actor, int userId);
    }
}
