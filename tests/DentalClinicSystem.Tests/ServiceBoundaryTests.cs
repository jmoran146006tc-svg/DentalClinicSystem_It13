using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public sealed class ServiceBoundaryTests
{
    [Theory]
    [InlineData(Roles.Admin)] [InlineData(Roles.Receptionist)] [InlineData(Roles.Dentist)] [InlineData("Unknown")]
    public void RoleMatrixIsExactForEveryPermission(string role)
    {
        Permission[] allowed = role switch
        {
            Roles.Admin => Enum.GetValues<Permission>(),
            Roles.Receptionist => [Permission.ViewDashboard, Permission.ViewPatients, Permission.ManagePatients, Permission.ViewAppointments, Permission.ManageAppointments, Permission.CancelAppointment, Permission.CheckInAppointment],
            Roles.Dentist => [Permission.ViewDashboard, Permission.ViewAppointments, Permission.MarkAppointmentCompleted, Permission.ViewTreatments, Permission.ManageTreatments], _ => []
        };
        var actor = new User { Role = role };
        foreach (var permission in Enum.GetValues<Permission>()) Assert.Equal(allowed.Contains(permission), RoleAccess.Can(actor, permission));
        actor.IsActive = false; Assert.All(Enum.GetValues<Permission>(), permission => Assert.False(RoleAccess.Can(actor, permission)));
    }
    [Theory]
    [InlineData(Roles.Admin, true)] [InlineData(Roles.Receptionist, true)] [InlineData(Roles.Dentist, false)]
    public async Task PatientReadsWritesAndReactivationEnforceRole(string role, bool allowed)
    {
        var repository = RepositoryStub.Create<IPatientRepository>((nameof(IPatientRepository.GetByIdAsync), _ => Task.FromResult<Patient?>(new() { IsActive = false })));
        var service = new PatientService(repository, new FixedTimeProvider(RescheduleTests.Now)); var actor = new User { Role = role };
        Assert.Equal(allowed, (await service.GetAllPatientsAsync(actor)).Success);
        Assert.Equal(allowed, (await service.ReactivatePatientAsync(actor, 2)).Success);
        Assert.Equal(allowed, (await service.AddPatientAsync(actor, new() { FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", DateOfBirth = new(1990, 1, 1) })).Success);
        if (!allowed) Assert.Empty(RepositoryStub.Of(repository).Calls);
    }
    [Fact]
    public async Task ReceptionistCannotReadOrWriteAnyTreatmentEndpoint()
    {
        var treatments = RepositoryStub.Create<ITreatmentRepository>(); var appointments = RepositoryStub.Create<IAppointmentRepository>(); var types = RepositoryStub.Create<ITreatmentTypeRepository>();
        var service = new TreatmentService(treatments, appointments, types); var actor = new User { Role = Roles.Receptionist };
        Assert.False((await service.GetAllTreatmentsAsync(actor)).Success);
        Assert.False((await service.GetTreatmentsForPatientAsync(actor, 1)).Success);
        Assert.False((await service.GetTreatmentsForAppointmentAsync(actor, 1)).Success);
        Assert.False((await service.AddTreatmentAsync(actor, new())).Success); Assert.False((await service.UpdateTreatmentAsync(actor, new())).Success);
        Assert.Empty(RepositoryStub.Of(treatments).Calls); Assert.Empty(RepositoryStub.Of(appointments).Calls); Assert.Empty(RepositoryStub.Of(types).Calls);
    }
    [Fact]
    public async Task DentistListsContainOnlyOwnAppointmentsTreatments()
    {
        Appointment[] appointments = [new() { AppointmentId = 1, DentistId = 2 }, new() { AppointmentId = 2, DentistId = 3 }];
        Treatment[] treatments = [new() { TreatmentId = 1, AppointmentId = 1 }, new() { TreatmentId = 2, AppointmentId = 2 }];
        var repository = RepositoryStub.Create<ITreatmentRepository>((nameof(ITreatmentRepository.GetAllAsync), _ => Task.FromResult<IReadOnlyList<Treatment>>(treatments)));
        var appointmentRepository = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.GetAllAsync), _ => Task.FromResult<IReadOnlyList<Appointment>>(appointments)));
        var result = await new TreatmentService(repository, appointmentRepository, RepositoryStub.Create<ITreatmentTypeRepository>()).GetAllTreatmentsAsync(new() { Role = Roles.Dentist, DentistId = 2 });
        Assert.True(result.Success); Assert.Equal(1, Assert.Single(result.Data ?? []).TreatmentId);
    }
    [Theory]
    [InlineData(1, 2, "own")] [InlineData(2, 1, "last active Admin")]
    public async Task UserDeactivationProtectsSelfAndLastAdmin(int actorId, int activeAdmins, string message)
    {
        var user = new User { UserId = 1, Role = Roles.Admin };
        var users = RepositoryStub.Create<IUserRepository>((nameof(IUserRepository.GetByIdAsync), _ => Task.FromResult<User?>(user)),
            (nameof(IUserRepository.GetAllAsync), _ => Task.FromResult<IReadOnlyList<User>>(Enumerable.Range(1, activeAdmins).Select(id => new User { UserId = id, Role = Roles.Admin }).ToArray())));
        var result = await new UserService(users, RepositoryStub.Create<IDentistRepository>()).DeactivateUserAsync(new() { UserId = actorId, Role = Roles.Admin }, 1);
        Assert.False(result.Success); Assert.Contains(message, result.ErrorMessage); Assert.False(RepositoryStub.Of(users).Calls.ContainsKey(nameof(IUserRepository.UpdateAsync)));
    }
    [Fact]
    public async Task DuplicateUserAndUnlinkedDentistFailBeforeWritingAndHashesStayPrivate()
    {
        var stored = new User { UserId = 4, Username = "duplicate", PasswordHash = "test-only-hash" };
        var users = RepositoryStub.Create<IUserRepository>((nameof(IUserRepository.GetByUsernameAsync), _ => Task.FromResult<User?>(stored)),
            (nameof(IUserRepository.GetAllAsync), _ => Task.FromResult<IReadOnlyList<User>>([stored])));
        var service = new UserService(users, RepositoryStub.Create<IDentistRepository>());
        Assert.Contains("taken", (await service.AddUserAsync(RescheduleTests.Admin, new() { Username = "duplicate", Role = Roles.Admin }, "test-only-password")).ErrorMessage);
        RepositoryStub.Of(users).Handlers[nameof(IUserRepository.GetByUsernameAsync)] = _ => Task.FromResult<User?>(null);
        Assert.Contains("active dentist", (await service.AddUserAsync(RescheduleTests.Admin, new() { Username = "dentist", Role = Roles.Dentist }, "test-only-password")).ErrorMessage);
        var result = await service.GetAllUsersAsync(RescheduleTests.Admin);
        Assert.True(string.IsNullOrEmpty(Assert.Single(result.Data ?? []).PasswordHash)); Assert.False(RepositoryStub.Of(users).Calls.ContainsKey(nameof(IUserRepository.AddAsync)));
    }
    [Theory]
    [InlineData(Roles.Receptionist)] [InlineData(Roles.Dentist)]
    public async Task EveryReportEndpointDeniesNonAdminsWithoutRepositoryCalls(string role)
    {
        var repository = RepositoryStub.Create<IReportRepository>(); var service = new ReportService(repository); var actor = new User { Role = role }; var day = RescheduleTests.Now;
        Assert.False((await service.GetAppointmentStatusCountsAsync(actor, day, day)).Success);
        Assert.False((await service.GetRevenueByDayAsync(actor, day, day)).Success);
        Assert.False((await service.GetTopTreatmentTypesAsync(actor, day, day)).Success);
        Assert.False((await service.GetDentistWorkloadAsync(actor, day, day)).Success); Assert.Empty(RepositoryStub.Of(repository).Calls);
    }
    [Fact]
    public async Task ReportsValidateRangeAndTopBeforeReading()
    {
        var repository = RepositoryStub.Create<IReportRepository>(); var service = new ReportService(repository); var day = RescheduleTests.Now; var actor = RescheduleTests.Admin;
        Assert.False((await service.GetAppointmentStatusCountsAsync(actor, day, day.AddDays(-1))).Success);
        Assert.False((await service.GetRevenueByDayAsync(actor, day, day.AddDays(-1))).Success);
        Assert.False((await service.GetTopTreatmentTypesAsync(actor, day, day, 0)).Success);
        Assert.False((await service.GetDentistWorkloadAsync(actor, day, day.AddDays(-1))).Success); Assert.Empty(RepositoryStub.Of(repository).Calls);
    }
}
