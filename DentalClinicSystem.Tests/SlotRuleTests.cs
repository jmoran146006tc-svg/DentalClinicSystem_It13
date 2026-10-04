using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class SlotRuleTests
{
    private static readonly DateTime Day = new(2026, 10, 5);
    [Theory]
    [InlineData(9, 0, 30, true)]
    [InlineData(8, 59, 30, false)]
    [InlineData(16, 30, 30, true)]
    [InlineData(16, 31, 30, false)]
    [InlineData(17, 0, 15, false)]
    public void ClinicHoursIncludeOpeningAndExactClosing(int hour, int minute, int duration, bool expected) =>
        Assert.Equal(expected, ClinicRules.IsOpen(Day.AddHours(hour).AddMinutes(minute), duration));
    [Fact]
    public void SundayIsClosed() => Assert.False(ClinicRules.IsOpen(Day.AddDays(-1).AddHours(9), 30));
    [Theory]
    [InlineData(15, true)] [InlineData(240, true)] [InlineData(30, true)]
    [InlineData(14, false)] [InlineData(241, false)] [InlineData(31, false)]
    public void DurationLimitsAndSteps(int duration, bool expected) => Assert.Equal(expected, Validator.Duration(duration).Success);

    internal static AppointmentService Service(IAppointmentRepository appointments, bool leave = false, IPatientRepository? patients = null) =>
        new(appointments, patients ?? RepositoryStub.Create<IPatientRepository>(),
            RepositoryStub.Create<IDentistRepository>((nameof(IDentistRepository.GetByIdAsync), _ => Task.FromResult<Dentist?>(new() { DentistId = 2, FirstName = "Ana", LastName = "Santos" }))),
            RepositoryStub.Create<IDentistTimeOffRepository>((nameof(IDentistTimeOffRepository.GetOverlappingAsync), _ => Task.FromResult<IReadOnlyList<DentistTimeOff>>(leave ? [new() { DentistId = 2, StartDate = Day, EndDate = Day }] : []))),
            new FixedTimeProvider(Day.AddHours(9)));

    [Theory]
    [InlineData(30, 30, false, null, true)]
    [InlineData(29, 30, false, null, false)]
    [InlineData(-30, 30, false, null, true)]
    [InlineData(-29, 30, false, null, false)]
    [InlineData(60, 90, false, null, false)]
    [InlineData(60, 90, true, null, true)]
    [InlineData(60, 90, false, 7, true)]
    public async Task SlotChecksHalfOpenIntervalsAndSelfExclusion(int offset, int existingDuration, bool cancelled, int? exclude, bool expected)
    {
        var existing = new Appointment { AppointmentId = 7, DentistId = 2, AppointmentDateTime = Day.AddHours(10), DurationMinutes = existingDuration, Status = cancelled ? AppointmentStatus.Cancelled : AppointmentStatus.Scheduled };
        var repository = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.GetByDentistAndRangeAsync), _ => Task.FromResult<IReadOnlyList<Appointment>>([existing])));
        var service = Service(repository);
        var result = await service.ValidateSlotAsync(2, existing.AppointmentDateTime.AddMinutes(offset), 30, exclude);
        Assert.Equal(expected, result.Success);
        Assert.Equal(expected, await service.IsDentistAvailableAsync(2, existing.AppointmentDateTime.AddMinutes(offset), 30, exclude));
        var args = RepositoryStub.Of(repository).Arguments[nameof(IAppointmentRepository.GetByDentistAndRangeAsync)];
        Assert.Equal(existing.AppointmentDateTime.AddMinutes(offset - ClinicRules.MaxDurationMinutes), args[1]);
    }
    [Fact]
    public async Task LeaveBlocksBookingAndAvailability()
    {
        var repository = RepositoryStub.Create<IAppointmentRepository>(); var service = Service(repository, leave: true);
        Assert.Contains("Dr. Ana Santos", (await service.ValidateSlotAsync(2, Day.AddHours(10), 30)).ErrorMessage);
        Assert.False(await service.IsDentistAvailableAsync(2, Day.AddHours(10), 30));
        Assert.Equal(0, RepositoryStub.Of(repository).Calls.GetValueOrDefault(nameof(IAppointmentRepository.GetByDentistAndRangeAsync)));
    }
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, false)]
    [InlineData(AppointmentStatus.CheckedIn, false)]
    [InlineData(AppointmentStatus.Cancelled, true)]
    [InlineData(AppointmentStatus.Completed, true)]
    [InlineData(AppointmentStatus.NoShow, true)]
    public async Task TimeOffCannotDisplaceWaitingPatients(string status, bool expected)
    {
        var appointments = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.GetByDentistAndRangeAsync), _ => Task.FromResult<IReadOnlyList<Appointment>>([new() { Status = status }])));
        var leave = RepositoryStub.Create<IDentistTimeOffRepository>();
        var dentists = RepositoryStub.Create<IDentistRepository>((nameof(IDentistRepository.GetByIdAsync), _ => Task.FromResult<Dentist?>(new() { FirstName = "Ana" })));
        var service = new DentistService(dentists, appointments, leave, new FixedTimeProvider(Day));
        var result = await service.AddTimeOffAsync(RescheduleTests.Admin, new() { DentistId = 2, StartDate = Day, EndDate = Day });
        Assert.Equal(expected, result.Success);
        Assert.Equal(expected ? 1 : 0, RepositoryStub.Of(leave).Calls.GetValueOrDefault(nameof(IDentistTimeOffRepository.AddAsync)));
        Assert.False((await service.AddTimeOffAsync(new() { Role = Roles.Receptionist }, new())).Success);
    }
    [Fact]
    public void TimeOffValidatesDateOrderAndReasonLength()
    {
        Assert.False(Validator.TimeOff(new() { StartDate = Day, EndDate = Day.AddDays(-1) }).Success);
        Assert.False(Validator.TimeOff(new() { StartDate = Day, EndDate = Day, Reason = new string('x', FieldLimits.TimeOffReason + 1) }).Success);
        Assert.True(Validator.TimeOff(new() { StartDate = Day, EndDate = Day }).Success);
    }
    [Fact]
    public void CalendarUsesLongAppointmentDuration()
    {
        Appointment[] rows = [new() { AppointmentId = 1, AppointmentDateTime = Day.AddHours(9), DurationMinutes = 90 },
            new() { AppointmentId = 2, AppointmentDateTime = Day.AddHours(10), DurationMinutes = 30 },
            new() { AppointmentId = 3, AppointmentDateTime = Day.AddHours(10.5), DurationMinutes = 30 }];
        var slots = CalendarLayout.Arrange(rows);
        Assert.Equal(2, slots[0].Columns); Assert.Equal(1, slots[1].Column); Assert.Equal(1, slots[2].Columns);
    }
    [Fact]
    public async Task ReceptionistVisitReasonsContainDurationsWithoutCosts()
    {
        var repository = RepositoryStub.Create<ITreatmentTypeRepository>((nameof(ITreatmentTypeRepository.GetAllAsync), _ => Task.FromResult<IReadOnlyList<TreatmentType>>([new() { Name = "Cleaning", DefaultCost = 800, DefaultDurationMinutes = 45 }, new() { Name = ClinicRules.ConsultationReason }])));
        var result = await new TreatmentTypeService(repository).GetVisitReasonsAsync(new() { Role = Roles.Receptionist });
        Assert.True(result.Success); Assert.Equal(2, result.Data!.Count); Assert.Equal(ClinicRules.ConsultationReason, result.Data[0].Name); Assert.Equal(45, result.Data[1].DurationMinutes);
        Assert.Null(typeof(VisitReason).GetProperty("Cost"));
    }
}
