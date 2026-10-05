using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Tests;

// Mirrors active/inactive queries and soft writes; returns detached models like a database read.
internal sealed class MemoryPatientRepository : IPatientRepository
{
    private readonly Dictionary<int, Patient> _rows = [];
    public Task<IReadOnlyList<Patient>> GetAllAsync() => Task.FromResult<IReadOnlyList<Patient>>(_rows.Values.Where(row => row.IsActive).Select(Copy).ToArray());
    public Task<IReadOnlyList<Patient>> GetAllIncludingInactiveAsync() => Task.FromResult<IReadOnlyList<Patient>>(_rows.Values.Select(Copy).ToArray());
    public Task<Patient?> GetByIdAsync(int patientId) => Task.FromResult(_rows.TryGetValue(patientId, out var patient) ? Copy(patient) : null);
    public Task AddAsync(Patient patient)
    {
        var stored = Copy(patient); stored.PatientId = _rows.Keys.DefaultIfEmpty().Max() + 1; stored.IsActive = true; _rows.Add(stored.PatientId, stored); return Task.CompletedTask;
    }
    public Task UpdateAsync(Patient patient)
    {
        if (_rows.TryGetValue(patient.PatientId, out var previous)) { var stored = Copy(patient); stored.CreatedAt = previous.CreatedAt; stored.IsActive = previous.IsActive; _rows[patient.PatientId] = stored; }
        return Task.CompletedTask;
    }
    internal void SimulateInactivityEvent(int patientId) { if (_rows.TryGetValue(patientId, out var patient)) patient.IsActive = false; }
    public Task ReactivateAsync(int patientId) { if (_rows.TryGetValue(patientId, out var patient)) patient.IsActive = true; return Task.CompletedTask; }
    public Task<IReadOnlyList<Patient>> FindByNameAndDateOfBirthAsync(string first, string last, DateTime dob) => Task.FromResult<IReadOnlyList<Patient>>(
        _rows.Values.Where(row => row.FirstName.Equals(first, StringComparison.OrdinalIgnoreCase) && row.LastName.Equals(last, StringComparison.OrdinalIgnoreCase) && row.DateOfBirth.Date == dob.Date).Select(Copy).ToArray());
    private static Patient Copy(Patient row) => new()
    {
        PatientId = row.PatientId, FirstName = row.FirstName, LastName = row.LastName, ContactNumber = row.ContactNumber, DateOfBirth = row.DateOfBirth,
        Email = row.Email, Address = row.Address, IsActive = row.IsActive, CreatedAt = row.CreatedAt, GuardianName = row.GuardianName,
        GuardianContact = row.GuardianContact, Allergies = row.Allergies, MedicalNotes = row.MedicalNotes
    };
}
