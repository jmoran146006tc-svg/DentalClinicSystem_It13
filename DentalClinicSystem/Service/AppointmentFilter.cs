using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service;

public enum AppointmentDateFilter { All, Today, ThisWeek }
public static class AppointmentFilter
{
    public static IEnumerable<Appointment> Apply(IEnumerable<Appointment> rows, User actor, string search, string? status,
        AppointmentDateFilter dates, DateTime today, Func<int, string> patientName)
    {
        var start = today.Date;
        if (dates == AppointmentDateFilter.ThisWeek) start = start.AddDays(-((int)start.DayOfWeek + 6) % 7);
        var end = start.AddDays(dates == AppointmentDateFilter.ThisWeek ? 7 : 1);
        return rows.Where(a => RoleAccess.CanAccessAppointment(actor, a)
            && patientName(a.PatientId).Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrEmpty(status) || a.Status == status)
            && (dates == AppointmentDateFilter.All || a.AppointmentDateTime >= start && a.AppointmentDateTime < end));
    }
}
