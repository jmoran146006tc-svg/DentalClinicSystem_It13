using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    internal static class DataReaderExtensions
    {
        public static string? GetNullableString(this MySqlDataReader reader, string column) =>
            reader[column] as string;
        public static int? GetNullableInt(this MySqlDataReader reader, string column) =>
            reader.IsDBNull(reader.GetOrdinal(column)) ? null : reader.GetInt32(column);
        public static bool GetBool(this MySqlDataReader reader, string column) => Convert.ToBoolean(reader[column]);
    }
}
