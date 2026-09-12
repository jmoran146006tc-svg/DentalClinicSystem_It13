using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.Repositories
{
    public class MySqlDentistRepository : IDentistRepository
    {
        public async Task<IReadOnlyList<Dentist>> GetAllAsync()
        {
            List<Dentist> dentists = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                "SELECT DentistId, FirstName, LastName, Specialization, ContactNumber, LicenseNumber, IsActive " +
                "FROM Dentists WHERE IsActive = 1 ORDER BY LastName, FirstName", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                dentists.Add(MapDentist(reader));
            }

            return dentists;
        }

        public async Task<Dentist?> GetByIdAsync(int dentistId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                "SELECT DentistId, FirstName, LastName, Specialization, ContactNumber, LicenseNumber, IsActive " +
                "FROM Dentists WHERE DentistId = @id", conn);
            cmd.Parameters.AddWithValue("@id", dentistId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapDentist(reader) : null;
        }

        public async Task AddAsync(Dentist dentist)
        {
            const string sql =
                "INSERT INTO Dentists (FirstName, LastName, Specialization, ContactNumber, LicenseNumber) " +
                "VALUES (@fn, @ln, @spec, @cn, @lic)";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddDentistParameters(cmd, dentist);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Dentist dentist)
        {
            const string sql =
                "UPDATE Dentists SET FirstName = @fn, LastName = @ln, Specialization = @spec, " +
                "ContactNumber = @cn, LicenseNumber = @lic WHERE DentistId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddDentistParameters(cmd, dentist);
            cmd.Parameters.AddWithValue("@id", dentist.DentistId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int dentistId)
        {
            // Soft delete - a dentist's appointment/treatment history must stay intact.
            const string sql = "UPDATE Dentists SET IsActive = 0 WHERE DentistId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", dentistId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddDentistParameters(MySqlCommand cmd, Dentist dentist)
        {
            cmd.Parameters.AddWithValue("@fn", dentist.FirstName);
            cmd.Parameters.AddWithValue("@ln", dentist.LastName);
            cmd.Parameters.AddWithValue("@spec", (object?)dentist.Specialization ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cn", (object?)dentist.ContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lic", (object?)dentist.LicenseNumber ?? DBNull.Value);
        }

        private static Dentist MapDentist(MySqlDataReader reader) => new()
        {
            DentistId = (int)reader["DentistId"],
            FirstName = (string)reader["FirstName"],
            LastName = (string)reader["LastName"],
            Specialization = reader["Specialization"] as string,
            ContactNumber = reader["ContactNumber"] as string,
            LicenseNumber = reader["LicenseNumber"] as string,
            IsActive = Convert.ToBoolean(reader["IsActive"])
        };
    }
}
