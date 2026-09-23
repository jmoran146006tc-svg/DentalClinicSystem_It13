using System.Data;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public class MySqlUserRepository : IUserRepository
    {
        public async Task<User?> GetByIdAsync(int userId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_User_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapUser(reader) : null;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            // Deliberately does NOT filter WHERE IsActive = 1 here - AuthService needs
            // to see a disabled account so it can return "Invalid username or
            // password" rather than silently treating it as "user not found".
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_User_GetByUsername", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_Username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapUser(reader) : null;
        }

        public async Task AddAsync(User user)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_User_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddUserParameters(cmd, user);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(User user)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_User_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_UserId", user.UserId);
            AddUserParameters(cmd, user);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            // Deliberately does NOT filter WHERE IsActive = 1, unlike Patients/Dentists -
            // an admin managing accounts needs to see deactivated ones too, in case
            // someone needs to be reactivated later.
            List<User> users = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_User_GetAll", conn) { CommandType = CommandType.StoredProcedure };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        private static void AddUserParameters(MySqlCommand cmd, User user)
        {
            cmd.Parameters.AddWithValue("@p_Username", user.Username);
            cmd.Parameters.AddWithValue("@p_PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@p_Role", user.Role);
            cmd.Parameters.AddWithValue("@p_DentistId", (object?)user.DentistId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p_IsActive", user.IsActive);
        }

        // `as int?` is legal here (unlike a direct cast) and returns null instead of
        // throwing when the boxed value is DBNull.Value rather than a boxed int.
        private static User MapUser(MySqlDataReader reader) => new()
        {
            UserId = (int)reader["UserId"],
            Username = (string)reader["Username"],
            PasswordHash = (string)reader["PasswordHash"],
            Role = (string)reader["Role"],
            DentistId = reader["DentistId"] as int?,
            IsActive = Convert.ToBoolean(reader["IsActive"])
        };
    }
}
