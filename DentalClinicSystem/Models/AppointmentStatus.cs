namespace DentalClinicSystem.Models
{
    public static class AppointmentStatus
    {
        public const string Scheduled = "Scheduled", CheckedIn = "CheckedIn", Completed = "Completed", Cancelled = "Cancelled", NoShow = "NoShow";
        public static readonly string[] All = [Scheduled, CheckedIn, Completed, Cancelled, NoShow];
        public static bool IsValid(string status) => All.Contains(status);
        public static string Display(string status) => status switch { CheckedIn => "Checked in", NoShow => "No show", _ => status };
        public static bool CanTransition(string from, string to) => from switch
        {
            Scheduled => to is CheckedIn or Completed or Cancelled or NoShow,
            CheckedIn => to == Completed,
            _ => false
        };
    }
}
