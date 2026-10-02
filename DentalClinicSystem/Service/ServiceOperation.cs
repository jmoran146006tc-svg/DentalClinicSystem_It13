using DentalClinicSystem.Interfaces;

namespace DentalClinicSystem.Service
{
    internal static class ServiceOperation
    {
        public static async Task<ServiceResult> SaveAsync(Func<Task> action)
        {
            try { await action(); return ServiceResult.Ok(); }
            catch (RepositoryConstraintException ex) { return ServiceResult.Fail(ex.Message); }
        }
    }
}
