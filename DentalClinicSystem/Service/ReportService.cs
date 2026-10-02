using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public class ReportService(IReportRepository repository) : IReportService
    {
        public async Task<ServiceResult<IReadOnlyList<AppointmentStatusCount>>> GetAppointmentStatusCountsAsync(User actor, DateTime from, DateTime to)
        {
            if (!RoleAccess.Can(actor, Permission.ViewReports)) return RoleAccess.Denied<IReadOnlyList<AppointmentStatusCount>>();
            var validation = Validator.DateRange(from, to);
            if (!validation.Success) return ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Fail(validation.ErrorMessage);
            return ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok(await repository.GetAppointmentStatusCountsAsync(from.Date, to.Date));
        }

        public async Task<ServiceResult<IReadOnlyList<RevenueDay>>> GetRevenueByDayAsync(User actor, DateTime from, DateTime to)
        {
            if (!RoleAccess.Can(actor, Permission.ViewReports)) return RoleAccess.Denied<IReadOnlyList<RevenueDay>>();
            var validation = Validator.DateRange(from, to);
            if (!validation.Success) return ServiceResult<IReadOnlyList<RevenueDay>>.Fail(validation.ErrorMessage);
            return ServiceResult<IReadOnlyList<RevenueDay>>.Ok(await repository.GetRevenueByDayAsync(from.Date, to.Date));
        }

        public async Task<ServiceResult<IReadOnlyList<TopTreatmentType>>> GetTopTreatmentTypesAsync(User actor, DateTime from, DateTime to, int top = 10)
        {
            if (!RoleAccess.Can(actor, Permission.ViewReports)) return RoleAccess.Denied<IReadOnlyList<TopTreatmentType>>();
            var validation = Validator.DateRange(from, to);
            if (!validation.Success) return ServiceResult<IReadOnlyList<TopTreatmentType>>.Fail(validation.ErrorMessage);
            if (top < 1) return ServiceResult<IReadOnlyList<TopTreatmentType>>.Fail("Top must be at least one.");
            return ServiceResult<IReadOnlyList<TopTreatmentType>>.Ok(await repository.GetTopTreatmentTypesAsync(from.Date, to.Date, top));
        }

        public async Task<ServiceResult<IReadOnlyList<DentistWorkload>>> GetDentistWorkloadAsync(User actor, DateTime from, DateTime to)
        {
            if (!RoleAccess.Can(actor, Permission.ViewReports)) return RoleAccess.Denied<IReadOnlyList<DentistWorkload>>();
            var validation = Validator.DateRange(from, to);
            if (!validation.Success) return ServiceResult<IReadOnlyList<DentistWorkload>>.Fail(validation.ErrorMessage);
            return ServiceResult<IReadOnlyList<DentistWorkload>>.Ok(await repository.GetDentistWorkloadAsync(from.Date, to.Date));
        }

    }
}
