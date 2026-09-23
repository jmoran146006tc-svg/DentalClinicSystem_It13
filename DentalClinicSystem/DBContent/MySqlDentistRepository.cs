using System.Data;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public class MySqlDentistRepository : IDentistRepository
    {
        public async Task<IReadOnlyList<Dentist>> GetAllAsync()
        {
            List<Dentist> dentists = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Dentist_GetAll", conn) { CommandType = CommandType.StoredProcedure };

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
            await using var cmd = new MySqlCommand("sp_Dentist_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_DentistId", dentistId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapDentist(reader) : null;
        }

        public async Task AddAsync(Dentist dentist)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Dentist_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddDentistParameters(cmd, dentist);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Dentist dentist)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Dentist_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_DentistId", dentist.DentistId);
            AddDentistParameters(cmd, dentist);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int dentistId)
        {
            // Soft delete - a dentist's appointment/treatment history must stay intact.
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Dentist_Delete", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_DentistId", dentistId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddDentistParameters(MySqlCommand cmd, Dentist dentist)
        {
            cmd.Parameters.AddWithValue("@p_FirstName", dentist.FirstName);
            cmd.Parameters.AddWithValue("@p_LastName", dentist.LastName);
            cmd.Parameters.AddWithValue("@p_Specialization", (object?)dentist.Specialization ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p_ContactNumber", (object?)dentist.ContactNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p_LicenseNumber", (object?)dentist.LicenseNumber ?? DBNull.Value);
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
