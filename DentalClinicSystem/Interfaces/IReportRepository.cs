using DentalClinicSystem.Models;

namespace DentalClinicSystem.Interfaces
{
    public interface IReportRepository
    {
        Task<IReadOnlyList<AppointmentStatusCount>> GetAppointmentStatusCountsAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<RevenueDay>> GetRevenueByDayAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<TopTreatmentType>> GetTopTreatmentTypesAsync(DateTime from, DateTime to, int top);
        Task<IReadOnlyList<DentistWorkload>> GetDentistWorkloadAsync(DateTime from, DateTime to);
    }
}
