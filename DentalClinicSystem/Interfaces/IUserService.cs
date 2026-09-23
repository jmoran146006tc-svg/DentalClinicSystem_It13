using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IUserService
    {
        Task<IReadOnlyList<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<ServiceResult> AddUserAsync(User user, string password);
        Task<ServiceResult> UpdateUserAsync(User user, string? newPassword);
        Task<ServiceResult> DeactivateUserAsync(int userId);
    }
}
