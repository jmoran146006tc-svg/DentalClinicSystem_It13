using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlTreatmentRepository : ITreatmentRepository
    {
        public Task<IReadOnlyList<Treatment>> GetAllAsync()
            => QueryAsync("sp_Treatment_GetAll",
                MapTreatment);

        public Task<Treatment?> GetByIdAsync(int treatmentId)
            => QuerySingleAsync("sp_Treatment_GetById",
                MapTreatment,
                Parameter("@p_TreatmentId", treatmentId));

        public Task<IReadOnlyList<Treatment>> GetByAppointmentIdAsync(int appointmentId)
            => QueryAsync("sp_Treatment_GetByAppointmentId",
                MapTreatment,
                Parameter("@p_AppointmentId", appointmentId));

        public Task AddAsync(Treatment treatment)
            => ExecuteAsync("sp_Treatment_Add",
                Parameter("@p_AppointmentId", treatment.AppointmentId),
                Parameter("@p_TreatmentTypeId", treatment.TreatmentTypeId),
                Parameter("@p_ToothNumber", treatment.ToothNumber),
                Parameter("@p_Cost", treatment.Cost),
                Parameter("@p_DiscountType", treatment.DiscountType), Parameter("@p_DiscountPercent", treatment.DiscountPercent),
                Parameter("@p_DatePerformed", treatment.DatePerformed),
                Parameter("@p_Notes", treatment.Notes));

        public Task UpdateAsync(Treatment treatment)
            => ExecuteAsync("sp_Treatment_Update",
                Parameter("@p_TreatmentId", treatment.TreatmentId),
                Parameter("@p_AppointmentId", treatment.AppointmentId),
                Parameter("@p_TreatmentTypeId", treatment.TreatmentTypeId),
                Parameter("@p_ToothNumber", treatment.ToothNumber),
                Parameter("@p_Cost", treatment.Cost),
                Parameter("@p_DiscountType", treatment.DiscountType), Parameter("@p_DiscountPercent", treatment.DiscountPercent),
                Parameter("@p_DatePerformed", treatment.DatePerformed),
                Parameter("@p_Notes", treatment.Notes));

        public Task<IReadOnlyList<Treatment>> GetByPatientIdAsync(int patientId)
            => QueryAsync("sp_Treatment_GetByPatientId", MapTreatment, Parameter("@p_PatientId", patientId));

        private static Treatment MapTreatment(MySqlDataReader reader) => new()
        {
            TreatmentId = (int)reader["TreatmentId"],
            AppointmentId = (int)reader["AppointmentId"],
            TreatmentTypeId = (int)reader["TreatmentTypeId"],
            ToothNumber = reader.GetNullableString("ToothNumber"),
            Cost = (decimal)reader["Cost"],
            DiscountType = reader.GetString("DiscountType"), DiscountPercent = reader.GetDecimal("DiscountPercent"),
            DatePerformed = (DateTime)reader["DatePerformed"],
            Notes = reader.GetNullableString("Notes")
        };
    }
}
