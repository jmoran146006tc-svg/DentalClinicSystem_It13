using System.Data;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public class MySqlPatientRepository : IPatientRepository
    {
        public async Task<IReadOnlyList<Patient>> GetAllAsync()
        {
            List<Patient> patients = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Patient_GetAll", conn) { CommandType = CommandType.StoredProcedure };

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
            await using var cmd = new MySqlCommand("sp_Patient_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_PatientId", patientId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapPatient(reader) : null;
        }

        public async Task AddAsync(Patient patient)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Patient_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddPatientParameters(cmd, patient);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Patient patient)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Patient_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_PatientId", patient.PatientId);
            AddPatientParameters(cmd, patient);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int patientId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Patient_Delete", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_PatientId", patientId);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void AddPatientParameters(MySqlCommand cmd, Patient patient)
        {
            cmd.Parameters.AddWithValue("@p_FirstName", patient.FirstName);
            cmd.Parameters.AddWithValue("@p_LastName", patient.LastName);
            cmd.Parameters.AddWithValue("@p_DateOfBirth", patient.DateOfBirth);
            cmd.Parameters.AddWithValue("@p_ContactNumber", patient.ContactNumber);
            cmd.Parameters.AddWithValue("@p_Email", (object?)patient.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p_Address", (object?)patient.Address ?? DBNull.Value);
        }

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
