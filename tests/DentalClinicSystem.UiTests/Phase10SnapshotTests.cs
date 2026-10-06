using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public sealed class Phase10SnapshotTests
{
    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)]
    public void CaptureReportsAtScaledSizes(float scale) => UiThread.Run(() =>
    {
        var reports = ServiceStub.For<IReportService>((nameof(IReportService.GetAppointmentStatusCountsAsync),
            Task.FromResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok([new(AppointmentStatus.Completed, 25), new(AppointmentStatus.Scheduled, 4), new(AppointmentStatus.Cancelled, 2)]))));
        using var page = new ucReports(reports, ClinicFixture.Actor()); using var host = new Form { MaximumSize = new(4000, 4000), ClientSize = new(1280, 1000) };
        host.Controls.Add(page); UiThread.Show(host); host.Scale(new SizeF(scale, scale)); Application.DoEvents();
        UiThread.Capture(host, "phase10-reports-" + scale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
    });
    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)]
    public void CaptureCalendarWorklistAndAppointmentDialogs(float scale) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); var actor = ClinicFixture.Actor();
        var patient = new Patient { PatientId = 3, FirstName = "Ana", LastName = "Santos", DateOfBirth = new(1998, 4, 12), ContactNumber = "09171234567", Email = "ana@example.test" };
        var dentist = new Dentist { DentistId = 2, FirstName = "Miguel", LastName = "Reyes" };
        var appointment = new Appointment { AppointmentId = 4, PatientId = 3, DentistId = 2,
            AppointmentDateTime = DateTime.Today.AddHours(14), DurationMinutes = 45, Reason = "Dental Cleaning" };
        var details = new AppointmentDetails(appointment, patient, dentist);
        var suffix = scale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
        using (var calendar = new ucWeekCalendar { Dock = DockStyle.Top })
        using (var host = new Form { MaximumSize = new(4000, 4000), ClientSize = new(1280, 900) })
        {
            host.Controls.Add(calendar); UiThread.Show(host);
            var late = new Appointment { AppointmentId = 5, PatientId = 3, DentistId = 2, AppointmentDateTime = DateTime.Today.AddHours(20), DurationMinutes = 120 };
            calendar.SetAppointments(DashboardPresentation.WeekStart(DateTime.Today), DateTime.Today, [appointment],
                new Dictionary<int, string> { [3] = patient.FullName }, new Dictionary<int, string> { [2] = dentist.FullName });
            host.Scale(new SizeF(scale, scale)); Application.DoEvents(); UiThread.Capture(host, "phase10-calendar-" + suffix);
            appointment.AppointmentDateTime = DateTime.Today.AddHours(7); appointment.DurationMinutes = 180;
            calendar.SetAppointments(DashboardPresentation.WeekStart(DateTime.Today), DateTime.Today, [appointment, late],
                new Dictionary<int, string> { [3] = patient.FullName }, new Dictionary<int, string> { [2] = dentist.FullName });
            var canvas = UiThread.Controls(calendar).OfType<WeekCalendarCanvas>().Single();
            ((ScrollableControl)canvas.Parent!).AutoScrollPosition = new(0, canvas.HourHeight * 4);
            Application.DoEvents(); UiThread.Capture(host, "phase10-calendar-scrolled-" + suffix);
        }
        appointment.AppointmentDateTime = DateTime.Today.AddHours(14); appointment.DurationMinutes = 45;
        using (var worklist = new DentistWorklist([details], ClinicFixture.Actor(Roles.Dentist), DateTime.Today.AddHours(10)))
        using (var host = new Form { MaximumSize = new(4000, 4000), ClientSize = new(1280, 900) })
        {
            host.Controls.Add(worklist); UiThread.Show(host); host.Scale(new SizeF(scale, scale));
            Application.DoEvents(); UiThread.Capture(host, "phase10-worklist-" + suffix);
            var card = UiThread.Controls(worklist).OfType<WorklistCard>().Single();
            typeof(Control).GetMethod("OnMouseEnter", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(card, [EventArgs.Empty]);
            Application.DoEvents(); UiThread.Capture(host, "phase10-worklist-hover-" + suffix);
        }
        using (var dialog = new frmAppointmentDetails(details, services.Appointments, actor, _ => Task.CompletedTask, services.Dentists))
        {
            UiThread.Show(dialog); dialog.Scale(new SizeF(scale, scale)); Application.DoEvents();
            UiThread.Capture(dialog, "phase10-details-" + suffix);
        }
        using (var dialog = new frmRescheduleAppointment(appointment, services.Appointments, services.Dentists, actor))
        {
            UiThread.Show(dialog); dialog.Scale(new SizeF(scale, scale)); Application.DoEvents();
            UiThread.Capture(dialog, "phase10-reschedule-" + suffix);
        }
    });
}
