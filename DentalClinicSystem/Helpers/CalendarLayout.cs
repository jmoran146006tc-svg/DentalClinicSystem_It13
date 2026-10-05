using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers
{
    public sealed record CalendarSlot(Appointment Appointment, int Column, int Columns);
    public static class CalendarLayout
    {
        public static IReadOnlyList<CalendarSlot> Arrange(IEnumerable<Appointment> appointments)
        {
            List<CalendarSlot> result = [];
            foreach (var day in appointments.GroupBy(a => a.AppointmentDateTime.Date))
            {
                List<(Appointment Appointment, int Column)> group = [];
                List<DateTime> ends = [];
                foreach (var appointment in day.OrderBy(a => a.AppointmentDateTime).ThenBy(a => a.AppointmentId))
                {
                    if (ends.Count > 0 && ends.All(end => end <= appointment.AppointmentDateTime))
                    {
                        result.AddRange(group.Select(item => new CalendarSlot(item.Appointment, item.Column, ends.Count)));
                        group.Clear();
                        ends.Clear();
                    }
                    var column = ends.FindIndex(end => end <= appointment.AppointmentDateTime);
                    if (column < 0) { column = ends.Count; ends.Add(DateTime.MinValue); }
                    ends[column] = appointment.AppointmentDateTime.AddMinutes(appointment.DurationMinutes);
                    group.Add((appointment, column));
                }
                result.AddRange(group.Select(item => new CalendarSlot(item.Appointment, item.Column, ends.Count)));
            }
            return result;
        }
    }
}
