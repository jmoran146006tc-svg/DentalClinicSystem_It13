using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Tests;

// These previews use deterministic test data and never connect to MySQL.
public class VisualPreviewTests
{
    private static AppServices SampleServices()
    {
        var patient = new Patient { PatientId = 3, FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", Email = "ana@example.test", Address = "Davao City", DateOfBirth = new(1998, 4, 12) };
        var dentist = new Dentist { DentistId = 2, FirstName = "Miguel", LastName = "Reyes", Specialization = "General dentistry", LicenseNumber = "PRC-12345", ContactNumber = "09181234567" };
        var appointment = new Appointment { AppointmentId = 4, PatientId = 3, DentistId = 2, AppointmentDateTime = DateTime.Today.AddHours(10), Reason = "Consultation & Check up", Notes = "Sample appointment for UI review" };
        return TestServices.Empty with
        {
            Patients = TestServices.Create<IPatientService>((nameof(IPatientService.GetAllPatientsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Patient>>.Ok([patient]))), (nameof(IPatientService.GetAllIncludingInactiveAsync), Task.FromResult(ServiceResult<IReadOnlyList<Patient>>.Ok([patient])))),
            Dentists = TestServices.Create<IDentistService>((nameof(IDentistService.GetAllDentistsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Dentist>>.Ok([dentist])))),
            Users = TestServices.Create<IUserService>((nameof(IUserService.GetAllUsersAsync), Task.FromResult(ServiceResult<IReadOnlyList<User>>.Ok([new() { UserId = 1, Username = "admin", Role = Roles.Admin, PasswordHash = "never display this" }, new() { UserId = 2, Username = "drreyes", Role = Roles.Dentist, DentistId = 2 }])))),
            Appointments = TestServices.Create<IAppointmentService>((nameof(IAppointmentService.GetAllAppointmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Ok([appointment]))), (nameof(IAppointmentService.GetDetailsAsync), Task.FromResult(ServiceResult<AppointmentDetails>.Ok(new(appointment, patient, dentist))))),
            TreatmentTypes = TestServices.Create<ITreatmentTypeService>((nameof(ITreatmentTypeService.GetAllTreatmentTypesAsync), Task.FromResult(ServiceResult<IReadOnlyList<TreatmentType>>.Ok([new() { TreatmentTypeId = 8, Name = "Cleaning", DefaultCost = 1250 }])))),
            Treatments = TestServices.Create<ITreatmentService>((nameof(ITreatmentService.GetAllTreatmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Treatment>>.Ok([new() { TreatmentId = 7, AppointmentId = 4, TreatmentTypeId = 8, Cost = 1250.50m, ToothNumber = "11", Notes = "Sample clinical record", DatePerformed = DateTime.Today }]))))
        };
    }
    private static void Capture(Form form, string name)
    {
        var directory = Environment.GetEnvironmentVariable("DENTAL_UI_SNAPSHOTS"); if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory); using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(directory, name + ".png"));
    }
    [Theory]
    [InlineData("patients")]
    [InlineData("dentists")]
    [InlineData("users")]
    [InlineData("appointments")]
    [InlineData("treatments")]
    public void RecordPagesRenderEmptyFilledSelectedAndErrorStates(string name) => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var actor = new User { UserId = 1, Username = "admin", Role = Roles.Admin };
            UserControl Page(AppServices services) => name switch
            {
                "patients" => new ucPatientRecords(services.Patients, actor), "dentists" => new ucDentistRecords(services.Dentists, actor),
                "users" => new ucUserManagement(services.Users, services.Dentists, actor), "appointments" => new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, actor),
                _ => new ucTreatmentRecords(services.Treatments, services.Appointments, services.TreatmentTypes, actor)
            };
            using var host = new Form { ClientSize = new(1440, 1000) };
            var empty = Page(TestServices.Empty); empty.Dock = DockStyle.Fill; host.Controls.Add(empty); PageLayoutTests.ShowOffscreen(host);
            Capture(host, name + "-empty"); empty.Dispose();
            var filled = Page(SampleServices()); filled.Dock = DockStyle.Fill; host.Controls.Add(filled); filled.CreateControl(); Application.DoEvents();
            var grid = PageLayoutTests.Descendants(filled).OfType<DataGridView>().Single(); Assert.NotEmpty(grid.Rows.Cast<DataGridViewRow>());
            Assert.True(grid.Width > host.ClientSize.Width / 2); Assert.True(grid.Height > host.ClientSize.Height / 3);
            Capture(host, name + "-filled");
            grid.CurrentCell = grid.Rows[0].Cells.Cast<DataGridViewCell>().First(c => c.Visible); grid.Rows[0].Selected = true; Application.DoEvents();
            Capture(host, name + "-selected");
            using (UiMessages.UseOwner(filled)) UiMessages.ShowError(ServiceResult.Fail("Preview: Check the entered details and try again."));
            Application.DoEvents(); Capture(host, name + "-error");
            Assert.True(PageLayoutTests.Descendants(filled).OfType<InlineAlert>().Single().Visible);
            Assert.DoesNotContain(grid.Columns.Cast<DataGridViewColumn>(), c => c.Visible && c.Name is "PasswordHash" or "Record");
            var create = PageLayoutTests.Descendants(filled).OfType<AppButton>().Single(button => button.Text == (name == "appointments" ? "Schedule" : "New " + name.TrimEnd('s')));
            PageLayoutTests.InspectNextDialog(host, dialog => Capture(dialog, name + "-dialog"), create.PerformClick);
            host.ClientSize = new(1100, 900); Application.DoEvents(); Capture(host, name + "-narrow");
            foreach (var toolbar in PageLayoutTests.Descendants(filled).OfType<FlowLayoutPanel>().Where(panel => panel.Controls.OfType<FormField>().Any()))
            {
                Assert.All(toolbar.Controls.Cast<Control>().Where(control => control.Visible), control => Assert.True(control.Right <= toolbar.ClientSize.Width));
                var fields = toolbar.Controls.OfType<FormField>().ToArray();
                Assert.All(fields, field => Assert.Equal(fields[0].Top, field.Top));
            }
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Theory]
    [InlineData(Roles.Admin, 7)]
    [InlineData(Roles.Receptionist, 3)]
    [InlineData(Roles.Dentist, 3)]
    public void SidebarRolesRenderWithoutPermissionGaps(string role, int visiblePages) => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var services = SampleServices(); using var login = new frmLogin(services);
            using var dashboard = new frmDashboard(new User { Username = "review", Role = role, DentistId = 2 }, login, services);
            typeof(frmDashboard).GetField("_loggingOut", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(dashboard, true);
            dashboard.WindowState = FormWindowState.Normal; dashboard.ClientSize = new(1440, 1000); PageLayoutTests.ShowOffscreen(dashboard);
            var navigation = PageLayoutTests.Descendants(dashboard).OfType<Button>().Where(b => b.Visible && b.Name != "btnLogout").ToArray(); Assert.Equal(visiblePages, navigation.Length);
            var nav = navigation[0].Parent!;
            var brand = nav.Parent!.Controls.Cast<Control>().Single(control => control.Dock == DockStyle.Top);
            var account = nav.Parent.Controls.Cast<Control>().Single(control => control.Dock == DockStyle.Bottom);
            Assert.True(nav.Top >= brand.Bottom); Assert.True(nav.Bottom <= account.Top);
            Assert.True(PageLayoutTests.Named<Button>(dashboard, "btnLogout").Visible);
            Assert.True(PageLayoutTests.Named<Label>(dashboard, "_lblClock").Parent!.Height > 0);
            Assert.True(PageLayoutTests.Named<NavIndicator>(dashboard, "navIndicator").Visible);
            Capture(dashboard, "sidebar-" + role.ToLowerInvariant());
            PageLayoutTests.ShowOffscreen(login);
            var loginBrand = PageLayoutTests.Descendants(login).OfType<LoginBrand>().Single();
            Assert.InRange(Math.Abs((double)loginBrand.Width / loginBrand.Height - (double)login.BackgroundImage!.Width / login.BackgroundImage.Height), 0, .005);
            Capture(login, "login");
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void ReportsBindAllFourGridsIncludingUnselectedTabs() => PresentationTests.Sta(() =>
    {
        var reports = TestServices.Create<IReportService>(
            (nameof(IReportService.GetAppointmentStatusCountsAsync), Task.FromResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok([new("Scheduled", 2)]))),
            (nameof(IReportService.GetRevenueByDayAsync), Task.FromResult(ServiceResult<IReadOnlyList<RevenueDay>>.Ok([new(DateTime.Today, 1250m)]))),
            (nameof(IReportService.GetTopTreatmentTypesAsync), Task.FromResult(ServiceResult<IReadOnlyList<TopTreatmentType>>.Ok([new("Cleaning", 2, 1250m)]))),
            (nameof(IReportService.GetDentistWorkloadAsync), Task.FromResult(ServiceResult<IReadOnlyList<DentistWorkload>>.Ok([new("Miguel Reyes", 2, 1, 1250m)]))));
        using var host = new Form { ClientSize = new(1200, 800) };
        using var page = new ucReports(reports, new User { Role = Roles.Admin }) { Dock = DockStyle.Fill };
        host.Controls.Add(page); PageLayoutTests.ShowOffscreen(host);
        var tabs = PageLayoutTests.Descendants(page).OfType<TabControl>().Single();
        Assert.Equal(4, tabs.TabCount);
        // All sources are assigned while three tabs still have no grid handles.
        Assert.All(PageLayoutTests.Descendants(page).OfType<DataGridView>(), grid => Assert.NotNull(grid.DataSource));
        foreach (TabPage tab in tabs.TabPages)
        {
            tabs.SelectedTab = tab; Application.DoEvents();
            var grid = tab.Controls.OfType<DataGridView>().Single();
            Assert.Single(grid.Rows.Cast<DataGridViewRow>());
            Assert.All(grid.Columns.Cast<DataGridViewColumn>(), column => Assert.True(column.MinimumWidth >= 80));
        }
        Capture(host, "reports");
    });
#if DEBUG
    [Fact]
    public void GuideRendersRealSampleRowsAndUnclippedNativeInputs() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            using var guide = new frmStyleGuide { ClientSize = new(1600, 1000) }; PageLayoutTests.ShowOffscreen(guide);
            Capture(guide, "guide-top");
            var grids = PageLayoutTests.Descendants(guide).OfType<DataGridView>().ToArray(); Assert.Contains(grids, grid => grid.Rows.Count == 4);
            foreach (var field in PageLayoutTests.Descendants(guide).OfType<FormField>()) Assert.True(field.Box.Input.Parent!.Height >= field.Box.Input.Height - Metrics.Scale(field.Box, 8), $"{field.Box.Input.AccessibleName}: viewport {field.Box.Input.Parent!.Height}, input {field.Box.Input.Height}, DPI {field.Box.DeviceDpi}");
            var kpi = PageLayoutTests.Descendants(guide).OfType<KpiCard>().Single();
            var tile = kpi.Content.Controls.OfType<IconTile>().Single(); Assert.True(kpi.Content.ClientRectangle.Contains(tile.Bounds)); Assert.Equal(tile.Width, tile.Height);
            var scroll = guide.Controls.OfType<FlowLayoutPanel>().Single(); var last = scroll.Controls[scroll.Controls.Count - 1]; scroll.ScrollControlIntoView(last); Application.DoEvents(); Capture(guide, "guide-motion");
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
#endif
}
