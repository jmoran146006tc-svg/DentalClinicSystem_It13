using System;
using System.Collections.Generic;
using System.Text;

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
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FullName => $"{FirstName} {LastName}";
    }

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

    public class TreatmentType
    {
        public int TreatmentTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DefaultCost { get; set; }
        public string? Description { get; set; }
    }

    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DentistId { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

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

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? DentistId { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
