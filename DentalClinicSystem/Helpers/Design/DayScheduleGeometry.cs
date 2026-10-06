using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design;

public enum CalendarView { Day, Week }
public sealed record CalendarColumn(DateTime Date, int DentistId, string Title, int Count);

public static class DayScheduleGeometry
{
    public static IReadOnlyList<CalendarColumn> Columns(DateTime day, IEnumerable<Appointment> appointments,
        IReadOnlyDictionary<int, string> dentists, IReadOnlySet<int>? activeDentistIds = null, int selectedDentist = 0)
    {
        var rows = appointments.Where(a => a.AppointmentDateTime.Date == day.Date).ToArray();
        var ids = selectedDentist != 0 ? new[] { selectedDentist } :
            (activeDentistIds ?? dentists.Keys.ToHashSet()).Concat(rows.Select(a => a.DentistId)).Distinct();
        return ids.Select(id => new CalendarColumn(day.Date, id, dentists.GetValueOrDefault(id, $"Dentist #{id}"), rows.Count(a => a.DentistId == id)))
            .OrderBy(column => column.Title, StringComparer.OrdinalIgnoreCase).ThenBy(column => column.DentistId).ToArray();
    }
    public static int Width(int available, int count, int dpi) => Math.Max(available,
        CalendarGeometry.Scale(Metrics.CalendarGutter + Math.Max(1, count) * Metrics.CalendarDentistColumnWidth, dpi));

    public static IReadOnlyList<CalendarBlock> Blocks(CalendarGeometry range, IEnumerable<Appointment> appointments,
        IReadOnlyList<CalendarColumn> columns, int width, int dpi, int hourHeight)
    {
        var gutter = CalendarGeometry.Scale(Metrics.CalendarGutter, dpi);
        if (columns.Count == 0 || width <= gutter || range.EndHour <= range.StartHour) return [];
        var columnWidth = (width - gutter) / (double)columns.Count;
        return columns.SelectMany((column, index) => CalendarLayout.Arrange(appointments.Where(a => a.DurationMinutes > 0 &&
                a.DentistId == column.DentistId && a.AppointmentDateTime.Date == column.Date))
            .Select(slot => range.Place(slot, index, columnWidth, gutter, dpi, hourHeight))).Where(b => b.Bounds.Width > 0 && b.Bounds.Height > 0).ToArray();
    }
}
