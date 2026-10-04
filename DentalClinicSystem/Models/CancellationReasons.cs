namespace DentalClinicSystem.Models;

public static class CancellationReasons
{
    public const string NoShow = "No Show", PatientRescheduled = "Patient Rescheduled",
        ClinicRescheduled = "Clinic Rescheduled", Other = "Other";
    public static readonly string[] All = [NoShow, PatientRescheduled, ClinicRescheduled, Other];
}
