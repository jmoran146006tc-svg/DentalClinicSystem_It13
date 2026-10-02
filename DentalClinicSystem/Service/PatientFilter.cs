using DentalClinicSystem.Models;

namespace DentalClinicSystem.Service
{
    public static class PatientFilter
    {
        public static IEnumerable<Patient> Apply(IEnumerable<Patient> patients, string search, bool includeInactive) =>
            patients.Where(p => (includeInactive || p.IsActive) &&
                new[] { p.FullName, p.ContactNumber, p.Email ?? string.Empty }
                    .Any(value => value.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase)));
    }
}
