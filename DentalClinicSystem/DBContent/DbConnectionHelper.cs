using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public static class DbConnectionHelper
    {
        private const string Server = "localhost";
        private const string DatabaseName = "dentalclinicdb";
        private const string UserId = "root";
        private const string Password = "";

        private static readonly string ConnectionString =
            $"Server={Server};Database={DatabaseName};User={UserId};Password={Password};";

        private static readonly string ServerOnlyConnectionString =
            $"Server={Server};User={UserId};Password={Password};";

        public static string DatabaseNameValue => DatabaseName;

        public static async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }

        public static async Task<MySqlConnection> GetOpenServerConnectionAsync()
        {
            var connection = new MySqlConnection(ServerOnlyConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
