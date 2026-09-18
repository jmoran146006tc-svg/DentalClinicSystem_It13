using MySqlConnector;

namespace DentalClinicSystem.Helpers
{

    public static class DbConnectionHelper
    {
        private const string ConnectionString =
            "Server=localhost;Database=dentalclinicdb;User=root;Password=;";

        /// <summary>
        /// Returns an already-open MySqlConnection. Callers are responsible for
        /// disposing it - always use "await using" so the connection is closed and
        /// returned to the pool even if the query throws.
        /// </summary>
        public static async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
