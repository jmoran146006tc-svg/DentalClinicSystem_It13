using System.Globalization;
using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers;

public enum ReportRange { ThisWeek, ThisMonth, LastThirtyDays }

public sealed record ReportSnapshot(DateTime From, DateTime To, IReadOnlyList<AppointmentStatusCount> Status,
    IReadOnlyList<RevenueDay> Days, IReadOnlyList<TopTreatmentType> Types, IReadOnlyList<DentistWorkload> Workload);

public static class ReportsPresentation
{
    public static (DateTime From, DateTime To) Range(ReportRange range, DateTime today) =>
        (range switch
        {
            ReportRange.ThisWeek => DashboardPresentation.WeekStart(today),
            ReportRange.LastThirtyDays => today.Date.AddDays(-29),
            _ => new(today.Year, today.Month, 1)
        }, today.Date);

    public static IReadOnlyList<AppointmentStatusCount> Statuses(IReadOnlyList<AppointmentStatusCount> rows) =>
        AppointmentStatus.All.Select(status => new AppointmentStatusCount(status, rows.Where(row => row.Status == status).Sum(row => row.Total))).ToArray();

    public static IReadOnlyList<ChartDatum> StatusChart(IReadOnlyList<AppointmentStatusCount> rows) =>
        rows.Select(row => new ChartDatum(AppointmentStatus.Display(row.Status), row.Total, Theme.StatusStyle(row.Status).Text)).ToArray();
    public static IReadOnlyList<ChartDatum> DailyChart(IReadOnlyList<RevenueDay> rows) =>
        rows.OrderBy(row => row.Day).Select(row => new ChartDatum(row.Day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), (double)row.Billed, Palette.Brand)).ToArray();
    public static IReadOnlyList<ChartDatum> TypeChart(IReadOnlyList<TopTreatmentType> rows) =>
        rows.Select(row => new ChartDatum(row.Name, row.Total, Palette.Brand)).ToArray();

    public static byte[] Csv(ReportSnapshot snapshot) => CsvExporter.Export([
        new("Appointments by status", ["Status", "Count"], snapshot.Status.Select(row => (IReadOnlyList<object?>)[AppointmentStatus.Display(row.Status), row.Total]).ToArray()),
        new("Billed by day", ["Date", "Billed (PHP)"], snapshot.Days.Select(row => (IReadOnlyList<object?>)[row.Day, row.Billed]).ToArray()),
        new("Top treatment types", ["Treatment", "Count", "Billed (PHP)"], snapshot.Types.Select(row => (IReadOnlyList<object?>)[row.Name, row.Total, row.Billed]).ToArray()),
        new("Dentist workload", ["Dentist", "Total", "Completed", "Billed (PHP)"], snapshot.Workload.Select(row => (IReadOnlyList<object?>)[row.Dentist, row.Total, row.Completed, row.Billed]).ToArray())]);
}
