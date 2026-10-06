using DentalClinicSystem.Helpers;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.UiTests;

// The six demo slot times and five dentists, with extra simultaneous and short
// visits to exercise the unreadable cases. Entirely synthetic; no SQL is run.
internal static class CalendarDemo
{
    public static readonly DateTime Today = new(2026, 10, 6);
    public static readonly DateTime Monday = DashboardPresentation.WeekStart(Today);
    public static readonly Dictionary<int, string> Dentists = new()
    {
        [1] = "Dr. Maria Santos", [2] = "Dr. Carlos Reyes", [3] = "Dr. Isabel del Rosario",
        [4] = "Dr. Paolo Navarro", [5] = "Dr. Teresa Bautista"
    };
    public static readonly Dictionary<int, string> Patients = new()
    {
        [1] = "Mark Gonzales", [2] = "Alyssa Rivera", [3] = "Teresita Aquino",
        [4] = "Juan Carlos Mendoza", [5] = "Maria Cristina Garcia", [6] = "Sofia Gomez"
    };
    public static Appointment[] Appointments => Enumerable.Range(0, 6).SelectMany(day =>
        Enumerable.Range(0, 6).SelectMany(slot => Enumerable.Range(1, 5).Select(dentist => new Appointment
        {
            AppointmentId = day * 100 + slot * 5 + dentist, DentistId = dentist, PatientId = slot + 1,
            AppointmentDateTime = Monday.AddDays(day).AddHours(new[] { 9d, 10, 11, 13.5, 14.5, 15.5 }[slot]),
            DurationMinutes = slot == 0 ? 15 : slot == 2 ? 90 : 45,
            Reason = slot == 0 ? "Consultation" : "Dental Cleaning",
            Status = AppointmentStatus.All[(day + dentist + slot) % AppointmentStatus.All.Length]
        }))).ToArray();
}
