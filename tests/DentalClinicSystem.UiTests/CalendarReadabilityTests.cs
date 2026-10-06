using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public sealed class CalendarReadabilityTests
{
    [Theory]
    [InlineData(1100, 96)] [InlineData(1100, 120)] [InlineData(1100, 144)]
    [InlineData(1440, 96)] [InlineData(1440, 120)] [InlineData(1440, 144)]
    public void DayAndWeekAreReadableAtWindowWidthsAndDpi(int width, int dpi) => UiThread.Run(() =>
    {
        using var dpiContext = new DpiContext();
        using var calendar = new ucWeekCalendar { Dock = DockStyle.Top };
        using var host = new Form { MaximumSize = new(4000, 4000), ClientSize = new(width, 1400) };
        host.Controls.Add(calendar); UiThread.Show(host); ResizeCapture(host, new(width, 1400)); SimulateDpi(calendar, dpi);
        Bind(calendar); calendar.Height = calendar.PreferredCalendarHeight; Application.DoEvents();
        var canvas = UiThread.Controls(calendar).OfType<WeekCalendarCanvas>().Single();
        var header = UiThread.Controls(calendar).OfType<WeekCalendarHeader>().Single();
        var gutter = UiThread.Controls(calendar).OfType<CalendarTimeGutter>().Single();
        var viewport = (BufferedPanel)canvas.Parent!;
        Assert.Equal(CalendarView.Day, calendar.View); Assert.Equal(CalendarDemo.Today, calendar.SelectedDay);
        Assert.Equal(5, canvas.Columns.Count); Assert.Equal(30, canvas.Blocks.Count);
        Assert.Equal(canvas.PointToScreen(Point.Empty).X, header.PointToScreen(Point.Empty).X);
        Assert.False(viewport.VerticalScroll.Visible);
        AssertText(canvas); EnglishUi.AssertTree(calendar);
        UiThread.Capture(host, $"calendar-day-{width}-{dpi * 100 / 96}");
        var origin = gutter.PointToScreen(Point.Empty);
        if (viewport.HorizontalScroll.Visible)
        {
            viewport.AutoScrollPosition = new(100, 0); Application.DoEvents();
            Assert.Equal(origin, gutter.PointToScreen(Point.Empty)); Assert.Equal(canvas.Left, header.Left);
            UiThread.Capture(host, $"calendar-day-scrolled-{width}-{dpi * 100 / 96}");
        }
        calendar.View = CalendarView.Week; calendar.Height = calendar.PreferredCalendarHeight; Application.DoEvents();
        var summary = UiThread.Controls(calendar).OfType<WeekSummaryCanvas>().Single();
        Assert.True(summary.Visible); Assert.False(canvas.Visible); Assert.Equal(30, summary.Cells.Count);
        Assert.DoesNotContain(summary.Cells, c => c.Cell.Date.DayOfWeek == DayOfWeek.Sunday);
        Assert.Equal(summary.PointToScreen(new Point(summary.LabelWidth, 0)).X, header.PointToScreen(Point.Empty).X);
        Assert.False(viewport.HorizontalScroll.Visible); Assert.False(viewport.VerticalScroll.Visible);
        UiThread.Capture(host, $"calendar-week-summary-{width}-{dpi * 100 / 96}");
        calendar.SelectedDentistId = 2; calendar.Height = calendar.PreferredCalendarHeight; Application.DoEvents();
        Assert.False(summary.Visible); Assert.True(canvas.Visible);
        Assert.False(viewport.HorizontalScroll.Visible, $"Calendar {calendar.Size} preferred {calendar.PreferredCalendarHeight}, viewport {viewport.ClientSize}, display {viewport.DisplayRectangle}, canvas {canvas.Bounds}, summary {summary.Bounds}, dpi {canvas.DeviceDpi}");
        Assert.Equal(6, canvas.Columns.Count); AssertText(canvas);
        UiThread.Capture(host, $"calendar-week-dentist-{width}-{dpi * 100 / 96}");
    });
    [Theory]
    [InlineData(96)] [InlineData(120)] [InlineData(144)]
    public void FullScreenFitsAvailableHeightAndCarriesState(int dpi) => UiThread.Run(() =>
    {
        using var dpiContext = new DpiContext();
        using var source = new ucWeekCalendar(); Bind(source); source.SelectedDentistId = 2;
        using var form = new frmCalendarFullScreen(source, (_, _, _) => Task.FromResult(true), (_, _) => Task.CompletedTask);
        Assert.Equal(FormWindowState.Maximized, form.WindowState);
        // A fixed off-screen viewport makes captures independent of the monitor.
        form.WindowState = FormWindowState.Normal; form.MaximumSize = new(4000, 4000); form.ClientSize = new(1920, 1440);
        UiThread.Show(form); Application.DoEvents();
        form.WindowState = FormWindowState.Normal; Application.DoEvents(); form.ClientSize = new(1920, 1440); Application.DoEvents();
        SimulateDpi(form, dpi); ResizeCapture(form, new(1920, 1440)); Application.DoEvents();
        var calendar = form.Calendar; var canvas = UiThread.Controls(calendar).OfType<WeekCalendarCanvas>().Single();
        var viewport = (BufferedPanel)canvas.Parent!;
        Assert.Equal(CalendarView.Day, calendar.View); Assert.Equal(CalendarDemo.Today, calendar.SelectedDay); Assert.Equal(2, calendar.SelectedDentistId);
        Assert.False(UiThread.Named<AppButton>(calendar, "calendarExpand").Visible);
        Assert.True(canvas.HourHeight >= CalendarGeometry.Scale(Metrics.CalendarDayHourHeight, dpi), $"DPI {canvas.DeviceDpi}, hour {canvas.HourHeight}");
        Assert.False(viewport.VerticalScroll.Visible, $"Form {form.ClientSize} {form.WindowState}, calendar {calendar.Bounds}, viewport {viewport.ClientSize}, display {viewport.DisplayRectangle}, canvas {canvas.Bounds}, hours {canvas.HourHeight}");
        Assert.False(viewport.HorizontalScroll.Visible, $"Viewport {viewport.ClientSize}, display {viewport.DisplayRectangle}, canvas {canvas.Bounds}"); AssertText(canvas); EnglishUi.AssertTree(form);
        UiThread.Capture(form, $"calendar-full-screen-1920-{dpi * 100 / 96}");
        calendar.SelectedDentistId = 0; Application.DoEvents(); AssertText(canvas);
        UiThread.Capture(form, $"calendar-full-screen-all-1920-{dpi * 100 / 96}");
        var args = new object[] { Message.Create(form.Handle, 0, IntPtr.Zero, IntPtr.Zero), Keys.Escape };
        Assert.True((bool)typeof(frmCalendarFullScreen).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, args)!);
        Assert.True(form.IsDisposed);
    });
    [Fact]
    public void MatrixClickDrillsDownAndKeyboardMovesWithinDentistColumns() => UiThread.Run(() =>
    {
        using var calendar = new ucWeekCalendar { Dock = DockStyle.Top };
        using var host = new Form { ClientSize = new(1440, 1000) }; host.Controls.Add(calendar); UiThread.Show(host); Bind(calendar);
        calendar.View = CalendarView.Week;
        var summary = UiThread.Controls(calendar).OfType<WeekSummaryCanvas>().Single();
        var cell = summary.Cells.First(c => c.Cell.Date == CalendarDemo.Today.AddDays(1));
        MouseDown(summary, new(cell.Bounds.Left + 10, cell.Bounds.Top + 10));
        Assert.Equal(CalendarView.Day, calendar.View); Assert.Equal(cell.Cell.Date, calendar.SelectedDay);
        var canvas = UiThread.Controls(calendar).OfType<WeekCalendarCanvas>().Single(); canvas.Focus();
        var activated = 0; calendar.AppointmentActivated += id => activated = id;
        Key(canvas, Keys.Enter); var first = canvas.Blocks.Single(b => b.Appointment.AppointmentId == activated).Appointment;
        Key(canvas, Keys.Right); Key(canvas, Keys.Enter);
        var right = canvas.Blocks.Single(b => b.Appointment.AppointmentId == activated).Appointment;
        Assert.NotEqual(first.DentistId, right.DentistId); Assert.Equal(first.AppointmentDateTime, right.AppointmentDateTime);
        Key(canvas, Keys.Down); Key(canvas, Keys.Enter);
        var down = canvas.Blocks.Single(b => b.Appointment.AppointmentId == activated).Appointment;
        Assert.Equal(right.DentistId, down.DentistId); Assert.True(down.AppointmentDateTime > right.AppointmentDateTime);
        Key(canvas, Keys.Up); Key(canvas, Keys.Enter); Assert.Equal(right.AppointmentId, activated);
    });
    [Fact]
    public void NavigationLoadsOnlyAcrossWeekBoundariesAndPreservesSelection() => UiThread.Run(() =>
    {
        using var calendar = new ucWeekCalendar(); Bind(calendar); calendar.SelectedDentistId = 3;
        var requests = new List<(DateTime Week, int Direction)>();
        calendar.WeekRequested += (week, direction) =>
        {
            requests.Add((week, direction));
            calendar.SetAppointments(week, CalendarDemo.Today, [], CalendarDemo.Patients, CalendarDemo.Dentists);
        };
        Click(calendar, "calendarNext"); Assert.Empty(requests); Assert.Equal(CalendarDemo.Today.AddDays(1), calendar.SelectedDay);
        calendar.SelectedDay = CalendarDemo.Monday.AddDays(6); Click(calendar, "calendarNext");
        Assert.Equal((CalendarDemo.Monday.AddDays(7), 1), Assert.Single(requests));
        Assert.Equal(CalendarDemo.Monday.AddDays(7), calendar.SelectedDay); Assert.Equal(3, calendar.SelectedDentistId); Assert.Equal(CalendarView.Day, calendar.View);
        calendar.View = CalendarView.Week; Click(calendar, "calendarPrevious");
        Assert.Equal(CalendarDemo.Monday, calendar.WeekStart); Assert.Equal(3, calendar.SelectedDentistId); Assert.Equal(CalendarView.Week, calendar.View);
        Click(calendar, "calendarToday"); Assert.Equal(CalendarDemo.Today, calendar.SelectedDay);
        using var other = new ucWeekCalendar(); other.SetAppointments(CalendarDemo.Monday.AddDays(7), CalendarDemo.Today, [], CalendarDemo.Patients, CalendarDemo.Dentists);
        Assert.Equal(CalendarDemo.Monday.AddDays(7), other.SelectedDay);
    });
    [Fact]
    public void DashboardExpandOpensFullScreenAndEscapeReturnsToDashboard() => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services();
        ((ServiceStub)services.Appointments).Results[nameof(IAppointmentService.GetAppointmentsInRangeAsync)] = Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Ok(CalendarDemo.Appointments));
        ((ServiceStub)services.Dentists).Results[nameof(IDentistService.GetAllDentistsAsync)] = Task.FromResult(ServiceResult<IReadOnlyList<Dentist>>.Ok(
            CalendarDemo.Dentists.Select(d => new Dentist { DentistId = d.Key, FirstName = d.Value[4..].Split(' ')[0], LastName = string.Join(" ", d.Value[4..].Split(' ').Skip(1)), IsActive = true }).ToArray()));
        using var page = new ucDashboardHome(services.Appointments, services.Patients, services.Dentists, services.Reports, services.PatientHistory, ClinicFixture.Actor(), new CalendarTime());
        using var host = new Form { ClientSize = new(1440, 1000) }; host.Controls.Add(page); UiThread.Show(host);
        var calendar = UiThread.Controls(page).OfType<ucWeekCalendar>().Single(); calendar.View = CalendarView.Week; calendar.SelectedDentistId = 2;
        Exception? error = null; var opened = false;
        using var timer = new System.Windows.Forms.Timer { Interval = 100 };
        timer.Tick += (_, _) =>
        {
            var full = Application.OpenForms.OfType<frmCalendarFullScreen>().SingleOrDefault(); if (full is null) return;
            timer.Stop(); opened = true;
            try
            {
                Assert.Equal(calendar.View, full.Calendar.View); Assert.Equal(calendar.SelectedDay, full.Calendar.SelectedDay); Assert.Equal(2, full.Calendar.SelectedDentistId);
                Assert.Equal(FormWindowState.Maximized, full.WindowState); EnglishUi.AssertTree(full);
                var args = new object[] { Message.Create(full.Handle, 0, IntPtr.Zero, IntPtr.Zero), Keys.Escape };
                typeof(frmCalendarFullScreen).GetMethod("ProcessCmdKey", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(full, args);
            }
            catch (Exception failure) { error = failure; }
            finally { full.Close(); }
        };
        timer.Start(); Click(calendar, "calendarExpand"); Application.DoEvents();
        Assert.True(opened); Assert.Null(error); Assert.Empty(Application.OpenForms.OfType<frmCalendarFullScreen>());
        Assert.Equal(2, calendar.SelectedDentistId); Assert.Equal(CalendarView.Week, calendar.View);
    });
    [Fact]
    public void FullScreenUsesSharedLoaderAndDetailsDelegate() => UiThread.Run(() =>
    {
        using var source = new ucWeekCalendar(); Bind(source); source.SelectedDentistId = 2;
        var loads = new List<(ucWeekCalendar Target, DateTime Week, int Direction)>();
        var opened = 0; IWin32Window? detailsOwner = null;
        using var form = new frmCalendarFullScreen(source, (target, week, direction) =>
        {
            loads.Add((target, week, direction));
            target.SetAppointments(week, CalendarDemo.Today, CalendarDemo.Appointments, CalendarDemo.Patients, CalendarDemo.Dentists);
            return Task.FromResult(true);
        }, (id, owner) => { opened = id; detailsOwner = owner; return Task.CompletedTask; });
        var canvas = UiThread.Controls(form.Calendar).OfType<WeekCalendarCanvas>().Single();
        var block = canvas.Blocks[0]; MouseDown(canvas, new(block.Bounds.Left + 2, block.Bounds.Top + 2));
        Assert.Equal(block.Appointment.AppointmentId, opened); Assert.Same(form, detailsOwner);
        Assert.Same(form.Calendar, Assert.Single(loads).Target);
        Click(form.Calendar, "calendarNext"); Assert.Single(loads);
        form.Calendar.SelectedDay = CalendarDemo.Monday.AddDays(6); Click(form.Calendar, "calendarNext");
        Assert.Equal(2, loads.Count); Assert.Same(form.Calendar, loads[1].Target);
        Assert.Equal(CalendarDemo.Monday.AddDays(7), loads[1].Week); Assert.Equal(1, loads[1].Direction);
        Assert.Equal(CalendarDemo.Monday.AddDays(7), form.Calendar.SelectedDay); Assert.Equal(2, form.Calendar.SelectedDentistId);
    });
    private sealed class CalendarTime : TimeProvider
    { public override DateTimeOffset GetUtcNow() => new(CalendarDemo.Today.AddHours(2), TimeSpan.Zero); public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time"); }
    private static void Bind(ucWeekCalendar calendar) => calendar.SetAppointments(CalendarDemo.Monday, CalendarDemo.Today, CalendarDemo.Appointments, CalendarDemo.Patients, CalendarDemo.Dentists);
    private static void AssertText(WeekCalendarCanvas canvas)
    {
        Assert.NotEmpty(canvas.BlockContent);
        foreach (var content in canvas.BlockContent)
        {
            Assert.Contains(CalendarDemo.Patients[content.Block.Appointment.PatientId], content.Lines[0].Text);
            Assert.All(content.Lines, line => Assert.True(content.Block.Bounds.Contains(line.Bounds), $"Text outside appointment #{content.Block.Appointment.AppointmentId}"));
            using var caption = Typography.PixelFont(Typography.Caption, canvas.DeviceDpi);
            Assert.All(content.Lines, line => Assert.True(line.Bounds.Height >= TextRenderer.MeasureText("Ag", caption).Height));
        }
    }
    private static void Click(Control root, string name) => typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(UiThread.Named<AppButton>(root, name), [EventArgs.Empty]);
    private static void Key(Control control, Keys key) => control.GetType().GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(control, [new KeyEventArgs(key)]);
    private static void MouseDown(Control control, Point point) => control.GetType().GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(control, [new MouseEventArgs(MouseButtons.Left, 1, point.X, point.Y, 0)]);
    private static void SimulateDpi(Control root, int dpi)
    {
        root.Scale(new SizeF(dpi / 96f, dpi / 96f));
        var property = typeof(Control).GetProperty("DeviceDpiInternal", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var controls = UiThread.Controls(root).Prepend(root).ToArray();
        foreach (var control in controls) property.SetValue(control, dpi);
        foreach (var control in controls.Reverse()) typeof(Control).GetMethod("OnDpiChangedAfterParent", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(control, [EventArgs.Empty]);
        Application.DoEvents();
        Assert.All(controls, c => Assert.Equal(dpi, c.DeviceDpi));
    }
    private sealed class DpiContext : IDisposable
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
        private readonly IntPtr _previous = SetThreadDpiAwarenessContext(new(-4));
        private readonly FieldInfo _processAware = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("System.Windows.Forms.ScaleHelper"))
            .First(t => t is not null)!.GetField("s_processPerMonitorAware", BindingFlags.Static | BindingFlags.NonPublic)!;
        private readonly bool _wasAware;
        public DpiContext()
        {
            // The test host may have initialized WinForms before this thread's
            // DPI context. Scope its cached gate as well as the native context.
            _wasAware = (bool)_processAware.GetValue(null)!; _processAware.SetValue(null, true);
        }
        public void Dispose() { _processAware.SetValue(null, _wasAware); SetThreadDpiAwarenessContext(_previous); }
    }
    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr window, IntPtr after, int x, int y, int width, int height, uint flags);
    private static void ResizeCapture(Form form, Size client)
    {
        // Form.SetBoundsCore caps sizes at the test monitor's tracking limits.
        // Resize this off-screen test HWND directly to render the requested size.
        var border = form.Size - form.ClientSize;
        Assert.True(SetWindowPos(form.Handle, IntPtr.Zero, 0, 0, client.Width + border.Width, client.Height + border.Height, 0x0016));
        Application.DoEvents(); Assert.Equal(client, form.ClientSize);
        Assert.Equal(new Size(client.Width - form.Padding.Horizontal, client.Height - form.Padding.Vertical), form.DisplayRectangle.Size);
    }
}
