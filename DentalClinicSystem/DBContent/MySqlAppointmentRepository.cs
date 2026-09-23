using System.Data;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.DBContent
{
    public class MySqlAppointmentRepository : IAppointmentRepository
    {
        public async Task<IReadOnlyList<Appointment>> GetAllAsync()
        {
            List<Appointment> appointments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Appointment_GetAll", conn) { CommandType = CommandType.StoredProcedure };

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                appointments.Add(MapAppointment(reader));
            }

            return appointments;
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Appointment_GetById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_AppointmentId", appointmentId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapAppointment(reader) : null;
        }

        public async Task<IReadOnlyList<Appointment>> GetByDentistAndDateAsync(int dentistId, DateTime date)
        {
            List<Appointment> appointments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Appointment_GetByDentistAndDate", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_DentistId", dentistId);
            cmd.Parameters.AddWithValue("@p_Date", date.Date);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                appointments.Add(MapAppointment(reader));
            }

            return appointments;
        }

        public async Task AddAsync(Appointment appointment)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Appointment_Add", conn) { CommandType = CommandType.StoredProcedure };
            AddAppointmentParameters(cmd, appointment);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Appointment_Update", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_AppointmentId", appointment.AppointmentId);
            AddAppointmentParameters(cmd, appointment);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int appointmentId)
        {
            // Prefer AppointmentService.UpdateStatusAsync(id, "Cancelled") over calling
            // this directly - an appointment with treatments attached can't be hard
            // deleted anyway (FK from Treatments), and cancelling preserves history.
            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand("sp_Appointment_Delete", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@p_AppointmentId", appointmentId);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.RowIsReferenced2)
            {
                throw new RepositoryConstraintException(
                    "This appointment has treatment records attached and can't be deleted. Cancel it instead.", ex);
            }
        }

        private static void AddAppointmentParameters(MySqlCommand cmd, Appointment appointment)
        {
            cmd.Parameters.AddWithValue("@p_PatientId", appointment.PatientId);
            cmd.Parameters.AddWithValue("@p_DentistId", appointment.DentistId);
            cmd.Parameters.AddWithValue("@p_AppointmentDateTime", appointment.AppointmentDateTime);
            cmd.Parameters.AddWithValue("@p_Status", appointment.Status);
            cmd.Parameters.AddWithValue("@p_Reason", (object?)appointment.Reason ?? DBNull.Value);
        }

        private static Appointment MapAppointment(MySqlDataReader reader) => new()
        {
            AppointmentId = (int)reader["AppointmentId"],
            PatientId = (int)reader["PatientId"],
            DentistId = (int)reader["DentistId"],
            AppointmentDateTime = (DateTime)reader["AppointmentDateTime"],
            Status = (string)reader["Status"],
            Reason = reader["Reason"] as string,
            CreatedAt = (DateTime)reader["CreatedAt"]
        };
    }
}
