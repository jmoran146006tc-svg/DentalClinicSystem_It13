using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class UserService : IUserService
    {
        private static readonly string[] ValidRoles = ["Admin", "Receptionist", "Dentist"];

        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository) => _repository = repository;

        public Task<IReadOnlyList<User>> GetAllUsersAsync() => _repository.GetAllAsync();

        public Task<User?> GetUserByIdAsync(int userId) => _repository.GetByIdAsync(userId);

        public async Task<ServiceResult> AddUserAsync(User user, string password)
        {
            var validation = Validate(user, password, isNewUser: true);
            if (!validation.Success)
                return validation;

            var existing = await _repository.GetByUsernameAsync(user.Username);
            if (existing is not null)
                return ServiceResult.Fail("That username is already taken.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _repository.AddAsync(user);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> UpdateUserAsync(User user, string? newPassword)
        {
            var validation = Validate(user, newPassword, isNewUser: false);
            if (!validation.Success)
                return validation;

            var existing = await _repository.GetByIdAsync(user.UserId);
            if (existing is null)
                return ServiceResult.Fail("User not found.");

            if (!string.Equals(existing.Username, user.Username, StringComparison.OrdinalIgnoreCase))
            {
                var usernameOwner = await _repository.GetByUsernameAsync(user.Username);
                if (usernameOwner is not null && usernameOwner.UserId != user.UserId)
                    return ServiceResult.Fail("That username is already taken.");
            }

            user.PasswordHash = string.IsNullOrWhiteSpace(newPassword)
                ? existing.PasswordHash
                : BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _repository.UpdateAsync(user);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeactivateUserAsync(int userId)
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user is null)
                return ServiceResult.Fail("User not found.");

            // Soft delete, same reasoning as Patients/Dentists.
            user.IsActive = false;
            await _repository.UpdateAsync(user);
            return ServiceResult.Ok();
        }

        private static ServiceResult Validate(User user, string? password, bool isNewUser)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                return ServiceResult.Fail("Username is required.");

            if (!ValidRoles.Contains(user.Role))
                return ServiceResult.Fail($"Role must be one of: {string.Join(", ", ValidRoles)}.");

            if (user.Role == "Dentist" && user.DentistId is null)
                return ServiceResult.Fail("A Dentist-role account must be linked to a dentist record.");

            if (isNewUser && string.IsNullOrWhiteSpace(password))
                return ServiceResult.Fail("Password is required for a new account.");

            if (!string.IsNullOrEmpty(password) && password.Length < 6)
                return ServiceResult.Fail("Password must be at least 6 characters.");

            return ServiceResult.Ok();
        }
    }
}
