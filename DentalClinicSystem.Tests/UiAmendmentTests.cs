using System.Reflection;
using System.Runtime.InteropServices;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Tests;

public class UiAmendmentTests
{
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    private static void Capture(Control control, string name)
    {
        var directory = Environment.GetEnvironmentVariable("DENTAL_UI_SNAPSHOTS"); if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory); using var bitmap = new Bitmap(control.Width, control.Height);
        control.DrawToBitmap(bitmap, control.ClientRectangle); bitmap.Save(Path.Combine(directory, name + ".png"));
    }
    [Fact]
    public void MarkIsWhiteTransparentAndKeepsAntialiasedCutouts() => PresentationTests.Sta(() =>
    {
        using var brand = new LoginBrand(); using var mark = (Bitmap)brand.LogoImage!.Clone();
        var transparent = 0; var antialias = 0; var opaque = 0;
        for (var y = 0; y < mark.Height; y++) for (var x = 0; x < mark.Width; x++)
        {
            var pixel = mark.GetPixel(x, y);
            if (pixel.A == 0) transparent++;
            else { Assert.Equal(255, pixel.R); Assert.Equal(255, pixel.G); Assert.Equal(255, pixel.B); if (pixel.A == 255) opaque++; else antialias++; }
        }
        Assert.True(transparent > 0 && antialias > 0 && opaque > 0);
        // The 90% local white shape meets 4.5 even over a pure black pixel.
        Assert.True(Contrast.Ratio(Palette.Ink900, Color.FromArgb(230, 230, 230)) >= 4.5);
    });
    [Theory]
    [InlineData(96)] [InlineData(120)] [InlineData(144)]
    public void FieldActionsStayInsideContinuousChromeAtScaledDpi(int dpi) => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        var previousDpiContext = SetThreadDpiAwarenessContext((IntPtr)(-4));
        // VSTest may initialize WinForms as DPI unaware before this case. Scope
        // its awareness cache along with the thread context for deterministic
        // simulated DPI; restore it before any other test creates a window.
        var scaleHelper = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType("System.Windows.Forms.ScaleHelper")).First(type => type is not null)!;
        var awareness = scaleHelper.GetField("s_processPerMonitorAware", BindingFlags.Static | BindingFlags.NonPublic)!;
        var previousAwareness = awareness.GetValue(null); awareness.SetValue(null, true);
        try
        {
            using var host = new Form { ClientSize = new(700, 550), BackColor = Palette.Canvas };
            var column = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Palette.Canvas };
            host.Controls.Add(column);
            var password = UiFactory.Field(new TextBox { Text = "sample" }, "Password", FieldKind.Password);
            var combo = new ComboBox(); combo.Items.AddRange(["One", "Two"]); combo.SelectedIndex = 0;
            var choice = UiFactory.Field(combo, "Choice", FieldKind.Choice);
            var date = UiFactory.Field(new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = DisplayFormat.DatePattern }, "Date", FieldKind.Date);
            column.Controls.AddRange([password, choice, date]); PageLayoutTests.ShowOffscreen(host);
            // Controlled DPI layout simulation, independent of the host monitor.
            var dpiProperty = typeof(Control).GetProperty("DeviceDpiInternal", BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (var field in new[] { password, choice, date })
            {
                foreach (var control in new[] { (Control)field }.Concat(PageLayoutTests.Descendants(field))) dpiProperty.SetValue(control, dpi);
                field.Width = 380 * dpi / 96;
                typeof(Control).GetMethod("OnDpiChangedAfterParent", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(field.Box, [EventArgs.Empty]);
                field.PerformLayout();
                Assert.Equal(dpi, field.Box.DeviceDpi);
                Assert.Equal(Color.Transparent, field.BackColor);
                Assert.True(field.Box.Input.Parent!.Height >= field.Box.Input.Height - Metrics.Scale(field.Box, 8));
            }
            var eye = PageLayoutTests.Descendants(password.Box).OfType<Button>().Single();
            Assert.True(eye.Right <= password.Box.Width - Metrics.Scale(password.Box, Space.Md));
            Assert.True(eye.Top >= Metrics.Scale(password.Box, Space.Xs));
            password.Box.Input.Focus(); Application.DoEvents(); Capture(host, $"fields-{dpi}-focused");
            // The empty portion of the selected-value viewport must be uniform:
            // native flat adapters used to leave a horizontal frame line here.
            using (var valuePaint = new Bitmap(choice.Box.Width, choice.Box.Height))
            {
                choice.Box.DrawToBitmap(valuePaint, choice.Box.ClientRectangle);
                var viewport = combo.Parent!.Bounds;
                for (var y = viewport.Top + 2; y < viewport.Bottom - 2; y++)
                    for (var x = viewport.Left + viewport.Width / 2; x < viewport.Right - 2; x++)
                        Assert.True(Palette.Surface.ToArgb() == valuePaint.GetPixel(x, y).ToArgb(),
                            $"DPI {dpi}: native chrome at ({x},{y}), input {combo.Bounds}, viewport {viewport}, preferred {combo.PreferredSize}.");
            }
            typeof(Control).GetMethod("OnMouseEnter", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(eye, [EventArgs.Empty]); Capture(host, $"fields-{dpi}-hover");
            password.SetError("Check this value."); Application.DoEvents(); Capture(host, $"fields-{dpi}-error");
            var withMessage = password.Height; password.SetError(""); password.PerformLayout(); Assert.True(password.Height < withMessage);
            Assert.True(combo.Left < 0); Assert.True(date.Box.Input.Left < 0);
            eye.PerformClick(); Assert.False(((TextBox)password.Box.Input).UseSystemPasswordChar);
            var calendarAction = PageLayoutTests.Descendants(date.Box).OfType<Button>().Single();
            Assert.Equal("Open calendar", calendarAction.AccessibleName);
            calendarAction.PerformClick(); Application.DoEvents();
            var popup = (ToolStripDropDown)typeof(FieldBox).GetField("_calendar", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(date.Box)!;
            Assert.True(popup.Visible); popup.Close();
        }
        finally { awareness.SetValue(null, previousAwareness); SetThreadDpiAwarenessContext(previousDpiContext); MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void AsyncModalFailureStaysOpenAndBusyGuardsDuplicateSaves() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var pending = new TaskCompletionSource<ServiceResult>(); var calls = 0;
            using var fields = UiFactory.Field(new TextBox(), "First name");
            using var dialog = new PatientDialog(fields, async () =>
            {
                calls++; var result = await pending.Task;
                if (!result.Success) { UiMessages.ShowError(result); return false; } return true;
            }, false);
            PageLayoutTests.ShowOffscreen(dialog); dialog.ConfirmButton.PerformClick(); dialog.ConfirmButton.PerformClick();
            Assert.Equal(1, calls); Assert.True(dialog.ConfirmButton.IsBusy); Assert.False(dialog.DismissButton.Enabled);
            pending.SetResult(ServiceResult.Fail("First name is required."));
            for (var i = 0; i < 1000 && dialog.ConfirmButton.IsBusy; i++) Application.DoEvents();
            Assert.False(dialog.ConfirmButton.IsBusy); Assert.True(dialog.Visible); Assert.Equal(DialogResult.None, dialog.DialogResult);
            Assert.True(dialog.Alert.Visible); Assert.Contains("First name", dialog.Alert.Text);
            pending = new TaskCompletionSource<ServiceResult>(); dialog.ConfirmButton.PerformClick(); pending.SetResult(ServiceResult.Ok());
            for (var i = 0; i < 1000 && dialog.Visible; i++) Application.DoEvents();
            Assert.Equal(DialogResult.OK, dialog.DialogResult);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Theory]
    [InlineData("patients")] [InlineData("appointments")]
    public void NativeGridsFastScrollSixHundredRowsWithoutPageScroll(string name) => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var actor = new User { Role = Roles.Admin };
            var patients = Enumerable.Range(1, 600).Select(id => new Patient { PatientId = id, FirstName = "Patient", LastName = id.ToString(), DateOfBirth = new(1998, 4, 12) }).ToArray();
            var appointments = Enumerable.Range(1, 600).Select(id => new Appointment { AppointmentId = id, PatientId = id, DentistId = 2, AppointmentDateTime = DateTime.Today.AddMinutes(id * 15) }).ToArray();
            var patientService = TestServices.Create<IPatientService>((nameof(IPatientService.GetAllPatientsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Patient>>.Ok(patients))));
            var dentistService = TestServices.Create<IDentistService>((nameof(IDentistService.GetAllDentistsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Dentist>>.Ok([new() { DentistId = 2, FirstName = "Miguel", LastName = "Reyes" }]))));
            var appointmentService = TestServices.Create<IAppointmentService>((nameof(IAppointmentService.GetAllAppointmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Ok(appointments))));
            using UserControl page = name == "patients" ? new ucPatientRecords(patientService, actor) : new ucAppointmentScheduler(appointmentService, patientService, dentistService, TestServices.Create<ITreatmentTypeService>(), actor);
            using var host = new Form { ClientSize = new(1100, 800) }; host.Controls.Add(page); page.Dock = DockStyle.Fill; PageLayoutTests.ShowOffscreen(host);
            var grid = PageLayoutTests.Descendants(page).OfType<DataGridView>().Single(); Assert.Equal(600, grid.Rows.Count);
            Assert.False(page.AutoScroll); Assert.DoesNotContain(PageLayoutTests.Descendants(page), control => control is ScrollableControl panel && panel.AutoScroll && control.Contains(grid));
            using var native = new DataGridView();
            var getStyle = typeof(Control).GetMethod("GetStyle", BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (var style in new[] { ControlStyles.Opaque, ControlStyles.UserPaint, ControlStyles.ResizeRedraw })
                Assert.Equal(getStyle.Invoke(native, [style]), getStyle.Invoke(grid, [style]));
            Assert.True((bool)typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(grid)!);
            for (var i = 0; i < 20; i++)
            {
                grid.FirstDisplayedScrollingRowIndex = (i * 29) % 550;
                SendMessage(grid.Handle, 0x115, (IntPtr)(4 | ((i * 29) << 16)), IntPtr.Zero); // WM_VSCROLL / thumb position
                SendMessage(grid.Handle, 0x20A, (IntPtr)(-1200 << 16), IntPtr.Zero); // fast wheel
                Application.DoEvents(); grid.Refresh();
            }
            Assert.All(grid.Rows.Cast<DataGridViewRow>(), row => Assert.Equal(grid.RowTemplate.Height, row.Height));
            Capture(host, name + "-fast-scroll");
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
}
