using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.Repositories
{
    public class MySqlTreatmentTypeRepository : ITreatmentTypeRepository
    {
        public async Task<IReadOnlyList<TreatmentType>> GetAllAsync()
        {
            List<TreatmentType> types = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                "SELECT TreatmentTypeId, Name, DefaultCost, Description " +
                "FROM TreatmentTypes ORDER BY Name", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                types.Add(MapTreatmentType(reader));
            }

            return types;
        }

        public async Task<TreatmentType?> GetByIdAsync(int treatmentTypeId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                "SELECT TreatmentTypeId, Name, DefaultCost, Description " +
                "FROM TreatmentTypes WHERE TreatmentTypeId = @id", conn);
            cmd.Parameters.AddWithValue("@id", treatmentTypeId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTreatmentType(reader) : null;
        }

        public async Task AddAsync(TreatmentType treatmentType)
        {
            const string sql =
                "INSERT INTO TreatmentTypes (Name, DefaultCost, Description) VALUES (@name, @cost, @desc)";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddTreatmentTypeParameters(cmd, treatmentType);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(TreatmentType treatmentType)
        {
            const string sql =
                "UPDATE TreatmentTypes SET Name = @name, DefaultCost = @cost, Description = @desc " +
                "WHERE TreatmentTypeId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddTreatmentTypeParameters(cmd, treatmentType);
            cmd.Parameters.AddWithValue("@id", treatmentType.TreatmentTypeId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int treatmentTypeId)
        {
            // TreatmentTypes has no IsActive flag so this is a real
            // DELETE - and the FK from Treatments will correctly reject it if any
            // treatment record still points at this type. We translate that into a
            // RepositoryConstraintException so TreatmentTypeService can show a plain-
            // English message instead of a raw MySqlException bubbling up.
            const string sql = "DELETE FROM TreatmentTypes WHERE TreatmentTypeId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", treatmentTypeId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.RowIsReferenced2)
            {
                throw new RepositoryConstraintException(
                    "This treatment type is used by one or more treatment records and can't be deleted.", ex);
            }
        }

        private static void AddTreatmentTypeParameters(MySqlCommand cmd, TreatmentType treatmentType)
        {
            cmd.Parameters.AddWithValue("@name", treatmentType.Name);
            cmd.Parameters.AddWithValue("@cost", treatmentType.DefaultCost);
            cmd.Parameters.AddWithValue("@desc", (object?)treatmentType.Description ?? DBNull.Value);
        }

        private static TreatmentType MapTreatmentType(MySqlDataReader reader) => new()
        {
            TreatmentTypeId = (int)reader["TreatmentTypeId"],
            Name = (string)reader["Name"],
            DefaultCost = (decimal)reader["DefaultCost"],
            Description = reader["Description"] as string
        };
    }
}
