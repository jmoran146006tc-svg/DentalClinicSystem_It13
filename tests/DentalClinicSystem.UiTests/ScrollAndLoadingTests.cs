using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.UiTests;

public sealed class ScrollAndLoadingTests
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);
    [Fact]
    public void SegmentedNativeWheelScrollsItsPageWithoutChangingRange() => UiThread.Run(() =>
    {
        using var host = new Form { ClientSize = new(600, 400) };
        var page = new BufferedPanel { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new(0, 1000) };
        var segmented = new AntdUI.Segmented { Size = new(400, 44), Full = true, SelectIndex = 1 };
        segmented.Items.Add(new AntdUI.SegmentedItem { Text = "One" }); segmented.Items.Add(new AntdUI.SegmentedItem { Text = "Two" });
        page.Controls.Add(segmented); host.Controls.Add(page); UiThread.Show(host); segmented.SelectIndex = 1;
        SendMessage(segmented.Handle, 0x020A, (IntPtr)(-120 << 16), IntPtr.Zero);
        Assert.Equal(1, segmented.SelectIndex); Assert.True(page.AutoScrollPosition.Y < 0);
    });
    [Theory]
    [InlineData("choice")] [InlineData("date")] [InlineData("number")]
    public void ClosedInputsRouteWheelToThePageWithoutChangingValues(string kind) => UiThread.Run(() =>
    {
        using var input = kind switch
        {
            "choice" => (Control)new ClinicSelect(), "date" => new ClinicDatePicker(), _ => new ClinicNumber()
        };
        if (input is ClinicSelect choice) { choice.Items.AddRange(["One", "Two", "Three"]); choice.SelectedIndex = 1; }
        if (input is ClinicNumber number) number.Value = 20;
        object Value() => input switch { ClinicSelect c => c.SelectedIndex, ClinicDatePicker d => d.Value, ClinicNumber n => n.Value, _ => throw new InvalidOperationException() };
        using var host = new Form { ClientSize = new(600, 400) };
        var page = new BufferedPanel { Dock = DockStyle.Fill, AutoScroll = true, AutoScrollMinSize = new(0, 1000) };
        page.Controls.Add(input); host.Controls.Add(page); UiThread.Show(host);
        var initial = Value();
        input.GetType().GetMethod("OnMouseWheel", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(input,
            [new HandledMouseEventArgs(MouseButtons.None, 0, 20, 20, -120)]);
        Assert.Equal(initial, Value()); Assert.True(page.AutoScrollPosition.Y < 0);
    });
    [Fact]
    public void OpeningAppointmentsDoesNotValidateTheHiddenBookingForm() => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services();
        var appointments = (ServiceStub)(object)services.Appointments;
        appointments.Results[nameof(IAppointmentService.IsDentistAvailableAsync)] = Task.FromResult(false);
        using var page = new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, services.TreatmentTypes, ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(1200, 800) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        var alert = UiThread.Controls(page).OfType<InlineAlert>().Single();
        Assert.False(alert.Visible);
        Assert.False(appointments.Calls.ContainsKey(nameof(IAppointmentService.IsDentistAvailableAsync)));
        UiThread.Capture(host, "scroll-appointments-first-open");
        UiThread.Named<AppButton>(page, "pageRefresh").PerformClick(); Application.DoEvents();
        Assert.False(alert.Visible);
        Assert.False(appointments.Calls.ContainsKey(nameof(IAppointmentService.IsDentistAvailableAsync)));

        var warnedInEditor = false;
        using var closeEditor = new System.Windows.Forms.Timer { Interval = 20 };
        closeEditor.Tick += (_, _) =>
        {
            if (Application.OpenForms.OfType<RecordDialog>().FirstOrDefault() is not { Enabled: true } dialog) return;
            warnedInEditor = dialog.Alert.Visible && dialog.Alert.Text!.Contains("unavailable");
            closeEditor.Stop(); dialog.Close();
        };
        // Keep the fixture's modal off the visible desktop, like the host form.
        host.Location = new(-20000, -20000);
        closeEditor.Start();
        var layout = (CrudPageLayout)typeof(ucAppointmentScheduler).GetField("_layout", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(page)!;
        layout.NewButton.PerformClick();
        Assert.True(warnedInEditor); Assert.False(alert.Visible);
    });

    [Theory]
    [InlineData(1f)] [InlineData(1.25f)] [InlineData(1.5f)]
    public void ShortSidebarFitsButtonsAndCentersTheLoginMark(float scale) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); using var login = new frmLogin(services);
        using var shell = new frmDashboard(ClinicFixture.Actor(), login, services);
        typeof(frmDashboard).GetField("_loggingOut", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(shell, true);
        shell.WindowState = FormWindowState.Normal;
        UiThread.Show(shell); shell.Scale(new SizeF(scale, scale)); shell.ClientSize = new((int)(1280 * scale), (int)(700 * scale)); Application.DoEvents();
        var navigation = (FlowLayoutPanel)typeof(frmDashboard).GetField("_navigation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(shell)!;
        Assert.False(navigation.HorizontalScroll.Visible);
        Assert.All(navigation.Controls.Cast<Control>(), button =>
        {
            Assert.True(button.Left >= navigation.Padding.Left);
            Assert.True(button.Right <= navigation.ClientSize.Width - navigation.Padding.Right);
        });
        var mark = UiThread.Named<BrandMark>(shell, "sidebarMark");
        var brand = UiThread.Named<TableLayoutPanel>(shell, "sidebarBrand");
        Assert.InRange(Math.Abs(mark.Left + mark.Width / 2d - brand.ClientSize.Width / 2d), 0, 1);
        Assert.All(UiThread.Controls(navigation).OfType<NavItemButton>(), button => Assert.True(button.IconBounds.Left > 0));
        using var menuFont = Typography.PixelFont(Typography.Nav, (int)(Metrics.BaselineDpi * scale));
        Assert.All(UiThread.Controls(shell).OfType<NavItemButton>().Where(button => button.Visible), button =>
        {
            var label = TextRenderer.MeasureText(button.Text, menuFont, Size.Empty, TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            Assert.True(label.Width <= button.TextBounds.Width, $"{button.Text} needs {label.Width}px but has {button.TextBounds.Width}px at {scale:P0} scale.");
        });
        UiThread.Capture(shell, "scroll-sidebar-" + scale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
    });

    [Fact]
    public void SlowTabLoadKeepsALiveSkeletonAndIgnoresALateResponseAfterLeaving() => UiThread.Run(() =>
    {
        MotionSystem.Enabled = true;
        var services = ClinicFixture.Services();
        var ready = new TaskCompletionSource<ServiceResult<IReadOnlyList<Patient>>>();
        ((ServiceStub)(object)services.Patients).Results[nameof(IPatientService.GetAllPatientsAsync)] = ready.Task;
        using var login = new frmLogin(services); using var shell = new frmDashboard(ClinicFixture.Actor(), login, services);
        typeof(frmDashboard).GetField("_loggingOut", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(shell, true);
        shell.WindowState = FormWindowState.Normal; shell.ClientSize = new(1280, 900); UiThread.Show(shell);
        UiThread.Named<AntdUI.Button>(shell, "btnPatients").PerformClick(); Application.DoEvents();
        var page = UiThread.Controls(shell).OfType<ucPatientRecords>().Single();
        Assert.Contains(UiThread.Controls(page).OfType<Skeleton>(), skeleton => skeleton.Visible);
        Assert.False(UiThread.Controls(page).OfType<ClinicTable>().Single().Visible);
        var stop = Environment.TickCount64 + 300;
        while (Environment.TickCount64 < stop) { Application.DoEvents(); Thread.Sleep(10); }
        Assert.Contains(UiThread.Controls(page).OfType<Skeleton>(), skeleton => skeleton.Visible);
        Assert.Empty(UiThread.Controls(page).OfType<LoadingOverlay>());
        UiThread.Capture(shell, "scroll-patients-loading");
        UiThread.Named<AntdUI.Button>(shell, "btnDentists").PerformClick(); Application.DoEvents();
        Assert.True(page.IsDisposed);
        ready.SetResult(ServiceResult<IReadOnlyList<Patient>>.Ok([])); Application.DoEvents();
        Assert.Equal("Dentists - Dental Care", shell.Text);
        Assert.Single(UiThread.Controls(shell).OfType<ucDentistRecords>());
        Assert.DoesNotContain(UiThread.Controls(shell).OfType<Skeleton>(), skeleton => skeleton.Visible);
    });

    [Theory]
    [InlineData(900)] [InlineData(1280)]
    public void RapidReportWheelScrollKeepsOneChartAndStableCardGeometry(int width) => UiThread.Run(() =>
    {
        MotionSystem.Enabled = true;
        var reports = ServiceStub.For<IReportService>(
            (nameof(IReportService.GetAppointmentStatusCountsAsync), Task.FromResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok([new(AppointmentStatus.Completed, 7)]))));
        using var page = new ucReports(reports, ClinicFixture.Actor()); using var host = new Form { ClientSize = new(width, 700) };
        host.Controls.Add(page); UiThread.Show(host);
        var cards = UiThread.Named<TableLayoutPanel>(page, "reportCards");
        Assert.Equal(4, cards.Controls.Count);
        var geometry = cards.Controls.Cast<Control>().Select(card => card.Bounds).ToArray();
        var chart = UiThread.Controls(page).OfType<DonutChart>().Single(); chart.SetAnimationProgress(1);
        var wheel = typeof(BufferedPage).GetMethod("OnMouseWheel", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (var i = 0; i < 40; i++) wheel.Invoke(page, [new MouseEventArgs(MouseButtons.None, 0, width / 2, 350, -120)]);
        Assert.True(page.AutoScrollPosition.Y < 0);
        for (var i = 0; i < 40; i++) wheel.Invoke(page, [new MouseEventArgs(MouseButtons.None, 0, width / 2, 350, 120)]);
        Application.DoEvents();
        Assert.Equal(0, page.AutoScrollPosition.Y);
        Assert.Equal(geometry, cards.Controls.Cast<Control>().Select(card => card.Bounds));
        Assert.Single(UiThread.Controls(page).OfType<DonutChart>());
        Assert.Equal(7, page.Snapshot!.Status.Sum(row => row.Total));
        Assert.False(page.HorizontalScroll.Visible);
        UiThread.Capture(host, "scroll-reports-" + width);
    });
}
