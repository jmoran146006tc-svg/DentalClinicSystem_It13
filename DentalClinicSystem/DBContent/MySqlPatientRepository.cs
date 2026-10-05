using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlPatientRepository : IPatientRepository
    {
        public Task<IReadOnlyList<Patient>> GetAllAsync()
            => QueryAsync("sp_Patient_GetAll",
                MapPatient);

        public Task<Patient?> GetByIdAsync(int patientId)
            => QuerySingleAsync("sp_Patient_GetById",
                MapPatient,
                Parameter("@p_PatientId", patientId));

        public Task AddAsync(Patient patient)
            => ExecuteAsync("sp_Patient_Add",
                Parameter("@p_FirstName", patient.FirstName),
                Parameter("@p_LastName", patient.LastName),
                Parameter("@p_DateOfBirth", patient.DateOfBirth),
                Parameter("@p_ContactNumber", patient.ContactNumber),
                Parameter("@p_Email", patient.Email),
                Parameter("@p_Address", patient.Address),
                Parameter("@p_GuardianName", patient.GuardianName), Parameter("@p_GuardianContact", patient.GuardianContact),
                Parameter("@p_Allergies", patient.Allergies), Parameter("@p_MedicalNotes", patient.MedicalNotes));

        public Task UpdateAsync(Patient patient)
            => ExecuteAsync("sp_Patient_Update",
                Parameter("@p_PatientId", patient.PatientId),
                Parameter("@p_FirstName", patient.FirstName),
                Parameter("@p_LastName", patient.LastName),
                Parameter("@p_DateOfBirth", patient.DateOfBirth),
                Parameter("@p_ContactNumber", patient.ContactNumber),
                Parameter("@p_Email", patient.Email),
                Parameter("@p_Address", patient.Address),
                Parameter("@p_GuardianName", patient.GuardianName), Parameter("@p_GuardianContact", patient.GuardianContact),
                Parameter("@p_Allergies", patient.Allergies), Parameter("@p_MedicalNotes", patient.MedicalNotes));

        public Task<IReadOnlyList<Patient>> GetAllIncludingInactiveAsync()
            => QueryAsync("sp_Patient_GetAllIncludingInactive", MapPatient);

        public Task<IReadOnlyList<Patient>> FindByNameAndDateOfBirthAsync(string first, string last, DateTime dob) =>
            QueryAsync("sp_Patient_FindByNameAndDob", MapPatient, Parameter("@p_FirstName", first), Parameter("@p_LastName", last), Parameter("@p_DateOfBirth", dob.Date));
        public Task ReactivateAsync(int patientId)
            => ExecuteAsync("sp_Patient_Reactivate", Parameter("@p_PatientId", patientId));

        private static Patient MapPatient(MySqlDataReader reader) => new()
        {
            PatientId = (int)reader["PatientId"],
            FirstName = (string)reader["FirstName"],
            LastName = (string)reader["LastName"],
            DateOfBirth = (DateTime)reader["DateOfBirth"],
            ContactNumber = (string)reader["ContactNumber"],
            Email = reader.GetNullableString("Email"),
            Address = reader.GetNullableString("Address"),
            GuardianName = reader.GetNullableString("GuardianName"), GuardianContact = reader.GetNullableString("GuardianContact"),
            Allergies = reader.GetNullableString("Allergies"), MedicalNotes = reader.GetNullableString("MedicalNotes"),
            IsActive = reader.GetBool("IsActive"),
            CreatedAt = (DateTime)reader["CreatedAt"]
        };
    }
}
