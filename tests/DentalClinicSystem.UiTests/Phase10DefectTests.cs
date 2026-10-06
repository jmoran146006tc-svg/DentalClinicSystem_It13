using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public sealed class Phase10DefectTests
{
    [Fact]
    public void CalendarHeaderStaysOutsideViewportAndNormalHoursFit() => UiThread.Run(() =>
    {
        using var calendar = new ucWeekCalendar { Dock = DockStyle.Top };
        using var host = new Form { ClientSize = new(1280, 900) }; host.Controls.Add(calendar); UiThread.Show(host);
        var monday = DashboardPresentation.WeekStart(DateTime.Today);
        var appointment = new Appointment { AppointmentId = 1, PatientId = 3, DentistId = 2, AppointmentDateTime = DateTime.Today.AddHours(14), DurationMinutes = 45 };
        calendar.SetAppointments(monday, DateTime.Today, [appointment], new Dictionary<int, string> { [3] = "Ana Santos" }, new Dictionary<int, string> { [2] = "Dr. Miguel Reyes" });
        var canvas = UiThread.Controls(calendar).OfType<WeekCalendarCanvas>().Single();
        var header = UiThread.Controls(calendar).OfType<WeekCalendarHeader>().Single();
        var viewport = Assert.IsType<BufferedPanel>(canvas.Parent);
        Assert.False(viewport.Contains(header)); Assert.Equal(ClinicRules.OpeningTime.Hour, canvas.StartHour);
        Assert.Equal(ClinicRules.ClosingTime.Hour, canvas.EndHour);
        Assert.False(viewport.VerticalScroll.Visible, $"Calendar {calendar.Size}, preferred {calendar.PreferredCalendarHeight}, viewport {viewport.ClientSize}, canvas {canvas.Bounds}");
        Assert.Equal(canvas.Width, header.Width);
        var entry = ((System.Collections.IEnumerable)typeof(WeekCalendarCanvas).GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(canvas)!).Cast<object>().Single();
        Assert.Equal("2:00–2:45 PM", (string)entry.GetType().GetProperty("Slot")!.GetValue(entry)!);
        var position = header.PointToScreen(Point.Empty);
        appointment.AppointmentDateTime = DateTime.Today.AddHours(7); calendar.SetAppointments(monday, DateTime.Today, [appointment], new Dictionary<int, string>(), new Dictionary<int, string>());
        Assert.Equal(7, canvas.StartHour); Assert.True(viewport.VerticalScroll.Visible);
        viewport.AutoScrollPosition = new(0, Metrics.CalendarHourHeight); Application.DoEvents();
        Assert.Equal(position, header.PointToScreen(Point.Empty)); Assert.Equal(canvas.Width, header.Width);
        var timer = (System.Windows.Forms.Timer)typeof(WeekCalendarCanvas).GetField("_nowTimer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(canvas)!;
        Assert.True(timer.Enabled); calendar.Hide(); Assert.False(timer.Enabled);
    });
    [Fact]
    public void CalendarWheelChainsToOuterPageAtBothLimits() => UiThread.Run(() =>
    {
        using var host = new Form { ClientSize = new(600, 400) };
        var page = new BufferedPage { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new(0, 1200) };
        var inner = new BufferedPanel { Size = new(500, 200), AutoScroll = true, AutoScrollMinSize = new(0, 800) };
        page.Controls.Add(inner); host.Controls.Add(page); UiThread.Show(host);
        inner.AutoScrollPosition = new(0, 10000);
        var wheel = typeof(BufferedPanel).GetMethod("OnMouseWheel", BindingFlags.Instance | BindingFlags.NonPublic)!;
        wheel.Invoke(inner, [new HandledMouseEventArgs(MouseButtons.None, 0, 20, 20, -120)]);
        Assert.True(page.AutoScrollPosition.Y < 0);
        inner.AutoScrollPosition = Point.Empty;
        wheel.Invoke(inner, [new HandledMouseEventArgs(MouseButtons.None, 0, 20, 20, 120)]);
        Assert.Equal(0, page.AutoScrollPosition.Y);
    });
    [Theory]
    [InlineData("donut")] [InlineData("line")] [InlineData("bar")]
    public void ChartsShowKeyboardFocusButNotProgrammaticOrMouseFocus(string kind) => UiThread.Run(() =>
    {
        using Control chart = kind switch { "donut" => new DonutChart(), "line" => new LineChart(), _ => new BarChart() };
        using var host = new Form { ClientSize = new(600, 400) };
        var other = new System.Windows.Forms.Button { Text = "Before chart", Top = 300 };
        host.Controls.AddRange([other, chart]); UiThread.Show(host); other.Focus(); chart.Focus();
        var state = chart.GetType().GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(chart)!;
        var keyboard = state.GetType().GetField("_keyboardFocus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        Assert.False((bool)keyboard.GetValue(state)!);
        other.Focus(); var tab = Message.Create(host.Handle, 0x0100, (IntPtr)Keys.Tab, IntPtr.Zero);
        ((IMessageFilter)state).PreFilterMessage(ref tab); chart.Focus(); Assert.True((bool)keyboard.GetValue(state)!);
        typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(chart, [new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0)]);
        Assert.False((bool)keyboard.GetValue(state)!); other.Focus(); Assert.False((bool)keyboard.GetValue(state)!);
    });
    public static IEnumerable<object[]> FooterStates => new[] { Roles.Admin, Roles.Receptionist, Roles.Dentist }
        .SelectMany(role => AppointmentStatus.All.Select(status => new object[] { role, status }));
    [Theory]
    [MemberData(nameof(FooterStates))]
    public void DetailsFooterNeverWrapsAcrossStatusesAndScaledLayouts(string role, string status) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services();
        foreach (var scale in new[] { 1f, 1.25f, 1.5f })
        {
            var appointment = new Appointment { AppointmentId = 1, DentistId = 2, AppointmentDateTime = DateTime.Today.AddHours(14), Status = status };
            using var dialog = new frmAppointmentDetails(new(appointment, new() { FirstName = "Ana", LastName = "Santos" }, new()),
                services.Appointments, ClinicFixture.Actor(role), _ => Task.CompletedTask, services.Dentists);
            UiThread.Show(dialog); dialog.Scale(new SizeF(scale, scale)); Application.DoEvents();
            var left = UiThread.Named<FlowLayoutPanel>(dialog, "dialogLeftActions");
            var right = UiThread.Named<FlowLayoutPanel>(dialog, "dialogRightActions");
            var actions = UiThread.Controls(dialog.Footer).OfType<AppButton>().Where(b => b.Visible).ToArray();
            Assert.NotEmpty(actions);
            var tops = actions.Select(b => b.PointToScreen(Point.Empty).Y).ToArray(); Assert.InRange(tops.Max() - tops.Min(), 0, 1);
            Assert.Single(actions.Select(b => b.Height).Distinct());
            Assert.All(actions, b => Assert.True(dialog.Footer.RectangleToScreen(dialog.Footer.ClientRectangle).Contains(b.RectangleToScreen(b.ClientRectangle)), $"{role}/{status}/{scale}: {b.Text} is clipped"));
            Assert.Equal("Close", right.Controls.OfType<AppButton>().First(b => b.Visible).Text);
            if (dialog.ConfirmButton.Visible) Assert.Equal(dialog.ConfirmButton, right.Controls.OfType<AppButton>().Last(b => b.Visible));
            Assert.Equal(status == AppointmentStatus.Scheduled && RoleAccess.CanChangeStatus(ClinicFixture.Actor(role), appointment, AppointmentStatus.Cancelled),
                left.Controls.OfType<AppButton>().First().Visible);
            EnglishUi.AssertTree(dialog);
        }
    });
    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)]
    public void RescheduleFieldsFitAndDatePrecedesDentist(float scale) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services();
        using var dialog = new frmRescheduleAppointment(new() { AppointmentDateTime = DateTime.Today.AddHours(14), DentistId = 2 }, services.Appointments, services.Dentists, ClinicFixture.Actor());
        Assert.True(dialog.Height > Metrics.DialogHeight);
        UiThread.Show(dialog); dialog.Scale(new SizeF(scale, scale)); Application.DoEvents();
        var date = UiThread.Controls(dialog).OfType<ClinicDatePicker>().Single(); var dentist = UiThread.Controls(dialog).OfType<ClinicSelect>().Single();
        Assert.True(date.PointToScreen(Point.Empty).Y < dentist.PointToScreen(Point.Empty).Y);
        foreach (var field in new Control[] { date, dentist }) Assert.True(dialog.Body.RectangleToScreen(dialog.Body.ClientRectangle).Contains(field.RectangleToScreen(field.ClientRectangle)));
        Assert.Equal("Dental Care", dialog.Text); Assert.Equal("Reschedule appointment", dialog.AccessibleName); EnglishUi.AssertTree(dialog);
        using var confirmation = new ConfirmDialog("A confirmation"); Assert.Equal(new Size(Metrics.DialogWidth, Metrics.DialogHeight), confirmation.Size);
    });
    [Theory]
    [InlineData(1280, 2)] [InlineData(800, 1)]
    public void WorklistCardsAreCompactAndHoverKeepsBounds(int width, int columns) => UiThread.Run(() =>
    {
        var appointment = new Appointment { AppointmentId = 1, DentistId = 2, AppointmentDateTime = DateTime.Today.AddHours(14), Reason = "Dental Cleaning", DurationMinutes = 45 };
        using var worklist = new DentistWorklist([new(appointment, new() { FirstName = "Ana", LastName = "Santos" }, new())], ClinicFixture.Actor(Roles.Dentist), DateTime.Today.AddHours(10));
        using var host = new Form { ClientSize = new(width, 900) }; host.Controls.Add(worklist); UiThread.Show(host);
        var card = UiThread.Controls(worklist).OfType<WorklistCard>().Single(); var bounds = card.Bounds;
        Assert.Equal(columns, ((TableLayoutPanel)card.Parent!).ColumnCount); Assert.InRange(card.Height, Metrics.WorklistHeight, 100);
        Assert.Equal(ElevationLevel.E0, card.Elevation); Assert.Equal(Palette.BrandSoft, card.FaceColor); Assert.Equal(card.FaceColor, card.Content.BackColor);
        typeof(Control).GetMethod("OnMouseEnter", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(card, [EventArgs.Empty]);
        Application.DoEvents(); Assert.Equal(bounds, card.Bounds);
    });
}
