using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers
{
    public static class AppointmentLabels
    {
        public const string DateTimeFormat = DisplayFormat.DateTimePattern;
        public const string ClockFormat = DisplayFormat.DateTimePattern;
        public static string Format(Appointment appointment, string patient) =>
            $"#{appointment.AppointmentId} - {patient} - {DisplayFormat.DateTime(appointment.AppointmentDateTime)} ({AppointmentStatus.Display(appointment.Status)})";
    }
}
