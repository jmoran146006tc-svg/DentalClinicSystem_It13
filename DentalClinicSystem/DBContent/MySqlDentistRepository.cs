using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlDentistRepository : IDentistRepository
    {
        public Task<IReadOnlyList<Dentist>> GetAllAsync()
            => QueryAsync("sp_Dentist_GetAll",
                MapDentist);

        public Task<Dentist?> GetByIdAsync(int dentistId)
            => QuerySingleAsync("sp_Dentist_GetById",
                MapDentist,
                Parameter("@p_DentistId", dentistId));

        public Task AddAsync(Dentist dentist)
            => ExecuteAsync("sp_Dentist_Add",
                Parameter("@p_FirstName", dentist.FirstName),
                Parameter("@p_LastName", dentist.LastName),
                Parameter("@p_Specialization", dentist.Specialization),
                Parameter("@p_ContactNumber", dentist.ContactNumber),
                Parameter("@p_LicenseNumber", dentist.LicenseNumber));

        public Task UpdateAsync(Dentist dentist)
            => ExecuteAsync("sp_Dentist_Update",
                Parameter("@p_DentistId", dentist.DentistId),
                Parameter("@p_FirstName", dentist.FirstName),
                Parameter("@p_LastName", dentist.LastName),
                Parameter("@p_Specialization", dentist.Specialization),
                Parameter("@p_ContactNumber", dentist.ContactNumber),
                Parameter("@p_LicenseNumber", dentist.LicenseNumber));

        public Task DeleteAsync(int dentistId)
            => ExecuteAsync("sp_Dentist_Delete",
                Parameter("@p_DentistId", dentistId));

        private static Dentist MapDentist(MySqlDataReader reader) => new()
        {
            DentistId = (int)reader["DentistId"],
            FirstName = (string)reader["FirstName"],
            LastName = (string)reader["LastName"],
            Specialization = reader.GetNullableString("Specialization"),
            ContactNumber = reader.GetNullableString("ContactNumber"),
            LicenseNumber = reader.GetNullableString("LicenseNumber"),
            IsActive = reader.GetBool("IsActive")
        };
    }
}
