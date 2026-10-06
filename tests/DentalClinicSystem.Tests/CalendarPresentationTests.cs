using System.Drawing;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Tests;

public sealed class CalendarPresentationTests
{
    private static readonly DateTime Monday = new(2026, 10, 5);
    private static Appointment Visit(int id, int dentist, double hour, int duration = 45, int day = 0, string status = AppointmentStatus.Scheduled) =>
        new() { AppointmentId = id, DentistId = dentist, AppointmentDateTime = Monday.AddDays(day).AddHours(hour), DurationMinutes = duration, Status = status };
    [Fact]
    public void WeekLabelIsCompactAndKeepsBothYearsAtYearBoundary()
    {
        Assert.Equal("Oct 5 – Oct 11, 2026", DashboardPresentation.WeekLabel(Monday));
        Assert.Equal("Dec 29, 2025 – Jan 4, 2026", DashboardPresentation.WeekLabel(new(2025, 12, 29)));
    }
    [Theory]
    [InlineData(96)] [InlineData(120)] [InlineData(144)]
    public void DayColumnsIncludeActiveAndBookedDentistsWithIsolatedOverlapLanes(int dpi)
    {
        var rows = new[] { Visit(1, 1, 9, 90), Visit(2, 1, 10), Visit(3, 2, 9), Visit(4, 3, 9), Visit(5, 4, 9, day: 1) };
        var names = new Dictionary<int, string> { [1] = "Dr. A", [2] = "Dr. B", [3] = "Dr. Inactive", [4] = "Dr. Other day" };
        var columns = DayScheduleGeometry.Columns(Monday, rows, names, new HashSet<int> { 1, 2 });
        Assert.Equal(new[] { 1, 2, 3 }, columns.Select(c => c.DentistId));
        var width = DayScheduleGeometry.Width(0, columns.Count, dpi);
        var gutter = CalendarGeometry.Scale(Metrics.CalendarGutter, dpi);
        Assert.True((width - gutter) / columns.Count >= CalendarGeometry.Scale(Metrics.CalendarDentistColumnWidth, dpi));
        var blocks = DayScheduleGeometry.Blocks(new(9, 17), rows, columns, width, dpi, CalendarGeometry.Scale(Metrics.CalendarDayHourHeight, dpi));
        Assert.Equal(4, blocks.Count);
        for (var i = 0; i < blocks.Count; i++)
        {
            var column = columns.ToList().FindIndex(c => c.DentistId == blocks[i].Appointment.DentistId);
            var columnWidth = (width - gutter) / (double)columns.Count;
            Assert.True(blocks[i].Bounds.Left >= gutter + column * columnWidth);
            Assert.True(blocks[i].Bounds.Right <= gutter + (column + 1) * columnWidth);
            for (var j = i + 1; j < blocks.Count; j++) Assert.False(blocks[i].Bounds.IntersectsWith(blocks[j].Bounds));
        }
        Assert.Equal(Assert.Single(blocks, b => b.Appointment.DentistId == 2).Bounds.Width,
            Assert.Single(blocks, b => b.Appointment.DentistId == 3).Bounds.Width);
        Assert.Single(DayScheduleGeometry.Columns(Monday, rows, names, selectedDentist: 2));
        Assert.Empty(DayScheduleGeometry.Blocks(new(9, 17), rows, columns, 0, dpi, 80));
    }
    [Theory]
    [InlineData(96, 16)] [InlineData(120, 20)] [InlineData(144, 24)]
    public void QuarterHourKeepsDurationAndFitsOneTextLine(int dpi, int lineHeight)
    {
        var row = Visit(1, 1, 9, 15);
        var columns = new[] { new CalendarColumn(Monday, 1, "Dr. A", 1) };
        var block = Assert.Single(DayScheduleGeometry.Blocks(new(9, 17), [row], columns, DayScheduleGeometry.Width(0, 1, dpi), dpi,
            CalendarGeometry.Scale(Metrics.CalendarDayHourHeight, dpi)));
        Assert.True(block.Bounds.Height >= lineHeight);
        Assert.InRange(block.Bounds.Height, CalendarGeometry.Scale(20, dpi) - CalendarGeometry.Scale(Metrics.Border, dpi), CalendarGeometry.Scale(20, dpi));
        var text = Assert.Single(CalendarBlockText.Lines(row, "Mark Gonzales", block.Bounds, lineHeight, dpi));
        Assert.Equal("9:00 · Mark Gonzales", text.Text); Assert.True(block.Bounds.Contains(text.Bounds));
    }
    [Theory]
    [InlineData(0, CalendarTextTier.Short)] [InlineData(15, CalendarTextTier.Short)] [InlineData(16, CalendarTextTier.Short)]
    [InlineData(31, CalendarTextTier.Short)] [InlineData(32, CalendarTextTier.Medium)] [InlineData(47, CalendarTextTier.Medium)]
    [InlineData(48, CalendarTextTier.Tall)]
    public void TextTierBoundaries(int height, CalendarTextTier expected) => Assert.Equal(expected, CalendarBlockText.Tier(height, 16));
    [Theory]
    [InlineData(10)] [InlineData(19)] [InlineData(32)] [InlineData(60)]
    public void EveryTextLineStaysInsideBlock(int height)
    {
        var block = new Rectangle(20, 30, 200, height);
        Assert.All(CalendarBlockText.Lines(Visit(1, 1, 9), "Alyssa Rivera", block, 16, 96), line => Assert.True(block.Contains(line.Bounds)));
    }
    [Fact]
    public void SummaryCountsStatusesAndShowsClosedDaysOnlyWhenBooked()
    {
        var names = new Dictionary<int, string> { [1] = "Dr. A", [2] = "Dr. B", [3] = "Dr. Inactive" };
        var rows = new[] { Visit(1, 1, 9), Visit(2, 1, 10, status: AppointmentStatus.Completed), Visit(3, 3, 9, day: 2), Visit(4, 1, 9, day: 8) };
        var summary = WeekSummary.Create(Monday, rows, names, new HashSet<int> { 1, 2 });
        Assert.Equal(6, summary.Days.Count); Assert.DoesNotContain(Monday.AddDays(6), summary.Days);
        Assert.Equal(3, summary.Dentists.Count);
        var cell = Assert.Single(summary.Cells, c => c.DentistId == 1 && c.Date == Monday);
        Assert.Equal(2, cell.Count); Assert.Equal(1, cell.StatusCounts[AppointmentStatus.Scheduled]); Assert.Equal(1, cell.StatusCounts[AppointmentStatus.Completed]);
        Assert.All(summary.Cells.Where(c => c.DentistId == 2), c => Assert.Equal(0, c.Count));
        var sunday = WeekSummary.Create(Monday, rows.Append(Visit(5, 1, 9, day: 6)), names);
        Assert.Equal(7, sunday.Days.Count); Assert.Contains(Monday.AddDays(6), sunday.Days);
    }
}
