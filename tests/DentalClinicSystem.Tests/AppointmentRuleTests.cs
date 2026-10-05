using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class AppointmentRuleTests
{
    private static readonly DateTime Now = new(2026, 10, 5, 10, 0, 0);
    [Theory]
    [InlineData(15, true)]
    [InlineData(16, false)]
    public void WalkInGraceHasInclusiveBoundary(int minutes, bool expected) =>
        Assert.Equal(expected, Validator.Appointment(new() { AppointmentDateTime = Now.AddMinutes(-minutes) }, Now).Success);

    [Fact]
    public void EveryStatusPairUsesTheTransitionTable()
    {
        foreach (var from in AppointmentStatus.All)
        foreach (var to in AppointmentStatus.All)
        {
            var allowed = from == AppointmentStatus.Scheduled && to != AppointmentStatus.Scheduled ||
                from == AppointmentStatus.CheckedIn && to == AppointmentStatus.Completed;
            Assert.Equal(allowed, AppointmentStatus.CanTransition(from, to));
            Assert.Equal(allowed, Validator.StatusChange(from, to, "Reason", Now, Now).Success);
        }
        Assert.False(AppointmentStatus.CanTransition("Unknown", AppointmentStatus.Completed));
    }
    [Theory]
    [InlineData(AppointmentStatus.Completed, 1, false)]
    [InlineData(AppointmentStatus.Completed, -1, true)]
    [InlineData(AppointmentStatus.CheckedIn, 1, false)]
    [InlineData(AppointmentStatus.CheckedIn, -1, false)]
    [InlineData(AppointmentStatus.CheckedIn, 0, true)]
    public void StatusDatesAreEnforced(string status, int days, bool expected) =>
        Assert.Equal(expected, Validator.StatusChange(AppointmentStatus.Scheduled, status, null, Now.AddDays(days), Now).Success);

    [Fact]
    public void NoShowWaitsUntilStartAndCancellationNeedsReason()
    {
        Assert.False(Validator.StatusChange(AppointmentStatus.Scheduled, AppointmentStatus.NoShow, null, Now.AddTicks(1), Now).Success);
        Assert.True(Validator.StatusChange(AppointmentStatus.Scheduled, AppointmentStatus.NoShow, null, Now, Now).Success);
        Assert.False(Validator.StatusChange(AppointmentStatus.Scheduled, AppointmentStatus.Cancelled, " ", Now, Now).Success);
    }
    [Theory]
    [InlineData(Roles.Admin, true)]
    [InlineData(Roles.Receptionist, true)]
    [InlineData(Roles.Dentist, false)]
    public void CheckInPermissions(string role, bool expected) =>
        Assert.Equal(expected, RoleAccess.CanChangeStatus(new() { Role = role, DentistId = 1 }, new() { DentistId = 1 }, AppointmentStatus.CheckedIn));

    [Fact]
    public void FixedClockSuppliesLocalDate() => Assert.Equal(Now, new FixedTimeProvider(Now).GetLocalNow().DateTime);
}
