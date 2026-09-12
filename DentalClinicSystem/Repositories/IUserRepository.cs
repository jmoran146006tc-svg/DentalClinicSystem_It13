using DentalClinicSystem.Models;

namespace DentalClinicSystem.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);

        Task<User?> GetByUsernameAsync(string username);

        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
