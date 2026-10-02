using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(int userId)
            => QuerySingleAsync("sp_User_GetById",
                MapUser,
                Parameter("@p_UserId", userId));

        public Task<User?> GetByUsernameAsync(string username)
            => QuerySingleAsync("sp_User_GetByUsername",
                MapUser,
                Parameter("@p_Username", username));

        public Task AddAsync(User user)
            => ExecuteAsync("sp_User_Add",
                Parameter("@p_Username", user.Username),
                Parameter("@p_PasswordHash", user.PasswordHash),
                Parameter("@p_Role", user.Role),
                Parameter("@p_DentistId", user.DentistId),
                Parameter("@p_IsActive", user.IsActive));

        public Task UpdateAsync(User user)
            => ExecuteAsync("sp_User_Update",
                Parameter("@p_UserId", user.UserId),
                Parameter("@p_Username", user.Username),
                Parameter("@p_PasswordHash", user.PasswordHash),
                Parameter("@p_Role", user.Role),
                Parameter("@p_DentistId", user.DentistId),
                Parameter("@p_IsActive", user.IsActive));

        public Task<IReadOnlyList<User>> GetAllAsync()
            => QueryAsync("sp_User_GetAll",
                MapUser);

        private static User MapUser(MySqlDataReader reader) => new()
        {
            UserId = (int)reader["UserId"],
            Username = (string)reader["Username"],
            PasswordHash = (string)reader["PasswordHash"],
            Role = (string)reader["Role"],
            DentistId = reader.GetNullableInt("DentistId"),
            IsActive = reader.GetBool("IsActive")
        };
    }
}
