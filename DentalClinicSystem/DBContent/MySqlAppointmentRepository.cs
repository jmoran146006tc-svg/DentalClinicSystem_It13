using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlAppointmentRepository : IAppointmentRepository
    {
        public Task<IReadOnlyList<Appointment>> GetAllAsync()
            => QueryAsync("sp_Appointment_GetAll",
                MapAppointment);

        public Task<Appointment?> GetByIdAsync(int appointmentId)
            => QuerySingleAsync("sp_Appointment_GetById",
                MapAppointment,
                Parameter("@p_AppointmentId", appointmentId));

        public Task AddAsync(Appointment appointment)
            => ExecuteAsync("sp_Appointment_Add",
                Parameter("@p_PatientId", appointment.PatientId),
                Parameter("@p_DentistId", appointment.DentistId),
                Parameter("@p_AppointmentDateTime", appointment.AppointmentDateTime),
                Parameter("@p_DurationMinutes", appointment.DurationMinutes),
                Parameter("@p_Status", appointment.Status),
                Parameter("@p_Reason", appointment.Reason),
                Parameter("@p_CancellationReason", appointment.CancellationReason),
                Parameter("@p_Notes", appointment.Notes));

        public Task UpdateAsync(Appointment appointment)
            => ExecuteAsync("sp_Appointment_Update",
                Parameter("@p_AppointmentId", appointment.AppointmentId),
                Parameter("@p_PatientId", appointment.PatientId),
                Parameter("@p_DentistId", appointment.DentistId),
                Parameter("@p_AppointmentDateTime", appointment.AppointmentDateTime),
                Parameter("@p_DurationMinutes", appointment.DurationMinutes),
                Parameter("@p_Status", appointment.Status),
                Parameter("@p_Reason", appointment.Reason),
                Parameter("@p_CancellationReason", appointment.CancellationReason),
                Parameter("@p_Notes", appointment.Notes));

        public Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(int patientId)
            => QueryAsync("sp_Appointment_GetByPatientId", MapAppointment, Parameter("@p_PatientId", patientId));

        public Task<IReadOnlyList<Appointment>> GetByRangeAsync(DateTime from, DateTime to)
            => QueryAsync("sp_Appointment_GetByRange", MapAppointment, Parameter("@p_From", from), Parameter("@p_To", to));

        public Task<IReadOnlyList<Appointment>> GetByDentistAndRangeAsync(int dentistId, DateTime from, DateTime to)
            => QueryAsync("sp_Appointment_GetByDentistAndRange", MapAppointment, Parameter("@p_DentistId", dentistId), Parameter("@p_From", from), Parameter("@p_To", to));

        public async Task<int> CountUpcomingByDentistAsync(int dentistId, DateTime from)
        {
            var rows = await QueryAsync("sp_Appointment_CountUpcomingByDentist", reader => reader.GetInt32("Total"),
                Parameter("@p_DentistId", dentistId), Parameter("@p_From", from));
            return rows.FirstOrDefault();
        }

        private static Appointment MapAppointment(MySqlDataReader reader) => new()
        {
            AppointmentId = (int)reader["AppointmentId"],
            PatientId = (int)reader["PatientId"],
            DentistId = (int)reader["DentistId"],
            AppointmentDateTime = (DateTime)reader["AppointmentDateTime"],
            DurationMinutes = reader.GetInt32("DurationMinutes"),
            Status = (string)reader["Status"],
            Reason = reader.GetNullableString("Reason"),
            CancellationReason = reader.GetNullableString("CancellationReason"),
            Notes = reader.GetNullableString("Notes"),
            CreatedAt = (DateTime)reader["CreatedAt"]
        };
    }
}
