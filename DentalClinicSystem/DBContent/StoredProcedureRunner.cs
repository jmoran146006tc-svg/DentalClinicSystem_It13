using System.Data;
using DentalClinicSystem.Interfaces;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    internal static class StoredProcedureRunner
    {
        public static MySqlParameter Parameter(string name, object? value) => new(name, value ?? DBNull.Value);

        public static async Task<IReadOnlyList<T>> QueryAsync<T>(string procedure,
            Func<MySqlDataReader, T> map, params MySqlParameter[] parameters)
        {
            await using var connection = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var command = new MySqlCommand(procedure, connection) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddRange(parameters);
            await using var reader = await command.ExecuteReaderAsync();
            List<T> rows = [];
            while (await reader.ReadAsync()) rows.Add(map(reader));
            return rows;
        }

        public static async Task<T?> QuerySingleAsync<T>(string procedure,
            Func<MySqlDataReader, T> map, params MySqlParameter[] parameters) where T : class =>
            (await QueryAsync(procedure, map, parameters)).FirstOrDefault();

        public static Task ExecuteAsync(string procedure, params MySqlParameter[] parameters) =>
            ExecuteAsync(procedure, "This record is in use and cannot be deleted.", parameters);

        public static async Task ExecuteAsync(string procedure, string constraintMessage, params MySqlParameter[] parameters)
        {
            await using var connection = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var command = new MySqlCommand(procedure, connection) { CommandType = CommandType.StoredProcedure };
            command.Parameters.AddRange(parameters);
            try { await command.ExecuteNonQueryAsync(); }
            catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.RowIsReferenced2)
            {
                throw new RepositoryConstraintException(constraintMessage, ex);
            }
            catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
            {
                throw new RepositoryConstraintException("That record already exists. Check for a duplicate.", ex);
            }
        }
    }
}
