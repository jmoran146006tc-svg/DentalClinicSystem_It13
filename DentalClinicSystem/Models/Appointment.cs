using DentalClinicSystem.Models;
namespace DentalClinicSystem.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = AppointmentStatus.Scheduled;
        public string? Reason { get; set; }
        public string? CancellationReason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
