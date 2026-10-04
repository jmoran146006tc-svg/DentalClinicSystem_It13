namespace DentalClinicSystem.Tests;

internal sealed class FixedTimeProvider(DateTime localNow) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
    public override DateTimeOffset GetUtcNow() => new(DateTime.SpecifyKind(localNow, DateTimeKind.Utc));
}
