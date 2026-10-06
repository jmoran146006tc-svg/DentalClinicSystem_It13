using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers.Design;

public sealed record WeekSummaryCell(int DentistId, DateTime Date, IReadOnlyDictionary<string, int> StatusCounts)
{
    public int Count => StatusCounts.Values.Sum();
}
public sealed record WeekSummary(IReadOnlyList<DateTime> Days, IReadOnlyList<CalendarColumn> Dentists, IReadOnlyList<WeekSummaryCell> Cells)
{
    public static IReadOnlyList<DateTime> VisibleDays(DateTime monday, IEnumerable<Appointment> appointments)
    {
        var occupied = appointments.Select(a => a.AppointmentDateTime.Date).ToHashSet();
        return Enumerable.Range(0, DashboardPresentation.DaysInWeek).Select(i => monday.Date.AddDays(i))
            .Where(day => !ClinicRules.ClosedDays.Contains(day.DayOfWeek) || occupied.Contains(day)).ToArray();
    }
    public static WeekSummary Create(DateTime monday, IEnumerable<Appointment> appointments,
        IReadOnlyDictionary<int, string> dentists, IReadOnlySet<int>? activeDentistIds = null)
    {
        var rows = appointments.Where(a => a.AppointmentDateTime >= monday.Date && a.AppointmentDateTime < monday.Date.AddDays(7)).ToArray();
        var days = VisibleDays(monday, rows);
        var ids = (activeDentistIds ?? dentists.Keys.ToHashSet()).Concat(rows.Select(a => a.DentistId)).ToHashSet();
        var columns = ids.Select(id => new CalendarColumn(monday.Date, id, dentists.GetValueOrDefault(id, $"Dentist #{id}"), rows.Count(a => a.DentistId == id)))
            .OrderBy(c => c.Title, StringComparer.OrdinalIgnoreCase).ThenBy(c => c.DentistId).ToArray();
        var cells = columns.SelectMany(d => days.Select(day => new WeekSummaryCell(d.DentistId, day,
            rows.Where(a => a.DentistId == d.DentistId && a.AppointmentDateTime.Date == day)
                .GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count())))).ToArray();
        return new(days, columns, cells);
    }
}
