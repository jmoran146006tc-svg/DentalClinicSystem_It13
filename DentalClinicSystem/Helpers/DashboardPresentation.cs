using System.Globalization;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers;

public sealed record DashboardCounts(int TodayAppointments, int ActivePatients, int WeekCancellations);

public static class DashboardPresentation
{
    public const int DaysInWeek = 7;
    public static DateTime WeekStart(DateTime date) => date.Date.AddDays(-((int)date.DayOfWeek + 6) % DaysInWeek);
    public static string GreetingPeriod(DateTime now) => now.Hour < 12 ? "morning" : now.Hour < 18 ? "afternoon" : "evening";
    public static string Greeting(string name, DateTime now)
    {
        var first = name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "there";
        var period = GreetingPeriod(now);
        return $"Good {period}, {first}";
    }
    public static string TodayLabel(DateTime today) => today.ToString("dddd, MMMM d, yyyy", CultureInfo.InvariantCulture);
    public static string WeekLabel(DateTime monday) => $"{DisplayFormat.Date(monday)} – {DisplayFormat.Date(monday.AddDays(6))}";
    public static DashboardCounts Counts(IEnumerable<Appointment> appointments, IEnumerable<Patient> patients, DateTime today)
    {
        var week = WeekStart(today);
        var rows = appointments.Where(a => a.AppointmentDateTime >= week && a.AppointmentDateTime < week.AddDays(DaysInWeek)).ToArray();
        return new(rows.Count(a => a.AppointmentDateTime.Date == today.Date), patients.Count(p => p.IsActive),
            rows.Count(a => a.Status == AppointmentStatus.Cancelled));
    }
    public static IReadOnlyList<AppointmentDetails> Worklist(IEnumerable<AppointmentDetails> details, int dentistId, DateTime today) =>
        details.Where(d => d.Appointment.DentistId == dentistId && d.Appointment.AppointmentDateTime.Date == today.Date)
            .OrderBy(d => d.Appointment.AppointmentDateTime).ThenBy(d => d.Appointment.AppointmentId).ToArray();
    public static int? NextAppointment(IEnumerable<AppointmentDetails> details, DateTime now) => details
        .Where(d => d.Appointment.Status == AppointmentStatus.Scheduled && d.Appointment.AppointmentDateTime >= now)
        .OrderBy(d => d.Appointment.AppointmentDateTime).ThenBy(d => d.Appointment.AppointmentId)
        .Select(d => (int?)d.Appointment.AppointmentId).FirstOrDefault();
}
