namespace DentalClinicSystem.Models
{
    public sealed record PatientHistory(AppointmentDetails Selected, IReadOnlyList<Appointment> Appointments, IReadOnlyList<Treatment> Treatments, IReadOnlyList<TreatmentType> TreatmentTypes);
}
