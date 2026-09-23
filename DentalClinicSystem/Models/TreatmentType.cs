namespace DentalClinicSystem.Models
{
    public class TreatmentType
    {
        public int TreatmentTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DefaultCost { get; set; }
        public string? Description { get; set; }
    }
}
