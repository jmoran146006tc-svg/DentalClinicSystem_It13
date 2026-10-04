namespace DentalClinicSystem.Models;

public class DentistTimeOff
{
    public int TimeOffId { get; set; }
    public int DentistId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
}
