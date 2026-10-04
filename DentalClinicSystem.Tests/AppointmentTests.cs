using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Tests;

public class AppointmentTests
{
    [Fact]
    public void FiltersUseMondayWeekAndExclusiveEndAndRoleScope()
    {
        var today = new DateTime(2026, 10, 4); // Sunday
        Appointment[] rows = [new() { AppointmentId = 1, PatientId = 1, DentistId = 2, AppointmentDateTime = new(2026, 9, 28), Status = AppointmentStatus.Scheduled },
            new() { AppointmentId = 2, PatientId = 1, DentistId = 2, AppointmentDateTime = today.AddHours(23), Status = AppointmentStatus.Completed },
            new() { AppointmentId = 3, PatientId = 1, DentistId = 3, AppointmentDateTime = today.AddDays(1) }];
        var admin = new User { Role = Roles.Admin };
        Assert.Equal([1, 2], AppointmentFilter.Apply(rows, admin, "ANA", null, AppointmentDateFilter.ThisWeek, today, _ => "Ana Santos").Select(a => a.AppointmentId));
        Assert.Equal([2], AppointmentFilter.Apply(rows, admin, "", AppointmentStatus.Completed, AppointmentDateFilter.Today, today, _ => "Ana").Select(a => a.AppointmentId));
        Assert.Empty(AppointmentFilter.Apply(rows, new User { Role = Roles.Dentist }, "", null, AppointmentDateFilter.All, today, _ => "Ana"));
        Assert.Equal([1, 2], AppointmentFilter.Apply(rows, new User { Role = Roles.Dentist, DentistId = 2 }, "", null, AppointmentDateFilter.All, today, _ => "Ana").Select(a => a.AppointmentId));
    }
    [Theory]
    [InlineData(Roles.Admin, 0, true, true)]
    [InlineData(Roles.Receptionist, 0, false, true)]
    [InlineData(Roles.Dentist, 2, true, false)]
    [InlineData(Roles.Dentist, 3, false, false)]
    public void DetailsActionsRespectRoleAndDentistOwnership(string role, int dentistId, bool complete, bool cancel) => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var data = new AppointmentDetails(new Appointment { DentistId = 2 }, new Patient(), new Dentist());
            using var dialog = new frmAppointmentDetails(data, TestServices.Create<IAppointmentService>(), new User { Role = role, DentistId = dentistId }, _ => Task.CompletedTask);
            PageLayoutTests.ShowOffscreen(dialog);
            Assert.Equal(complete, dialog.ConfirmButton.Visible);
            Assert.Equal(cancel, dialog.Footer.Controls.OfType<AppButton>().Single(b => b.Text == "Cancel appointment").Visible);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void DentistHasNoSchedulingFormAndLegacyStatusControlsAreDisposed() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var services = TestServices.Empty;
            using var page = new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, services.TreatmentTypes, new User { Role = Roles.Dentist });
            using var host = new Form { ClientSize = new(1440, 1000) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); PageLayoutTests.ShowOffscreen(host);
            Assert.DoesNotContain(PageLayoutTests.Descendants(page), c => c.Name is "cboStatus" or "btnUpdateStatus");
            Assert.False(PageLayoutTests.Named<Button>(page, "btnSchedule").Visible);
            Assert.True(PageLayoutTests.Named<DataGridView>(page, "dgvAppointments").ReadOnly);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
}
