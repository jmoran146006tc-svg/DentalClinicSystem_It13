using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class PatientRuleTests
{
    private static readonly DateTime Now = RescheduleTests.Now;
    private static Patient Patient(DateTime dob) => new() { PatientId = 1, FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", DateOfBirth = dob };
    [Theory]
    [InlineData(-1, false)] [InlineData(0, true)] [InlineData(1, true)]
    public void GuardianRequiredUntilEighteenthBirthday(int birthdayOffset, bool expected)
    {
        var patient = Patient(Now.Date.AddYears(-18));
        Assert.Equal(expected, Validator.Patient(patient, Now.AddDays(birthdayOffset)).Success);
        patient.GuardianName = "Maria Santos"; patient.GuardianContact = "09171234567";
        Assert.True(Validator.Patient(patient, Now.AddDays(birthdayOffset)).Success);
        patient.GuardianContact = "bad"; Assert.False(Validator.Patient(patient, Now.AddDays(birthdayOffset)).Success);
    }
    [Fact]
    public void GuardianAndMedicalFieldsHaveLimitsAndFutureDobDoesNotThrow()
    {
        var patient = Patient(Now.AddYears(-10)); patient.GuardianName = "Maria";
        Assert.Contains("Guardian contact", Validator.Patient(patient, Now).ErrorMessage);
        patient.GuardianContact = "09171234567"; patient.Allergies = new string('x', FieldLimits.Allergies + 1);
        Assert.False(Validator.Patient(patient, Now).Success);
        patient.Allergies = null; patient.MedicalNotes = new string('x', FieldLimits.MedicalNotes + 1);
        Assert.False(Validator.Patient(patient, Now).Success);
        Assert.False(Validator.Patient(Patient(DateTime.MaxValue), Now).Success);
    }
    [Theory]
    [InlineData(true, false, false)] [InlineData(false, false, false)]
    [InlineData(true, true, true)] [InlineData(false, true, true)]
    public async Task DuplicateDetectionCoversActiveInactiveAndUpdateSelf(bool active, bool self, bool expected)
    {
        var patient = Patient(Now.AddYears(-25));
        var repository = RepositoryStub.Create<IPatientRepository>(
            (nameof(IPatientRepository.FindByNameAndDateOfBirthAsync), _ => Task.FromResult<IReadOnlyList<Patient>>([new() { PatientId = self ? 1 : 2, IsActive = active }])),
            (nameof(IPatientRepository.GetByIdAsync), _ => Task.FromResult<Patient?>(patient)));
        var service = new PatientService(repository, new FixedTimeProvider(Now));
        var result = await service.UpdatePatientAsync(RescheduleTests.Admin, patient);
        Assert.Equal(expected, result.Success);
        if (!expected) Assert.Contains(active ? "already exists" : "inactive", result.ErrorMessage);
        var add = await service.AddPatientAsync(RescheduleTests.Admin, patient);
        Assert.False(add.Success); // An add never excludes a supplied PatientId.
    }
    [Fact]
    public async Task UniquePatientCanBeAdded()
    {
        var repository = RepositoryStub.Create<IPatientRepository>();
        Assert.True((await new PatientService(repository, new FixedTimeProvider(Now)).AddPatientAsync(RescheduleTests.Admin, Patient(Now.AddYears(-25)))).Success);
        Assert.Equal(1, RepositoryStub.Of(repository).Calls[nameof(IPatientRepository.AddAsync)]);
    }
    [Theory]
    [InlineData(false, true)] [InlineData(true, false)]
    public async Task InactivePatientIsReactivatedOnlyAfterAValidSlot(bool leave, bool expected)
    {
        List<string> writes = [];
        var patient = Patient(Now.AddYears(-25)); patient.IsActive = false;
        var patients = RepositoryStub.Create<IPatientRepository>(
            (nameof(IPatientRepository.GetByIdAsync), _ => Task.FromResult<Patient?>(patient)),
            (nameof(IPatientRepository.ReactivateAsync), _ => { writes.Add("reactivate"); return Task.CompletedTask; }));
        var appointments = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.AddAsync), _ => { writes.Add("book"); return Task.CompletedTask; }));
        var result = await SlotRuleTests.Service(appointments, leave, patients).ScheduleAppointmentAsync(RescheduleTests.Admin, new() { PatientId = 1, DentistId = 2, AppointmentDateTime = Now });
        Assert.Equal(expected, result.Success);
        Assert.Equal(expected ? ["book"] : Array.Empty<string>(), writes);
        Assert.False(RepositoryStub.Of(patients).Calls.ContainsKey(nameof(IPatientRepository.ReactivateAsync)));
    }
    [Fact]
    public async Task BookingBackstopFailureDoesNotIssueASeparateReactivation()
    {
        var patient = Patient(Now.AddYears(-25)); patient.IsActive = false;
        var patients = RepositoryStub.Create<IPatientRepository>((nameof(IPatientRepository.GetByIdAsync), _ => Task.FromResult<Patient?>(patient)));
        var appointments = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.AddAsync),
            _ => throw new RepositoryConstraintException("This dentist is already booked during that appointment.", new InvalidOperationException("Database backstop"))));
        var result = await SlotRuleTests.Service(appointments, false, patients).ScheduleAppointmentAsync(RescheduleTests.Admin,
            new() { PatientId = 1, DentistId = 2, AppointmentDateTime = Now });
        Assert.False(result.Success); Assert.Contains("already booked", result.ErrorMessage);
        Assert.False(RepositoryStub.Of(patients).Calls.ContainsKey(nameof(IPatientRepository.ReactivateAsync)));
    }
}
