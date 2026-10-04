namespace DentalClinicSystem.Models
{
    public sealed record DentistWorkload(string Dentist, int Total, int Completed, decimal Billed);
}
