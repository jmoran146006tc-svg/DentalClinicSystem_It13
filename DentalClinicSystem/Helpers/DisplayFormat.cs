using System.Globalization;
using DentalClinicSystem.Helpers.Design;

namespace DentalClinicSystem.Helpers;

public static class DisplayFormat
{
    public const string DatePattern = "MMM d, yyyy";
    public const string DateTimePattern = "MMM d, yyyy h:mm tt";
    public static string Currency(decimal amount) => $"₱{amount.ToString("N2", CultureInfo.InvariantCulture)}";
    public static string Time(DateTime date) => date.ToString("h:mm tt", CultureInfo.InvariantCulture);
    public static string Date(DateTime date) => date.ToString(DatePattern, CultureInfo.InvariantCulture);
    public static string DateTime(DateTime date) => date.ToString(DateTimePattern, CultureInfo.InvariantCulture);
    public static string ColumnDatePattern(string name) => name.Contains("Time", StringComparison.OrdinalIgnoreCase) ? DateTimePattern : DatePattern;
    public static string RelativeDate(DateTime date, DateTime today) => (date.Date - today.Date).Days switch
    {
        0 => "Today", 1 => "Tomorrow", -1 => "Yesterday", _ => date.ToString("ddd, MMM d", CultureInfo.InvariantCulture)
    };
    public static string Optional(string? value) => string.IsNullOrWhiteSpace(value) ? "n/a" : value;
    public static string Phone(string? value) => value is { Length: 11 } && value.StartsWith("09", StringComparison.Ordinal) && value.All(c => c is >= '0' and <= '9')
        ? $"{value[..4]} {value[4..7]} {value[7..]}" : Optional(value);
    public static string Initials(string? name)
    {
        var words = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length switch { 0 => "?", 1 => words[0][..1].ToUpperInvariant(), _ => $"{char.ToUpperInvariant(words[0][0])}{char.ToUpperInvariant(words[^1][0])}" };
    }
    public static SemanticStyle AvatarStyle(string? name)
    {
        uint hash = 2166136261;
        foreach (var c in (name ?? string.Empty).Trim().ToUpperInvariant()) hash = unchecked((hash ^ c) * 16777619);
        return Palette.AvatarColors[(int)(hash % Palette.AvatarColors.Count)];
    }
}
