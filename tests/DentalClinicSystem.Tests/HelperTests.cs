using System.Globalization;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public sealed class HelperTests
{
    [Fact]
    public void RequiredContrastPairsMeetNormalTextThreshold()
    {
        foreach (var pair in Contrast.RequiredPairs()) Assert.True(Contrast.Ratio(pair.Text, pair.Background) >= 4.5, pair.Name);
    }
    [Theory]
    [InlineData("en-US")] [InlineData("fr-FR")] [InlineData("ar-SA")]
    public void DisplayFormattingIsDeterministic(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture); var day = new DateTime(2026, 10, 5, 13, 5, 0);
            Assert.Equal("₱1,250.50", DisplayFormat.Currency(1250.5m)); Assert.Equal("Oct 5, 2026", DisplayFormat.Date(day));
            Assert.Equal("1:05 PM", DisplayFormat.Time(day)); Assert.Equal("Today", DisplayFormat.RelativeDate(day, day));
            Assert.Equal("Tomorrow", DisplayFormat.RelativeDate(day.AddDays(1), day)); Assert.Equal("Yesterday", DisplayFormat.RelativeDate(day.AddDays(-1), day));
            Assert.Equal("0917 123 4567", DisplayFormat.Phone("09171234567")); Assert.Equal("n/a", DisplayFormat.Optional(" "));
            Assert.Equal("AS", DisplayFormat.Initials("  Ana Maria Santos ")); Assert.Equal("?", DisplayFormat.Initials(null));
            Assert.Equal(DisplayFormat.AvatarStyle("ana santos"), DisplayFormat.AvatarStyle(" ANA SANTOS "));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
    [Fact]
    public void FiltersUseCaseInsensitiveSearchInactiveRulesAndRoleScope()
    {
        Patient[] patients = [new() { PatientId = 1, FirstName = "Ana", LastName = "Santos", IsActive = true }, new() { PatientId = 2, FirstName = "Ana", IsActive = false }];
        Assert.Equal(1, Assert.Single(PatientFilter.Apply(patients, " ANA ", false)).PatientId); Assert.Equal(2, PatientFilter.Apply(patients, "ana", true).Count());
        var day = new DateTime(2026, 10, 4);
        Appointment[] rows = [new() { AppointmentId = 1, DentistId = 2, AppointmentDateTime = day, Status = AppointmentStatus.CheckedIn }, new() { AppointmentId = 2, DentistId = 3, AppointmentDateTime = day.AddDays(1) }];
        var result = AppointmentFilter.Apply(rows, new() { Role = Roles.Dentist, DentistId = 2 }, "ANA", null, AppointmentDateFilter.ThisWeek, day, _ => "Ana Santos");
        Assert.Equal(1, Assert.Single(result).AppointmentId); Assert.Contains("Checked in", AppointmentLabels.Format(rows[0], "Ana Santos"));
    }
    [Fact]
    public void DashboardCountsWorklistAndWeekBoundariesAreStable()
    {
        var sunday = new DateTime(2026, 10, 4); var monday = DashboardPresentation.WeekStart(sunday);
        Assert.Equal(new DateTime(2026, 9, 28), monday); Assert.Equal(new DateTime(2025, 12, 29), DashboardPresentation.WeekStart(new(2026, 1, 1)));
        Assert.Equal("Good morning, Ana", DashboardPresentation.Greeting("Ana Santos", sunday.AddHours(11)));
        Assert.Equal("afternoon", DashboardPresentation.GreetingPeriod(sunday.AddHours(12))); Assert.Equal("evening", DashboardPresentation.GreetingPeriod(sunday.AddHours(18)));
        Appointment[] appointments = [new() { AppointmentId = 1, DentistId = 2, AppointmentDateTime = sunday.AddHours(10), Status = AppointmentStatus.Cancelled },
            new() { AppointmentId = 2, DentistId = 2, AppointmentDateTime = sunday.AddHours(11), Status = AppointmentStatus.Scheduled },
            new() { AppointmentId = 3, DentistId = 3, AppointmentDateTime = sunday.AddDays(1), Status = AppointmentStatus.Cancelled }];
        var counts = DashboardPresentation.Counts(appointments, [new() { IsActive = true }, new() { IsActive = false }], sunday);
        Assert.Equal(new DashboardCounts(2, 1, 1), counts);
        var details = appointments.Select(row => new AppointmentDetails(row, new(), new())).ToArray();
        Assert.Equal(2, DashboardPresentation.Worklist(details, 2, sunday).Count); Assert.Equal(2, DashboardPresentation.NextAppointment(details, sunday.AddHours(10)));
    }
    [Fact]
    public void CalendarGeometrySupportsDpiOverlapAndZeroSizes()
    {
        var day = new DateTime(2026, 10, 5);
        Appointment[] appointments = [new() { AppointmentId = 1, AppointmentDateTime = day.AddHours(9), DurationMinutes = 90 }, new() { AppointmentId = 2, AppointmentDateTime = day.AddHours(10), DurationMinutes = 30 }];
        var geometry = CalendarGeometry.ForWeek(appointments, day);
        Assert.Equal(ClinicRules.OpeningTime.Hour, geometry.StartHour); Assert.Equal(ClinicRules.ClosingTime.Hour, geometry.EndHour);
        Assert.Empty(geometry.Blocks(appointments, day, 0, 96));
        foreach (var dpi in new[] { 96, 120, 144 })
        {
            var blocks = geometry.Blocks(appointments, day, CalendarGeometry.MinimumWidth(dpi), dpi);
            Assert.Equal(2, blocks.Count); Assert.All(blocks, block => Assert.True(block.Bounds.Width > 0 && block.Bounds.Height > 0));
            Assert.False(blocks[0].Bounds.IntersectsWith(blocks[1].Bounds));
            Assert.Equal(CalendarGeometry.Scale(Metrics.CalendarHourHeight * 8, dpi), geometry.Height(dpi));
        }
        var expanded = CalendarGeometry.ForWeek([new() { AppointmentDateTime = day.AddHours(7.5), DurationMinutes = 90 },
            new() { AppointmentDateTime = day.AddHours(17.5), DurationMinutes = 90 },
            new() { AppointmentDateTime = day.AddDays(8).AddHours(5), DurationMinutes = 30 }], day);
        Assert.Equal(new CalendarGeometry(7, 19), expanded);
    }
    [Fact]
    public void EveryStatusHasSafeStyleAndEveryIconBuildsGeometry()
    {
        SemanticStyle[] styles = [Palette.Info, Palette.Warning, Palette.Success, Palette.Neutral, Palette.Danger];
        for (var i = 0; i < AppointmentStatus.All.Length; i++) Assert.Equal(styles[i], Theme.StatusStyle(AppointmentStatus.All[i]));
        Assert.Equal(Palette.Neutral, Theme.StatusStyle("Unknown"));
        foreach (var icon in Enum.GetValues<IconKind>()) { using var path = Icons.Build(icon); Assert.True(path.PointCount > 0, icon.ToString()); }
    }
    [Theory]
    [InlineData("0", true)] [InlineData("99999999.99", true)] [InlineData("100000000", false)] [InlineData("-1", false)] [InlineData("1,23", false)]
    public void CostsAndLengthsRespectServiceBoundaries(string text, bool expected)
    {
        Assert.Equal(expected, InputRules.TryCost(text, out _));
        Assert.True(Validator.MaxLength(new string('a', FieldLimits.Name), FieldLimits.Name, "Name").Success);
        Assert.False(Validator.MaxLength(new string('a', FieldLimits.Name + 1), FieldLimits.Name, "Name").Success);
        Assert.Equal("Ana Santos", Validator.Sanitize("  Ana   Santos  ")); Assert.Null(Validator.Optional(" "));
    }
}
