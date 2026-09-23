using System.Data;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public class MySqlTreatmentRepository : ITreatmentRepository
    {
        public async Task<IReadOnlyList<Treatment>> GetAllAsync()
        {
            List<Treatment> treatments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Treatment_GetAll", conn) { CommandType = CommandType.StoredProcedure };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                treatments.Add(MapTreatment(reader));
            }

            return treatments;
        }

        public async Task<Treatment?> GetByIdAsync(int treatmentId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Treatment_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_TreatmentId", treatmentId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTreatment(reader) : null;
        }

        public async Task<IReadOnlyList<Treatment>> GetByAppointmentIdAsync(int appointmentId)
        {
            List<Treatment> treatments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Treatment_GetByAppointmentId", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_AppointmentId", appointmentId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                treatments.Add(MapTreatment(reader));
            }

            return treatments;
        }

        public async Task AddAsync(Treatment treatment)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Treatment_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddTreatmentParameters(cmd, treatment);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Treatment treatment)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Treatment_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_TreatmentId", treatment.TreatmentId);
            AddTreatmentParameters(cmd, treatment);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int treatmentId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Treatment_Delete", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_TreatmentId", treatmentId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddTreatmentParameters(MySqlCommand cmd, Treatment treatment)
        {
            cmd.Parameters.AddWithValue("@p_AppointmentId", treatment.AppointmentId);
            cmd.Parameters.AddWithValue("@p_TreatmentTypeId", treatment.TreatmentTypeId);
            cmd.Parameters.AddWithValue("@p_ToothNumber", (object?)treatment.ToothNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p_Cost", treatment.Cost);
            cmd.Parameters.AddWithValue("@p_DatePerformed", treatment.DatePerformed);
            cmd.Parameters.AddWithValue("@p_Notes", (object?)treatment.Notes ?? DBNull.Value);
        }

        private static Treatment MapTreatment(MySqlDataReader reader) => new()
        {
            TreatmentId = (int)reader["TreatmentId"],
            AppointmentId = (int)reader["AppointmentId"],
            TreatmentTypeId = (int)reader["TreatmentTypeId"],
            ToothNumber = reader["ToothNumber"] as string,
            Cost = (decimal)reader["Cost"],
            DatePerformed = (DateTime)reader["DatePerformed"],
            Notes = reader["Notes"] as string
        };
    }
}
