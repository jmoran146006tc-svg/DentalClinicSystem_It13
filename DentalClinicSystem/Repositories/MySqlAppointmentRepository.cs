using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;
using MySqlConnector;

namespace DentalClinicSystem.Repositories
{
    public class MySqlAppointmentRepository : IAppointmentRepository
    {
        private const string SelectColumns =
            "SELECT AppointmentId, PatientId, DentistId, AppointmentDateTime, Status, Reason, CreatedAt FROM Appointments";

        public async Task<IReadOnlyList<Appointment>> GetAllAsync()
        {
            List<Appointment> appointments = [];

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand($"{SelectColumns} ORDER BY AppointmentDateTime", conn);

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
            await using var cmd = new MySqlCommand($"{SelectColumns} WHERE AppointmentId = @id", conn);
            cmd.Parameters.AddWithValue("@id", appointmentId);

            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapAppointment(reader) : null;
        }

        public async Task<IReadOnlyList<Appointment>> GetByDentistAndDateAsync(int dentistId, DateTime date)
        {
            List<Appointment> appointments = [];
            var startOfDay = date.Date;
            var startOfNextDay = startOfDay.AddDays(1);

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(
                $"{SelectColumns} WHERE DentistId = @dentistId " +
                "AND AppointmentDateTime >= @start AND AppointmentDateTime < @end " +
                "AND Status != 'Cancelled'", conn);
            cmd.Parameters.AddWithValue("@dentistId", dentistId);
            cmd.Parameters.AddWithValue("@start", startOfDay);
            cmd.Parameters.AddWithValue("@end", startOfNextDay);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                appointments.Add(MapAppointment(reader));
            }

            return appointments;
        }

        public async Task AddAsync(Appointment appointment)
        {
            const string sql =
                "INSERT INTO Appointments (PatientId, DentistId, AppointmentDateTime, Status, Reason) " +
                "VALUES (@pid, @did, @dt, @status, @reason)";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddAppointmentParameters(cmd, appointment);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            const string sql =
                "UPDATE Appointments SET PatientId = @pid, DentistId = @did, AppointmentDateTime = @dt, " +
                "Status = @status, Reason = @reason WHERE AppointmentId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            AddAppointmentParameters(cmd, appointment);
            cmd.Parameters.AddWithValue("@id", appointment.AppointmentId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int appointmentId)
        {
            // Prefer AppointmentService.UpdateStatusAsync(id, "Cancelled") over calling
            // this directly - an appointment with treatments attached can't be hard
            // deleted anyway (FK from Treatments), and cancelling preserves history.
            const string sql = "DELETE FROM Appointments WHERE AppointmentId = @id";

            await using var conn = await DbConnectionHelper.GetOpenConnectionAsync();
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", appointmentId);

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
            cmd.Parameters.AddWithValue("@pid", appointment.PatientId);
            cmd.Parameters.AddWithValue("@did", appointment.DentistId);
            cmd.Parameters.AddWithValue("@dt", appointment.AppointmentDateTime);
            cmd.Parameters.AddWithValue("@status", appointment.Status);
            cmd.Parameters.AddWithValue("@reason", (object?)appointment.Reason ?? DBNull.Value);
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
