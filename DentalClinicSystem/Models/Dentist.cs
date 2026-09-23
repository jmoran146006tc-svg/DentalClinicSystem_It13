namespace DentalClinicSystem.Models
{
    public class Dentist
    {
        public int DentistId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? ContactNumber { get; set; }
        public string? LicenseNumber { get; set; }
        public bool IsActive { get; set; } = true;

        public string FullName => $"Dr. {FirstName} {LastName}";
    }
}
