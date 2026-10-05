using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class TreatmentRuleTests
{
    private static readonly DateTime Now = RescheduleTests.Now;
    [Theory]
    [InlineData(null, true)] [InlineData("", true)] [InlineData(" ", true)]
    [InlineData("11", true)] [InlineData("18", true)] [InlineData("21", true)] [InlineData("28", true)]
    [InlineData("31", true)] [InlineData("38", true)] [InlineData("41", true)] [InlineData("48", true)]
    [InlineData("51", true)] [InlineData("55", true)] [InlineData("65", true)] [InlineData("75", true)] [InlineData("85", true)]
    [InlineData("10", false)] [InlineData("19", false)] [InlineData("49", false)] [InlineData("56", false)]
    [InlineData("86", false)] [InlineData("91", false)] [InlineData("1", false)] [InlineData("111", false)] [InlineData("1a", false)]
    public void FdiToothValidation(string? tooth, bool expected)
    {
        Assert.Equal(expected, ToothNumbering.IsValid(tooth));
        Assert.Equal(expected, Validator.Treatment(new() { ToothNumber = tooth, DatePerformed = Now }, Now).Success);
    }
    [Theory]
    [InlineData(AppointmentStatus.Cancelled, 0, 0, false, "cancelled or no-show")]
    [InlineData(AppointmentStatus.NoShow, 0, 0, false, "cancelled or no-show")]
    [InlineData(AppointmentStatus.Scheduled, 1, 1, false, "hasn't happened")]
    [InlineData(AppointmentStatus.Completed, 0, -1, false, "match the appointment")]
    [InlineData(AppointmentStatus.Completed, -1, 0, false, "match the appointment")]
    [InlineData(AppointmentStatus.Completed, 0, 0, true, "")]
    [InlineData(AppointmentStatus.CheckedIn, 0, 0, true, "")]
    [InlineData(AppointmentStatus.Scheduled, -1, -1, true, "")]
    public async Task AddAndUpdateUseSameEligibility(string status, int appointmentDays, int performedDays, bool expected, string error)
    {
        var appointment = new Appointment { AppointmentId = 1, DentistId = 2, Status = status, AppointmentDateTime = Now.AddDays(appointmentDays) };
        var appointments = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.GetByIdAsync), _ => Task.FromResult<Appointment?>(appointment)));
        var treatments = RepositoryStub.Create<ITreatmentRepository>((nameof(ITreatmentRepository.GetByIdAsync), _ => Task.FromResult<Treatment?>(new() { TreatmentId = 7, AppointmentId = 1 })));
        var types = RepositoryStub.Create<ITreatmentTypeRepository>((nameof(ITreatmentTypeRepository.GetByIdAsync), _ => Task.FromResult<TreatmentType?>(new())));
        var service = new TreatmentService(treatments, appointments, types, new FixedTimeProvider(Now));
        foreach (var update in new[] { false, true })
        {
            var treatment = new Treatment { TreatmentId = update ? 7 : 0, AppointmentId = 1, DatePerformed = Now.AddDays(performedDays), ToothNumber = "11" };
            var result = update ? await service.UpdateTreatmentAsync(RescheduleTests.Admin, treatment) : await service.AddTreatmentAsync(RescheduleTests.Admin, treatment);
            Assert.Equal(expected, result.Success); Assert.Contains(error, result.ErrorMessage);
        }
        Assert.False((await service.AddTreatmentAsync(new() { Role = Roles.Dentist, DentistId = 3 }, new() { AppointmentId = 1 })).Success);
    }
}
