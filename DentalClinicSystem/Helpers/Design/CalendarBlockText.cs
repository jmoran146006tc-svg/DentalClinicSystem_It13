using System.Globalization;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design;

public enum CalendarTextTier { Short, Medium, Tall }
public sealed record CalendarTextLine(string Text, Rectangle Bounds, bool Bold);

public static class CalendarBlockText
{
    public static CalendarTextTier Tier(int height, int lineHeight) => height >= Math.Max(1, lineHeight) * 3
        ? CalendarTextTier.Tall : height >= Math.Max(1, lineHeight) * 2 ? CalendarTextTier.Medium : CalendarTextTier.Short;

    public static string TimeRange(Appointment appointment) =>
        $"{appointment.AppointmentDateTime.ToString("h:mm", CultureInfo.InvariantCulture)}–{DisplayFormat.Time(appointment.AppointmentDateTime.AddMinutes(appointment.DurationMinutes))}";

    public static IReadOnlyList<CalendarTextLine> Lines(Appointment appointment, string patient, Rectangle block, int lineHeight, int dpi)
    {
        if (block.Width <= 0 || block.Height <= 0) return [];
        lineHeight = Math.Max(1, lineHeight);
        var inset = Math.Min(CalendarGeometry.Scale(Space.Sm, dpi), block.Width / 2);
        var content = new Rectangle(block.X + inset, block.Y, Math.Max(0, block.Width - inset * 2), block.Height);
        var tier = Tier(content.Height, lineHeight);
        string[] texts = tier switch
        {
            CalendarTextTier.Tall => [patient, TimeRange(appointment), DisplayFormat.Optional(appointment.Reason)],
            CalendarTextTier.Medium => [patient, TimeRange(appointment)],
            _ => [$"{appointment.AppointmentDateTime.ToString("h:mm", CultureInfo.InvariantCulture)} · {patient}"]
        };
        var height = Math.Min(lineHeight, content.Height);
        var top = content.Top + Math.Max(0, (content.Height - height * texts.Length) / 2);
        return texts.Select((text, index) => new CalendarTextLine(text,
            new(content.Left, top + index * height, content.Width, height), index == 0 && tier != CalendarTextTier.Short)).ToArray();
    }
}
