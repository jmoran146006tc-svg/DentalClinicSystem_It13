using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.Repositories
{
    public class MySqlTreatmentRepository : ITreatmentRepository
    {
        private const string SelectColumns =
            "SELECT TreatmentId, AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes FROM Treatments";

        public async Task<IReadOnlyList<Treatment>> GetAllAsync()
        {
            List<Treatment> treatments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand($"{SelectColumns} ORDER BY DatePerformed DESC", conn);

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
            await using var cmd = new MySqlCommand($"{SelectColumns} WHERE TreatmentId = @id", conn);
            cmd.Parameters.AddWithValue("@id", treatmentId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTreatment(reader) : null;
        }

        public async Task<IReadOnlyList<Treatment>> GetByAppointmentIdAsync(int appointmentId)
        {
            List<Treatment> treatments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                $"{SelectColumns} WHERE AppointmentId = @appointmentId", conn);
            cmd.Parameters.AddWithValue("@appointmentId", appointmentId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                treatments.Add(MapTreatment(reader));
            }

            return treatments;
        }

        public async Task AddAsync(Treatment treatment)
        {
            const string sql =
                "INSERT INTO Treatments (AppointmentId, TreatmentTypeId, ToothNumber, Cost, DatePerformed, Notes) " +
                "VALUES (@aid, @ttid, @tooth, @cost, @date, @notes)";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddTreatmentParameters(cmd, treatment);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Treatment treatment)
        {
            const string sql =
                "UPDATE Treatments SET AppointmentId = @aid, TreatmentTypeId = @ttid, ToothNumber = @tooth, " +
                "Cost = @cost, DatePerformed = @date, Notes = @notes WHERE TreatmentId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddTreatmentParameters(cmd, treatment);
            cmd.Parameters.AddWithValue("@id", treatment.TreatmentId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int treatmentId)
        {
            const string sql = "DELETE FROM Treatments WHERE TreatmentId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", treatmentId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddTreatmentParameters(MySqlCommand cmd, Treatment treatment)
        {
            cmd.Parameters.AddWithValue("@aid", treatment.AppointmentId);
            cmd.Parameters.AddWithValue("@ttid", treatment.TreatmentTypeId);
            cmd.Parameters.AddWithValue("@tooth", (object?)treatment.ToothNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cost", treatment.Cost);
            cmd.Parameters.AddWithValue("@date", treatment.DatePerformed);
            cmd.Parameters.AddWithValue("@notes", (object?)treatment.Notes ?? DBNull.Value);
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
