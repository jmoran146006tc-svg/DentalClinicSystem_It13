using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using static DentalClinicSystem.DBContent.StoredProcedureRunner;

namespace DentalClinicSystem.DBContent
{
    public class MySqlReportRepository : IReportRepository
    {
        public Task<IReadOnlyList<AppointmentStatusCount>> GetAppointmentStatusCountsAsync(DateTime from, DateTime to) =>
            QueryAsync<AppointmentStatusCount>("sp_Report_AppointmentsByStatus", reader => new(reader.GetString("Status"), reader.GetInt32("Total")),
                Parameter("@p_From", from.Date), Parameter("@p_To", to.Date));

        public Task<IReadOnlyList<RevenueDay>> GetRevenueByDayAsync(DateTime from, DateTime to) =>
            QueryAsync<RevenueDay>("sp_Report_RevenueByDay", reader => new(reader.GetDateTime("Day"), reader.GetDecimal("Revenue")),
                Parameter("@p_From", from.Date), Parameter("@p_To", to.Date));

        public Task<IReadOnlyList<TopTreatmentType>> GetTopTreatmentTypesAsync(DateTime from, DateTime to, int top) =>
            QueryAsync<TopTreatmentType>("sp_Report_TopTreatmentTypes", reader => new(reader.GetString("Name"), reader.GetInt32("Total"), reader.GetDecimal("Revenue")),
                Parameter("@p_From", from.Date), Parameter("@p_To", to.Date), Parameter("@p_Top", top));

        public Task<IReadOnlyList<DentistWorkload>> GetDentistWorkloadAsync(DateTime from, DateTime to) =>
            QueryAsync<DentistWorkload>("sp_Report_DentistWorkload", reader => new(reader.GetString(nameof(DentistWorkload.Dentist)), reader.GetInt32("Total"), reader.GetInt32(nameof(DentistWorkload.Completed)), reader.GetDecimal("Revenue")),
                Parameter("@p_From", from.Date), Parameter("@p_To", to.Date));

    }
}
