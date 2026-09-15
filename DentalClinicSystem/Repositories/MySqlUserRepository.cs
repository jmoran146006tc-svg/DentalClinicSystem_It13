using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.Repositories
{
    public class MySqlUserRepository : IUserRepository
    {
        private const string SelectColumns =
            "SELECT UserId, Username, PasswordHash, Role, DentistId, IsActive FROM Users";

        public async Task<User?> GetByIdAsync(int userId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand($"{SelectColumns} WHERE UserId = @id", conn);
            cmd.Parameters.AddWithValue("@id", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapUser(reader) : null;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            // Deliberately does NOT filter WHERE IsActive = 1 here - AuthService needs
            // to see a disabled account so it can return "Invalid username or
            // password" rather than silently treating it as "user not found".
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand($"{SelectColumns} WHERE Username = @username", conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapUser(reader) : null;
        }

        public async Task AddAsync(User user)
        {
            const string sql =
                "INSERT INTO Users (Username, PasswordHash, Role, DentistId, IsActive) " +
                "VALUES (@username, @hash, @role, @dentistId, @isActive)";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddUserParameters(cmd, user);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(User user)
        {
            const string sql =
                "UPDATE Users SET Username = @username, PasswordHash = @hash, Role = @role, " +
                "DentistId = @dentistId, IsActive = @isActive WHERE UserId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddUserParameters(cmd, user);
            cmd.Parameters.AddWithValue("@id", user.UserId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            // Deliberately does NOT filter WHERE IsActive = 1, unlike Patients/Dentists -
            // an admin managing accounts needs to see deactivated ones too, in case
            // someone needs to be reactivated later.
            List<User> users = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand($"{SelectColumns} ORDER BY Username", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(MapUser(reader));
            }

            return users;
        }

        private static void AddUserParameters(MySqlCommand cmd, User user)
        {
            cmd.Parameters.AddWithValue("@username", user.Username);
            cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@role", user.Role);
            cmd.Parameters.AddWithValue("@dentistId", (object?)user.DentistId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@isActive", user.IsActive);
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
