using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class UserService(IUserRepository repository, IDentistRepository dentists) : IUserService
    {
        public async Task<ServiceResult<IReadOnlyList<User>>> GetAllUsersAsync(User actor)
        {
            if (!RoleAccess.Can(actor, Permission.ViewUsers)) return RoleAccess.Denied<IReadOnlyList<User>>();
            return ServiceResult<IReadOnlyList<User>>.Ok((await repository.GetAllAsync()).Select(u => new User
            {
                UserId = u.UserId, Username = u.Username, Role = u.Role, DentistId = u.DentistId, IsActive = u.IsActive
            }).ToList());
        }
        public async Task<ServiceResult> AddUserAsync(User actor, User user, string password)
        {
            if (!RoleAccess.Can(actor, Permission.ManageUsers)) return RoleAccess.Denied();
            var validation = await ValidateAsync(user, password, true);
            if (!validation.Success) return validation;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            return await ServiceOperation.SaveAsync(() => repository.AddAsync(user));
        }
        public async Task<ServiceResult> UpdateUserAsync(User actor, User user, string? newPassword)
        {
            if (!RoleAccess.Can(actor, Permission.ManageUsers)) return RoleAccess.Denied();
            var existing = await repository.GetByIdAsync(user.UserId);
            if (existing is null) return ServiceResult.Fail("User not found.");
            var validation = await ValidateAsync(user, newPassword, false);
            if (!validation.Success) return validation;
            var protection = await ProtectAdminAsync(actor, existing, user);
            if (!protection.Success) return protection;
            user.PasswordHash = string.IsNullOrEmpty(newPassword) ? existing.PasswordHash : BCrypt.Net.BCrypt.HashPassword(newPassword);
            return await ServiceOperation.SaveAsync(() => repository.UpdateAsync(user));
        }
        public async Task<ServiceResult> DeactivateUserAsync(User actor, int userId)
        {
            if (!RoleAccess.Can(actor, Permission.ManageUsers)) return RoleAccess.Denied();
            var existing = await repository.GetByIdAsync(userId);
            if (existing is null) return ServiceResult.Fail("User not found.");
            var updated = new User { UserId = existing.UserId, Username = existing.Username, Role = existing.Role,
                DentistId = existing.DentistId, PasswordHash = existing.PasswordHash, IsActive = false };
            var protection = await ProtectAdminAsync(actor, existing, updated);
            if (!protection.Success) return protection;
            return await ServiceOperation.SaveAsync(() => repository.UpdateAsync(updated));
        }
        private async Task<ServiceResult> ValidateAsync(User user, string? password, bool isNew)
        {
            var validation = Validator.User(user, password, isNew);
            if (!validation.Success) return validation;
            var duplicate = await repository.GetByUsernameAsync(user.Username);
            if (duplicate is not null && (isNew || duplicate.UserId != user.UserId)) return ServiceResult.Fail("That username is already taken.");
            if (RoleAccess.RequiresDentist(user.Role) &&
                (user.DentistId is not int id || await dentists.GetByIdAsync(id) is not { IsActive: true }))
                return ServiceResult.Fail("Link this account to an active dentist.");
            return ServiceResult.Ok();
        }
        private async Task<ServiceResult> ProtectAdminAsync(User actor, User existing, User updated)
        {
            if (actor.UserId == updated.UserId && !updated.IsActive) return ServiceResult.Fail("You cannot deactivate your own account.");
            if (existing.IsActive && RoleAccess.IsAdmin(existing) && (!updated.IsActive || !RoleAccess.IsAdmin(updated)) &&
                (await repository.GetAllAsync()).Count(u => u.IsActive && RoleAccess.IsAdmin(u)) <= 1)
                return ServiceResult.Fail("The last active Admin cannot be deactivated or demoted.");
            return ServiceResult.Ok();
        }
    }
}
