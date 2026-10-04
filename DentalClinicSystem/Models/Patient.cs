namespace DentalClinicSystem.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianContact { get; set; }
        public string? Allergies { get; set; }
        public string? MedicalNotes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FullName => $"{FirstName} {LastName}";
    }
}
