namespace DentalClinicSystem.Service;

public static class ToothNumbering
{
    // FDI: first digit is the quadrant, second is the tooth within that quadrant.
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        value = value.Trim();
        if (value.Length != 2 || value[1] < '1') return false;
        return value[0] is >= '1' and <= '4' && value[1] <= '8' ||
            value[0] is >= '5' and <= '8' && value[1] <= '5';
    }
}
