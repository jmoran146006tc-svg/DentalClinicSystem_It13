namespace DentalClinicSystem.Helpers;

public static class RecordSearch
{
    public static bool Matches(string? query, params string?[] values)
    {
        var search = query?.Trim() ?? "";
        return values.Any(value => (value ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));
    }
}
