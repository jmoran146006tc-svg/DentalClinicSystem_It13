using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public sealed class HardeningTests
{
    [Theory]
    [InlineData("patients")] [InlineData("dentists")] [InlineData("users")] [InlineData("appointments")] [InlineData("treatments")]
    public void EveryCrudPageOffersRetryAndRecoversFromServiceFailure(string kind) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); var actor = ClinicFixture.Actor();
        var (service, method, failure) = kind switch
        {
            "patients" => ((object)services.Patients, nameof(IPatientService.GetAllPatientsAsync), (object)Task.FromResult(ServiceResult<IReadOnlyList<Patient>>.Fail("Load unavailable"))),
            "dentists" => (services.Dentists, nameof(IDentistService.GetAllDentistsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Dentist>>.Fail("Load unavailable"))),
            "users" => (services.Users, nameof(IUserService.GetAllUsersAsync), Task.FromResult(ServiceResult<IReadOnlyList<User>>.Fail("Load unavailable"))),
            "appointments" => (services.Appointments, nameof(IAppointmentService.GetAllAppointmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Fail("Load unavailable"))),
            _ => (services.Treatments, nameof(ITreatmentService.GetAllTreatmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Treatment>>.Fail("Load unavailable")))
        };
        var stub = (ServiceStub)service; var original = stub.Results[method]; stub.Results[method] = failure;
        using UserControl page = kind switch
        {
            "patients" => new ucPatientRecords(services.Patients, actor), "dentists" => new ucDentistRecords(services.Dentists, actor),
            "users" => new ucUserManagement(services.Users, services.Dentists, actor),
            "appointments" => new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, services.TreatmentTypes, actor),
            _ => new ucTreatmentRecords(services.Treatments, services.Appointments, services.TreatmentTypes, actor)
        };
        using var host = new Form { ClientSize = new(1200, 800) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        var alert = UiThread.Controls(page).OfType<InlineAlert>().Single(); var retry = UiThread.Named<AppButton>(page, "pageRefresh");
        Assert.True(alert.Visible); Assert.Equal("Retry", retry.Text);
        stub.Results[method] = original; retry.PerformClick(); Application.DoEvents();
        Assert.False(alert.Visible); Assert.Equal("Refresh", retry.Text); Assert.True(page.Enabled);
        Assert.NotEmpty(UiThread.Controls(page).OfType<ClinicTable>().Single().Records);
    });
}
