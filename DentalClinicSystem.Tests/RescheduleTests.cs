using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class RescheduleTests
{
    internal static readonly DateTime Now = new(2026, 10, 5, 10, 0, 0);
    internal static readonly User Admin = new() { Role = Roles.Admin };
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, true, Roles.Admin, true)]
    [InlineData(AppointmentStatus.Scheduled, true, Roles.Receptionist, true)]
    [InlineData(AppointmentStatus.CheckedIn, true, Roles.Admin, false)]
    [InlineData(AppointmentStatus.Completed, true, Roles.Admin, false)]
    [InlineData(AppointmentStatus.Cancelled, true, Roles.Admin, false)]
    [InlineData(AppointmentStatus.NoShow, true, Roles.Admin, false)]
    [InlineData(AppointmentStatus.Scheduled, false, Roles.Admin, false)]
    [InlineData(AppointmentStatus.Scheduled, true, Roles.Dentist, false)]
    public async Task RescheduleKeepsDetailsAndExcludesItself(string status, bool active, string role, bool expected)
    {
        var current = new Appointment { AppointmentId = 7, PatientId = 1, DentistId = 2, AppointmentDateTime = Now, Status = status, Reason = "Cleaning", Notes = "Keep", CancellationReason = "Keep too" };
        var appointments = RepositoryStub.Create<IAppointmentRepository>(
            (nameof(IAppointmentRepository.GetByIdAsync), _ => Task.FromResult<Appointment?>(current)),
            (nameof(IAppointmentRepository.GetByDentistAndRangeAsync), _ => Task.FromResult<IReadOnlyList<Appointment>>([current])));
        var dentists = RepositoryStub.Create<IDentistRepository>((nameof(IDentistRepository.GetByIdAsync), _ => Task.FromResult<Dentist?>(new() { DentistId = 3, IsActive = active })));
        var service = new AppointmentService(appointments, RepositoryStub.Create<IPatientRepository>(), dentists, RepositoryStub.Create<IDentistTimeOffRepository>(), new FixedTimeProvider(Now));
        var result = await service.RescheduleAppointmentAsync(new() { Role = role }, 7, Now, 3);
        Assert.Equal(expected, result.Success);
        Assert.Equal(expected ? 1 : 0, RepositoryStub.Of(appointments).Calls.GetValueOrDefault(nameof(IAppointmentRepository.UpdateAsync)));
        if (expected)
        {
            var saved = Assert.IsType<Appointment>(RepositoryStub.Of(appointments).Arguments[nameof(IAppointmentRepository.UpdateAsync)][0]);
            Assert.Equal(3, saved.DentistId); Assert.Equal("Cleaning", saved.Reason); Assert.Equal("Keep", saved.Notes); Assert.Equal("Keep too", saved.CancellationReason);
        }
        Assert.Equal(2, current.DentistId);
    }
}
