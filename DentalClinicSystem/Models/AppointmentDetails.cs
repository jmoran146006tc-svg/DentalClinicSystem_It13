namespace DentalClinicSystem.Models
{
    public sealed record AppointmentDetails(Appointment Appointment, Patient Patient, Dentist Dentist);
}
