using MySqlConnector;

namespace DentalClinicSystem.Helpers
{
    /// <summary>
    /// Single source of truth for opening a MySQL connection. Every repository calls
    /// GetOpenConnectionAsync() instead of building its own MySqlConnection - this is
    /// the "one connection-string setup, referenced everywhere" DRY rule from
    /// Section 3.3 of the project plan, applied to the data layer.
    /// </summary>
    public static class DbConnectionHelper
    {
        // TODO before you run anything: change the password below (and Server=, if you
        // demo on a different machine than you developed on). Longer term this string
        // belongs in an App.config/appsettings.json file instead of hard-coded source,
        // but for a project this size a single constant is a reasonable starting point.
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
