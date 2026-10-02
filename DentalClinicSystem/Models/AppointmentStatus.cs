namespace DentalClinicSystem.Models
{
    public static class AppointmentStatus
    {
        public const string Scheduled = "Scheduled", Completed = "Completed", Cancelled = "Cancelled", NoShow = "NoShow";
        public static readonly string[] All = [Scheduled, Completed, Cancelled, NoShow];
        public static bool IsValid(string status) => All.Contains(status);
    }
}
