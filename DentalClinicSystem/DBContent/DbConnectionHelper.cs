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


        public static async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }

        public static async Task<bool> CanConnectAsync()
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                return true;
            }
            catch (MySqlException)
            {
                return false;
            }
        }
    }
}
