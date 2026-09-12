using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.Repositories
{
    public class MySqlPatientRepository : IPatientRepository
    {
        public async Task<IReadOnlyList<Patient>> GetAllAsync()
        {
            List<Patient> patients = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                "SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, IsActive, CreatedAt " +
                "FROM Patients WHERE IsActive = 1 ORDER BY LastName, FirstName", conn);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                patients.Add(MapPatient(reader));
            }

            return patients;
        }

        public async Task<Patient?> GetByIdAsync(int patientId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                "SELECT PatientId, FirstName, LastName, DateOfBirth, ContactNumber, Email, Address, IsActive, CreatedAt " +
                "FROM Patients WHERE PatientId = @id", conn);
            cmd.Parameters.AddWithValue("@id", patientId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapPatient(reader) : null;
        }

        public async Task AddAsync(Patient patient)
        {
            const string sql =
                "INSERT INTO Patients (FirstName, LastName, DateOfBirth, ContactNumber, Email, Address) " +
                "VALUES (@fn, @ln, @dob, @cn, @email, @addr)";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddPatientParameters(cmd, patient);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Patient patient)
        {
            const string sql =
                "UPDATE Patients SET FirstName = @fn, LastName = @ln, DateOfBirth = @dob, " +
                "ContactNumber = @cn, Email = @email, Address = @addr WHERE PatientId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddPatientParameters(cmd, patient);
            cmd.Parameters.AddWithValue("@id", patient.PatientId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int patientId)
        {
            // Soft delete only. Never hard-delete a patient - it would orphan their
            // appointment/treatment history. See the Section 5.3 notes.
            const string sql = "UPDATE Patients SET IsActive = 0 WHERE PatientId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", patientId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddPatientParameters(MySqlCommand cmd, Patient patient)
        {
            cmd.Parameters.AddWithValue("@fn", patient.FirstName);
            cmd.Parameters.AddWithValue("@ln", patient.LastName);
            cmd.Parameters.AddWithValue("@dob", patient.DateOfBirth);
            cmd.Parameters.AddWithValue("@cn", patient.ContactNumber);
            cmd.Parameters.AddWithValue("@email", (object?)patient.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@addr", (object?)patient.Address ?? DBNull.Value);
        }

        // reader["ColumnName"] + an explicit cast, not GetInt32("...")/GetString("...") -
        // those typed accessors only take an ordinal int in the base ADO.NET reader API,
        // not a column name, so e.g. GetString("Email") won't compile against
        // MySqlDataReader. `reader["Email"] as string` also gets a null-safe read for
        // free, since a DBNull value simply fails the `as` cast instead of throwing.
        // Convert.ToBoolean(...) is used for IsActive rather than a direct (bool) cast
        // so this keeps working even if a connection string ever disables
        // MySqlConnector's "treat TINYINT(1) as boolean" default.
        private static Patient MapPatient(MySqlDataReader reader) => new()
        {
            PatientId = (int)reader["PatientId"],
            FirstName = (string)reader["FirstName"],
            LastName = (string)reader["LastName"],
            DateOfBirth = (DateTime)reader["DateOfBirth"],
            ContactNumber = (string)reader["ContactNumber"],
            Email = reader["Email"] as string,
            Address = reader["Address"] as string,
            IsActive = Convert.ToBoolean(reader["IsActive"]),
            CreatedAt = (DateTime)reader["CreatedAt"]
        };
    }
}
