using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlTreatmentTypeRepository : ITreatmentTypeRepository
    {
        public Task<IReadOnlyList<TreatmentType>> GetAllAsync()
            => QueryAsync("sp_TreatmentType_GetAll",
                MapTreatmentType);

        public Task<TreatmentType?> GetByIdAsync(int treatmentTypeId)
            => QuerySingleAsync("sp_TreatmentType_GetById",
                MapTreatmentType,
                Parameter("@p_TreatmentTypeId", treatmentTypeId));

        private static TreatmentType MapTreatmentType(MySqlDataReader reader) => new()
        {
            TreatmentTypeId = (int)reader["TreatmentTypeId"],
            Name = (string)reader["Name"],
            DefaultDurationMinutes = reader.GetInt32("DefaultDurationMinutes"),
            DefaultCost = (decimal)reader["DefaultCost"],
            Description = reader.GetNullableString("Description")
        };
    }
}
