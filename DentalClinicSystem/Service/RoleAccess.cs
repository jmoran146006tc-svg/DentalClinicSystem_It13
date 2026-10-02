using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public enum Permission
    {
        ViewDashboard, ViewPatients, ManagePatients, ViewDentists, ManageDentists,
        ViewAppointments, ManageAppointments, MarkAppointmentCompleted, CancelAppointment,
        ViewTreatments, ManageTreatments, ViewUsers, ManageUsers, ViewReports
    }

    public static class RoleAccess
    {
        private const string DeniedMessage = "You do not have permission to do that.";
        private static readonly IReadOnlyDictionary<string, HashSet<Permission>> Matrix =
            new Dictionary<string, HashSet<Permission>>
            {
                [Roles.Admin] = Enum.GetValues<Permission>().ToHashSet(),
                [Roles.Receptionist] = [Permission.ViewDashboard, Permission.ViewPatients,
                    Permission.ManagePatients, Permission.ViewAppointments, Permission.ManageAppointments,
                    Permission.CancelAppointment],
                [Roles.Dentist] = [Permission.ViewDashboard, Permission.ViewAppointments,
                    Permission.MarkAppointmentCompleted, Permission.ViewTreatments, Permission.ManageTreatments]
            };

        public static bool Can(User actor, Permission permission) =>
            actor.IsActive && Matrix.TryGetValue(actor.Role, out var permissions) && permissions.Contains(permission);
        public static bool IsDentist(User actor) => actor.Role == Roles.Dentist;
        public static bool RequiresDentist(string role) => role == Roles.Dentist;
        public static bool IsAdmin(User actor) => actor.Role == Roles.Admin;
        public static bool CanAccessAppointment(User actor, Appointment appointment) =>
            Can(actor, Permission.ViewAppointments) && (!IsDentist(actor) || actor.DentistId == appointment.DentistId);
        public static bool CanChangeStatus(User actor, Appointment appointment, string status)
        {
            if (!CanAccessAppointment(actor, appointment)) return false;
            if (IsAdmin(actor)) return true;
            return status switch
            {
                AppointmentStatus.Completed => Can(actor, Permission.MarkAppointmentCompleted),
                AppointmentStatus.Cancelled or AppointmentStatus.NoShow => Can(actor, Permission.CancelAppointment),
                _ => false
            };
        }
        public static ServiceResult Denied() => ServiceResult.Fail(DeniedMessage);
        public static ServiceResult<T> Denied<T>() => ServiceResult<T>.Fail(DeniedMessage);
    }
}
