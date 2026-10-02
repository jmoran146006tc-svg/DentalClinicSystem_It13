using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Interfaces
{
    public interface IReportService
    {
        Task<ServiceResult<IReadOnlyList<AppointmentStatusCount>>> GetAppointmentStatusCountsAsync(User actor, DateTime from, DateTime to);
        Task<ServiceResult<IReadOnlyList<RevenueDay>>> GetRevenueByDayAsync(User actor, DateTime from, DateTime to);
        Task<ServiceResult<IReadOnlyList<TopTreatmentType>>> GetTopTreatmentTypesAsync(User actor, DateTime from, DateTime to, int top = 10);
        Task<ServiceResult<IReadOnlyList<DentistWorkload>>> GetDentistWorkloadAsync(User actor, DateTime from, DateTime to);
    }
}
