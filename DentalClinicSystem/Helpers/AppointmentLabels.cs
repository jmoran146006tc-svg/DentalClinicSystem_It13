using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers
{
    public static class AppointmentLabels
    {
        public const string DateTimeFormat = "MM/dd/yyyy hh:mm tt";
        public const string ClockFormat = "dddd, dd MMM yyyy HH:mm:ss";
        public static string Format(Appointment appointment, string patient) =>
            $"#{appointment.AppointmentId} - {patient} - {appointment.AppointmentDateTime.ToString(DateTimeFormat)} ({appointment.Status})";
    }
}
