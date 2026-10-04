namespace DentalClinicSystem.Service;

public static class ClinicRules
{
    public const int PastGraceMinutes = 15;
    // Assumed single-clinic hours; confirm with the clinic before use.
    public static readonly TimeOnly OpeningTime = new(9, 0), ClosingTime = new(17, 0);
    public static IReadOnlySet<DayOfWeek> ClosedDays { get; } = new HashSet<DayOfWeek> { DayOfWeek.Sunday };
    public const int DefaultDurationMinutes = 30, MinDurationMinutes = 15, MaxDurationMinutes = 240, DurationStepMinutes = 15;
    public const string ConsultationReason = "Consultation / Check-up";
    public const string TimeOffSuffix = " is on leave on that date.";
    public static IEnumerable<int> Durations => Enumerable.Range(0, (MaxDurationMinutes - MinDurationMinutes) / DurationStepMinutes + 1)
        .Select(i => MinDurationMinutes + i * DurationStepMinutes);
    public static bool IsOpen(DateTime start, int durationMinutes)
    {
        if (durationMinutes < MinDurationMinutes || durationMinutes > MaxDurationMinutes || ClosedDays.Contains(start.DayOfWeek)) return false;
        var end = start.AddMinutes(durationMinutes);
        return end.Date == start.Date && TimeOnly.FromDateTime(start) >= OpeningTime && TimeOnly.FromDateTime(end) <= ClosingTime;
    }
    public static string HoursDescription
    {
        get
        {
            var days = Enumerable.Range(0, 7).Select(i => (DayOfWeek)((i + 1) % 7)).Where(d => !ClosedDays.Contains(d)).ToArray();
            var names = days.Select(d => System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedDayName(d));
            var contiguous = days.Length > 1 && days.Zip(days.Skip(1)).All(pair => ((int)pair.Second + 7 - (int)pair.First) % 7 == 1);
            var dayText = contiguous ? $"{names.First()} to {names.Last()}" : string.Join(", ", names);
            return $"{dayText}, {OpeningTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture)} to {ClosingTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture)}";
        }
    }
}
