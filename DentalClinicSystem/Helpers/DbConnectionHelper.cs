using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;

namespace DentalClinicSystem.Helpers
{

    public static class DbConnectionHelper
    {
        private const string ConnectionString =
            "Server=localhost;Database=dentalclinicdb;User=root;Password=;";


        public static async Task<MySqlConnection> GetOpenConnectionAsync()
        {
            var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}
