using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers.Design;

public sealed record CalendarBlock(Appointment Appointment, Rectangle Bounds);

public sealed record CalendarGeometry(int StartHour, int EndHour)
{
    public int Height(int dpi) => Scale((EndHour - StartHour) * Metrics.CalendarHourHeight, dpi);
    public static int MinimumWidth(int dpi) => Scale(Metrics.CalendarGutter + DashboardPresentation.DaysInWeek * Metrics.CalendarDayWidth, dpi);
    public static int Scale(int value, int dpi) => (int)Math.Round(value * dpi / (double)Metrics.BaselineDpi);
    public static CalendarGeometry ForWeek(IEnumerable<Appointment> appointments, DateTime monday)
    {
        var start = (int)Math.Floor(ClinicRules.OpeningTime.ToTimeSpan().TotalHours);
        var end = (int)Math.Ceiling(ClinicRules.ClosingTime.ToTimeSpan().TotalHours);
        foreach (var appointment in appointments.Where(a => a.DurationMinutes > 0 && a.AppointmentDateTime >= monday.Date &&
            a.AppointmentDateTime < monday.Date.AddDays(DashboardPresentation.DaysInWeek)))
        {
            start = Math.Min(start, (int)Math.Floor(appointment.AppointmentDateTime.TimeOfDay.TotalHours));
            end = Math.Max(end, Math.Min(24, (int)Math.Ceiling(appointment.AppointmentDateTime.TimeOfDay.TotalHours + appointment.DurationMinutes / 60d)));
        }
        return new(start, end);
    }
    public IReadOnlyList<CalendarBlock> Blocks(IEnumerable<Appointment> appointments, DateTime monday, int width, int dpi)
        => Blocks(appointments, Enumerable.Range(0, DashboardPresentation.DaysInWeek).Select(i => monday.Date.AddDays(i)).ToArray(), width, dpi, Scale(Metrics.CalendarHourHeight, dpi));

    public IReadOnlyList<CalendarBlock> Blocks(IEnumerable<Appointment> appointments, IReadOnlyList<DateTime> days, int width, int dpi, int hourHeight)
    {
        var gutter = Scale(Metrics.CalendarGutter, dpi);
        if (days.Count == 0 || width <= gutter || EndHour <= StartHour) return [];
        var columnWidth = (width - gutter) / (double)days.Count;
        return days.SelectMany((day, index) => CalendarLayout.Arrange(appointments.Where(a => a.DurationMinutes > 0 && a.AppointmentDateTime.Date == day))
            .Select(slot => Place(slot, index, columnWidth, gutter, dpi, hourHeight))).Where(b => b.Bounds.Width > 0 && b.Bounds.Height > 0).ToArray();
    }

    internal CalendarBlock Place(CalendarSlot slot, int column, double columnWidth, int gutter, int dpi, int hourHeight)
    {
        var start = Math.Clamp(slot.Appointment.AppointmentDateTime.TimeOfDay.TotalHours, StartHour, EndHour);
        var end = Math.Clamp(slot.Appointment.AppointmentDateTime.TimeOfDay.TotalHours + slot.Appointment.DurationMinutes / 60d, StartHour, EndHour);
        var left = gutter + column * columnWidth + slot.Column * columnWidth / slot.Columns;
        var right = gutter + column * columnWidth + (slot.Column + 1) * columnWidth / slot.Columns;
        var gap = Scale(Space.Xs, dpi);
        return new(slot.Appointment, new((int)Math.Round(left) + gap, (int)Math.Round((start - StartHour) * hourHeight),
            Math.Max(0, (int)Math.Round(right) - (int)Math.Round(left) - gap * 2), Math.Max(0, (int)Math.Round((end - start) * hourHeight))));
    }
}
