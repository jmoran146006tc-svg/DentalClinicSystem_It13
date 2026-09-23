namespace DentalClinicSystem.Models
{
    public class Treatment
    {
        public int TreatmentId { get; set; }
        public int AppointmentId { get; set; }
        public int TreatmentTypeId { get; set; }
        public string? ToothNumber { get; set; }
        public decimal Cost { get; set; }
        public DateTime DatePerformed { get; set; }
        public string? Notes { get; set; }
    }
}
