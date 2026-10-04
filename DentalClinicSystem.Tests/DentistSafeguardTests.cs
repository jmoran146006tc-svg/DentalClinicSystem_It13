using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class DentistSafeguardTests
{
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, 0, false)]
    [InlineData(AppointmentStatus.CheckedIn, 1, false)]
    [InlineData(AppointmentStatus.Scheduled, -1, true)]
    [InlineData(AppointmentStatus.Cancelled, 1, true)]
    [InlineData(AppointmentStatus.Completed, 1, true)]
    [InlineData(AppointmentStatus.NoShow, 1, true)]
    public async Task UpcomingWaitingAppointmentsBlockDeactivation(string status, int minuteOffset, bool expected)
    {
        var now = RescheduleTests.Now;
        var row = new Appointment { DentistId = 2, Status = status, AppointmentDateTime = now.AddMinutes(minuteOffset) };
        var appointments = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.CountUpcomingByDentistAsync), args => Task.FromResult(
            row.AppointmentDateTime >= (DateTime)args[1]! && row.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn ? 1 : 0)));
        var dentists = RepositoryStub.Create<IDentistRepository>((nameof(IDentistRepository.GetByIdAsync), _ => Task.FromResult<Dentist?>(new() { FirstName = "Ana" })));
        var service = new DentistService(dentists, appointments, RepositoryStub.Create<IDentistTimeOffRepository>(), new FixedTimeProvider(now));
        var result = await service.DeleteDentistAsync(RescheduleTests.Admin, 2);
        Assert.Equal(expected, result.Success);
        Assert.Equal(expected ? 1 : 0, RepositoryStub.Of(dentists).Calls.GetValueOrDefault(nameof(IDentistRepository.DeleteAsync)));
        Assert.Equal(now, RepositoryStub.Of(appointments).Arguments[nameof(IAppointmentRepository.CountUpcomingByDentistAsync)][1]);
    }
    [Theory]
    [InlineData(false, 2, false)] [InlineData(true, 2, true)] [InlineData(true, null, false)]
    public async Task DentistLoginRequiresAnActiveLinkedDentist(bool active, int? dentistId, bool expected)
    {
        var user = new User { Role = Roles.Dentist, DentistId = dentistId, PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123", 4) };
        var users = RepositoryStub.Create<IUserRepository>((nameof(IUserRepository.GetByUsernameAsync), _ => Task.FromResult<User?>(user)));
        var dentists = RepositoryStub.Create<IDentistRepository>((nameof(IDentistRepository.GetByIdAsync), _ => Task.FromResult<Dentist?>(new() { IsActive = active })));
        var result = await new AuthService(users, dentists).LoginAsync("dentist", "test123");
        Assert.Equal(expected, result.Success);
        if (!expected) Assert.Equal("Invalid username or password.", result.ErrorMessage);
    }
    [Fact]
    public async Task AdminLoginDoesNotNeedDentistRecord()
    {
        var users = RepositoryStub.Create<IUserRepository>((nameof(IUserRepository.GetByUsernameAsync), _ => Task.FromResult<User?>(new() { Role = Roles.Admin, PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123", 4) })));
        var dentists = RepositoryStub.Create<IDentistRepository>();
        Assert.True((await new AuthService(users, dentists).LoginAsync("admin", "test123")).Success);
        Assert.Empty(RepositoryStub.Of(dentists).Calls);
    }
    [Fact]
    public void SpecializationChoiceRemainsEditableAfterWrapping() => PresentationTests.Sta(() =>
    {
        using var page = new Forms.ucDentistRecords(TestServices.Create<IDentistService>(), RescheduleTests.Admin);
        var combo = (ComboBox)page.GetType().GetField("cboSpecialization", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(page)!;
        Assert.Equal(ComboBoxStyle.DropDown, combo.DropDownStyle); Assert.Equal(FieldLimits.Specialization, combo.MaxLength);
        Assert.Equal(DentalSpecializations.All, combo.Items.Cast<string>()); combo.Text = "Custom field"; Assert.Equal("Custom field", combo.Text);
    });
}
