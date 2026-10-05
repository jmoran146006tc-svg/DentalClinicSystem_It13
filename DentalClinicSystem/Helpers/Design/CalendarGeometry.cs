using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design;

public sealed record CalendarBlock(Appointment Appointment, Rectangle Bounds);

public static class CalendarGeometry
{
    public const int StartHour = 8, EndHour = 18;
    public static int Height(int dpi) => Scale(Metrics.CalendarHeaderHeight + (EndHour - StartHour) * Metrics.CalendarHourHeight + Space.Xl, dpi);
    public static int MinimumWidth(int dpi) => Scale(Metrics.CalendarGutter + DashboardPresentation.DaysInWeek * Metrics.CalendarDayWidth, dpi);
    public static int Scale(int value, int dpi) => (int)Math.Round(value * dpi / (double)Metrics.BaselineDpi);
    public static IReadOnlyList<CalendarBlock> Blocks(IEnumerable<Appointment> appointments, DateTime monday, int width, int dpi)
    {
        var gutter = Scale(Metrics.CalendarGutter, dpi);
        if (width <= gutter) return [];
        var columnWidth = (width - gutter) / (double)DashboardPresentation.DaysInWeek;
        var header = Scale(Metrics.CalendarHeaderHeight, dpi);
        var hour = Scale(Metrics.CalendarHourHeight, dpi);
        var gap = Scale(Space.Xs, dpi);
        var visible = appointments.Where(a => a.DurationMinutes > 0 && a.AppointmentDateTime >= monday.Date &&
            a.AppointmentDateTime < monday.Date.AddDays(DashboardPresentation.DaysInWeek));
        return CalendarLayout.Arrange(visible).Select(slot =>
        {
            var start = Math.Clamp(slot.Appointment.AppointmentDateTime.TimeOfDay.TotalHours, StartHour, EndHour);
            var end = Math.Clamp(slot.Appointment.AppointmentDateTime.TimeOfDay.TotalHours + slot.Appointment.DurationMinutes / 60d, StartHour, EndHour);
            var day = (slot.Appointment.AppointmentDateTime.Date - monday.Date).Days;
            var left = gutter + day * columnWidth + slot.Column * columnWidth / slot.Columns;
            var right = gutter + day * columnWidth + (slot.Column + 1) * columnWidth / slot.Columns;
            return new CalendarBlock(slot.Appointment, new((int)Math.Round(left) + gap, header + (int)Math.Round((start - StartHour) * hour) + gap,
                Math.Max(0, (int)Math.Round(right) - (int)Math.Round(left) - gap * 2), Math.Max(0, (int)Math.Round((end - start) * hour) - gap)));
        }).Where(block => block.Bounds.Width > 0 && block.Bounds.Height > 0).ToArray();
    }
}
