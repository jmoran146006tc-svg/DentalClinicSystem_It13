using System.Data;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public class MySqlTreatmentTypeRepository : ITreatmentTypeRepository
    {
        public async Task<IReadOnlyList<TreatmentType>> GetAllAsync()
        {
            List<TreatmentType> types = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_TreatmentType_GetAll", conn) { CommandType = CommandType.StoredProcedure };

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
            await using var cmd = new MySqlCommand("sp_TreatmentType_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_TreatmentTypeId", treatmentTypeId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapTreatmentType(reader) : null;
        }

        public async Task AddAsync(TreatmentType treatmentType)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_TreatmentType_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddTreatmentTypeParameters(cmd, treatmentType);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(TreatmentType treatmentType)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_TreatmentType_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_TreatmentTypeId", treatmentType.TreatmentTypeId);
            AddTreatmentTypeParameters(cmd, treatmentType);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int treatmentTypeId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_TreatmentType_Delete", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_TreatmentTypeId", treatmentTypeId);

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
            cmd.Parameters.AddWithValue("@p_Name", treatmentType.Name);
            cmd.Parameters.AddWithValue("@p_DefaultCost", treatmentType.DefaultCost);
            cmd.Parameters.AddWithValue("@p_Description", (object?)treatmentType.Description ?? DBNull.Value);
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
