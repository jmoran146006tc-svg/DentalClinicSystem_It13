using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class RealWorldUiTests
{
    [Theory]
    [InlineData(false, "No Show", AppointmentStatus.Cancelled)]
    [InlineData(true, "Patient absent", AppointmentStatus.NoShow)]
    public async Task CancellationUsesExplicitFlag(bool noShow, string reason, string expected)
    {
        var appointment = new Appointment { AppointmentId = 7, AppointmentDateTime = RescheduleTests.Now };
        var repository = RepositoryStub.Create<IAppointmentRepository>((nameof(IAppointmentRepository.GetByIdAsync), _ => Task.FromResult<Appointment?>(appointment)));
        var service = new AppointmentService(repository, RepositoryStub.Create<IPatientRepository>(), RepositoryStub.Create<IDentistRepository>(), RepositoryStub.Create<IDentistTimeOffRepository>(), new FixedTimeProvider(RescheduleTests.Now));
        Assert.True((await service.CancelAppointmentAsync(RescheduleTests.Admin, 7, noShow, reason)).Success);
        Assert.Equal(expected, appointment.Status);
    }
    [Fact]
    public void WalkInToggleRoundsTimeAndDefaultsReasonWithoutReplacingText() => PresentationTests.Sta(() =>
    {
        var services = TestServices.Empty;
        var reasons = TestServices.Create<ITreatmentTypeService>((nameof(ITreatmentTypeService.GetVisitReasonsAsync),
            Task.FromResult(ServiceResult<IReadOnlyList<VisitReason>>.Ok([new(ClinicRules.ConsultationReason, 30), new("Root Canal", 90)]))));
        using var page = new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, reasons, RescheduleTests.Admin);
        using var host = new Form { ClientSize = new(1440, 1000) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); PageLayoutTests.ShowOffscreen(host);
        var create = PageLayoutTests.Descendants(page).OfType<AppButton>().Single(b => b.Text == "Schedule");
        PageLayoutTests.InspectNextDialog(host, dialog =>
        {
            var walkIn = PageLayoutTests.Named<Toggle>(dialog, "tglWalkIn");
            var when = PageLayoutTests.Named<DateTimePicker>(dialog, "dtpAppointmentDateTime");
            var reason = PageLayoutTests.Named<ComboBox>(dialog, "cmbReason");
            var before = DateTime.Now; walkIn.Checked = true;
            Assert.False(when.Enabled); Assert.Equal(0, when.Value.Ticks % TimeSpan.TicksPerMinute);
            Assert.InRange((when.Value - before).TotalSeconds, -31, 31); Assert.Equal(ClinicRules.ConsultationReason, reason.Text);
            reason.SelectedIndex = 1;
            var duration = PageLayoutTests.Named<ComboBox>(dialog, "cboDuration"); Assert.Equal(90, duration.SelectedItem);
            duration.SelectedItem = 45; Assert.Equal(45, duration.SelectedItem);
            walkIn.Checked = false; Assert.True(when.Enabled); reason.Text = "Custom reason"; walkIn.Checked = true; Assert.Equal("Custom reason", reason.Text);
        }, create.PerformClick);
    });
    [Fact]
    public void NewTreatmentOnlyListsEligibleAppointmentsAndEditKeepsItsOwn() => PresentationTests.Sta(() =>
    {
        var today = DateTime.Today;
        Appointment[] rows = [new() { AppointmentId = 1, AppointmentDateTime = today.AddDays(-1), Status = AppointmentStatus.Completed },
            new() { AppointmentId = 2, AppointmentDateTime = today, Status = AppointmentStatus.CheckedIn },
            new() { AppointmentId = 3, AppointmentDateTime = today, Status = AppointmentStatus.Cancelled },
            new() { AppointmentId = 4, AppointmentDateTime = today, Status = AppointmentStatus.NoShow },
            new() { AppointmentId = 5, AppointmentDateTime = today.AddDays(1), Status = AppointmentStatus.Scheduled }];
        var appointments = TestServices.Create<IAppointmentService>(
            (nameof(IAppointmentService.GetAllAppointmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Ok(rows))),
            (nameof(IAppointmentService.GetDetailsAsync), Task.FromResult(ServiceResult<AppointmentDetails>.Ok(new(rows[0], new(), new())))));
        var treatments = TestServices.Create<ITreatmentService>((nameof(ITreatmentService.GetAllTreatmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Treatment>>.Ok([new() { TreatmentId = 7, AppointmentId = 5, DatePerformed = today.AddDays(1) }]))));
        using var page = new ucTreatmentRecords(treatments, appointments, TestServices.Empty.TreatmentTypes, RescheduleTests.Admin);
        using var host = new Form { ClientSize = new(1440, 1000) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); PageLayoutTests.ShowOffscreen(host);
        var create = PageLayoutTests.Descendants(page).OfType<AppButton>().Single(b => b.Text == "New treatment");
        PageLayoutTests.InspectNextDialog(host, dialog =>
        {
            var combo = PageLayoutTests.Named<ComboBox>(dialog, "cboAppointment");
            Assert.Equal([1, 2], combo.Items.Cast<DisplayOption>().Select(o => o.Id));
            combo.SelectedValue = 1;
            Assert.Equal(1, combo.SelectedValue);
            var date = PageLayoutTests.Named<DateTimePicker>(dialog, "dtpDatePerformed"); Assert.False(date.Enabled); Assert.Equal(today.AddDays(-1), date.Value.Date);
            Assert.Contains("Checked in", combo.Items.Cast<DisplayOption>().Single(o => o.Id == 2).Display);
        }, create.PerformClick);
        var grid = PageLayoutTests.Named<DataGridView>(page, "dgvTreatments"); grid.CurrentCell = grid.Rows[0].Cells["Patient"]; grid.Rows[0].Selected = true;
        var edit = PageLayoutTests.Descendants(page).OfType<AppButton>().Single(b => b.Text == "Edit treatment");
        PageLayoutTests.InspectNextDialog(host, dialog =>
        {
            var combo = PageLayoutTests.Named<ComboBox>(dialog, "cboAppointment"); Assert.Contains(combo.Items.Cast<DisplayOption>(), o => o.Id == 5);
            Assert.Equal(5, combo.SelectedValue); Assert.Equal(today.AddDays(1), PageLayoutTests.Named<DateTimePicker>(dialog, "dtpDatePerformed").Value.Date);
        }, edit.PerformClick);
    });
}
