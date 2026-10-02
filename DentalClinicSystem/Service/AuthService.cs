using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository) => _userRepository = userRepository;

        public async Task<ServiceResult<User>> LoginAsync(string username, string password)
        {
            username = Validator.Sanitize(username);
            if (!Validator.Required(username, "Username").Success || string.IsNullOrEmpty(password) ||
                !Validator.MaxLength(username, FieldLimits.Username, "Username").Success)
                return ServiceResult<User>.Fail("Invalid username or password.");
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user is null || !user.IsActive)
                return ServiceResult<User>.Fail("Invalid username or password.");

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return ServiceResult<User>.Fail("Invalid username or password.");

            return ServiceResult<User>.Ok(new User
            {
                UserId = user.UserId, Username = user.Username, Role = user.Role,
                DentistId = user.DentistId, IsActive = user.IsActive
            });
        }
    }
}
