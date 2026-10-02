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

        public Task AddAsync(TreatmentType treatmentType)
            => ExecuteAsync("sp_TreatmentType_Add",
                Parameter("@p_Name", treatmentType.Name),
                Parameter("@p_DefaultCost", treatmentType.DefaultCost),
                Parameter("@p_Description", treatmentType.Description));

        public Task UpdateAsync(TreatmentType treatmentType)
            => ExecuteAsync("sp_TreatmentType_Update",
                Parameter("@p_TreatmentTypeId", treatmentType.TreatmentTypeId),
                Parameter("@p_Name", treatmentType.Name),
                Parameter("@p_DefaultCost", treatmentType.DefaultCost),
                Parameter("@p_Description", treatmentType.Description));

        public Task DeleteAsync(int treatmentTypeId)
            => ExecuteAsync("sp_TreatmentType_Delete",
                "This treatment type is used by one or more treatment records and can't be deleted.",
                Parameter("@p_TreatmentTypeId", treatmentTypeId));

        private static TreatmentType MapTreatmentType(MySqlDataReader reader) => new()
        {
            TreatmentTypeId = (int)reader["TreatmentTypeId"],
            Name = (string)reader["Name"],
            DefaultCost = (decimal)reader["DefaultCost"],
            Description = reader.GetNullableString("Description")
        };
    }
}
