using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public sealed class AppointmentBoundaryTests
{
    [Theory]
    [InlineData(Roles.Admin, 2, true)] [InlineData(Roles.Dentist, 2, true)] [InlineData(Roles.Dentist, 3, false)] [InlineData(Roles.Receptionist, 2, false)]
    public async Task CompletionEnforcesRoleAndOwnershipAtServiceBoundary(string role, int dentistId, bool allowed)
    {
        var appointment = new Appointment { AppointmentId = 1, DentistId = 2, AppointmentDateTime = RescheduleTests.Now, Status = AppointmentStatus.CheckedIn };
        var repository = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.GetByIdAsync), _ => Task.FromResult<Appointment?>(appointment)));
        var result = await SlotRuleTests.Service(repository).UpdateAppointmentStatusAsync(new() { Role = role, DentistId = dentistId }, 1, AppointmentStatus.Completed);
        Assert.Equal(allowed, result.Success); Assert.Equal(allowed ? 1 : 0, RepositoryStub.Of(repository).Calls.GetValueOrDefault(nameof(IAppointmentRepository.UpdateAsync)));
    }
    [Fact]
    public async Task BookingForcesScheduledAndClearsSuppliedCancellationReason()
    {
        var patients = RepositoryStub.Create<IPatientRepository>((nameof(IPatientRepository.GetByIdAsync), _ => Task.FromResult<Patient?>(new() { PatientId = 1 })));
        var repository = RepositoryStub.Create<IAppointmentRepository>();
        var appointment = new Appointment { PatientId = 1, DentistId = 2, AppointmentDateTime = RescheduleTests.Now, Status = AppointmentStatus.Completed, CancellationReason = "Forged" };
        var result = await SlotRuleTests.Service(repository, patients: patients).ScheduleAppointmentAsync(RescheduleTests.Admin, appointment);
        Assert.True(result.Success); var written = Assert.IsType<Appointment>(RepositoryStub.Of(repository).Arguments[nameof(IAppointmentRepository.AddAsync)][0]);
        Assert.Equal(AppointmentStatus.Scheduled, written.Status); Assert.Null(written.CancellationReason);
    }
}
