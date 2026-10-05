using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public class AntdIntegrationTests
{
    [Theory]
    [InlineData("patients", 1440)]
    [InlineData("patients", 1100)]
    [InlineData("dentists", 1440)]
    [InlineData("dentists", 1100)]
    [InlineData("users", 1440)]
    [InlineData("users", 1100)]
    [InlineData("appointments", 1440)]
    [InlineData("appointments", 1100)]
    [InlineData("treatments", 1440)]
    [InlineData("treatments", 1100)]
    public void RecordsSearchSelectionAndDialogsKeepTheirBehavior(string name, int width) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); var actor = ClinicFixture.Actor();
        using var page = Page(name, services, actor);
        using var host = new Form { ClientSize = new(width, 900) };
        page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        var grid = UiThread.Controls(page).OfType<ClinicTable>().Single();
        Assert.Single(grid.Records);
        Assert.DoesNotContain(grid.Columns, c => c.Key is "PasswordHash" or "Record" or "PatientId" or "DentistId" or "UserId" or "TreatmentId" or "AppointmentId");
        Assert.True(grid.Width > host.ClientSize.Width / 2); Assert.True(grid.Height > 300);
        EnglishUi.AssertTree(page);
        Assert.Null(grid.SelectedRecord);
        UiThread.Capture(host, name + "-" + width);
        grid.SetSelected(grid.Records[0], true); Application.DoEvents();
        Assert.Same(grid.Records[0], grid.SelectedRecord);
        if (name == "patients") Assert.Equal("Ana", UiThread.Named<AntdUI.Input>(page, "txtFirstName").Text);
        if (name == "dentists") Assert.Equal("Miguel", UiThread.Named<AntdUI.Input>(page, "txtFirstName").Text);
        if (name == "users") Assert.Equal("admin", UiThread.Named<AntdUI.Input>(page, "txtUsername").Text);
        if (name == "treatments") Assert.Equal("1250.50", UiThread.Named<AntdUI.Input>(page, "txtCost").Text);
        var search = UiThread.Controls(page).OfType<AntdUI.Input>().Single(i => i.PlaceholderText?.StartsWith("Search ") == true);
        search.Text = "no matching record"; Application.DoEvents();
        Assert.Empty(grid.Records); Assert.Null(grid.SelectedRecord);
        search.Clear(); Application.DoEvents(); Assert.Single(grid.Records);
        using (UiMessages.UseOwner(page)) UiMessages.ShowError(ServiceResult.Fail("Please check the entered details."));
        Assert.True(UiThread.Controls(page).OfType<InlineAlert>().Single().Visible);
        var create = UiThread.Controls(page).OfType<AppButton>().Single(b => b.Text == (name == "appointments" ? "Schedule" : "New " + name.TrimEnd('s')));
        InspectDialog(host, create.PerformClick, dialog =>
        {
            EnglishUi.AssertTree(dialog);
            Assert.NotEmpty(UiThread.Controls(dialog).OfType<AntdUI.Input>());
            Assert.DoesNotContain(UiThread.Controls(dialog), c => c is System.Windows.Forms.TextBox or System.Windows.Forms.ComboBox or System.Windows.Forms.DateTimePicker);
            Assert.NotEmpty(UiThread.Controls(dialog).OfType<AntdUI.Button>());
            if (name == "patients") Assert.Equal("", UiThread.Named<AntdUI.Input>(dialog, "txtFirstName").Text);
            if (name == "appointments") Assert.Equal("Ana Santos", UiThread.Named<ClinicSelect>(dialog, "cboPatient").Text);
            if (name == "users") Assert.False(UiThread.Named<ClinicSelect>(dialog, "cboDentist").Enabled);
            if (name == "treatments") Assert.False(UiThread.Named<ClinicDatePicker>(dialog, "dtpDatePerformed").Enabled);
            UiThread.Capture(dialog, name + "-dialog-" + width);
        });
    });

    [Theory]
    [InlineData("patients")]
    [InlineData("dentists")]
    [InlineData("users")]
    [InlineData("appointments")]
    [InlineData("treatments")]
    public void EmptyRecordPagesRenderWithoutCreatingFakeRows(string name) => UiThread.Run(() =>
    {
        using var page = Page(name, ClinicFixture.Services(false), ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(1100, 800) };
        page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        var table = UiThread.Controls(page).OfType<ClinicTable>().Single();
        Assert.Empty(table.Records); Assert.Null(table.SelectedRecord); Assert.False(string.IsNullOrWhiteSpace(table.EmptyText));
        UiThread.Capture(host, name + "-empty");
    });

    [Fact]
    public void DropdownPreservesOriginalModelAndIdAfterRebind() => UiThread.Run(() =>
    {
        using var choice = new ClinicSelect { DisplayMember = nameof(DisplayOption.Display), ValueMember = nameof(DisplayOption.Id) };
        choice.DataSource = new[] { new DisplayOption(1, "Ana"), new DisplayOption(2, "Miguel") };
        choice.SelectedValue = 2;
        Assert.Equal("Miguel", choice.Text); Assert.Equal(2, choice.SelectedValue);
        Assert.Equal(2, Assert.IsType<DisplayOption>(choice.SelectedItem).Id);
        var notifications = 0; choice.SelectedIndexChanged += (_, _) => notifications++;
        choice.DataSource = new[] { new DisplayOption(2, "Miguel (busy)", true), new DisplayOption(1, "Ana") };
        Assert.Equal(2, choice.SelectedValue); Assert.Equal("Miguel (busy)", choice.Text);
        Assert.True(Assert.IsType<DisplayOption>(choice.SelectedItem).Busy);
        Assert.Equal(1, notifications);
        choice.SelectedIndex = -1; Assert.Null(choice.SelectedValue);
        choice.Items.Clear(); Assert.Null(choice.SelectedItem);
    });

    [Fact]
    public void DropdownRefreshesVisibleLabelWhenMetadataIsSetAfterDataSource() => UiThread.Run(() =>
    {
        using var choice = new ClinicSelect();
        choice.DataSource = new[] { new DisplayOption(3, "Ana Santos") };
        choice.DisplayMember = nameof(DisplayOption.Display);
        choice.ValueMember = nameof(DisplayOption.Id);
        Assert.Equal("Ana Santos", choice.Text); Assert.Equal(3, choice.SelectedValue);
        choice.DisplayMember = nameof(DisplayOption.Id);
        Assert.Equal("3", choice.Text);
    });

    [Fact]
    public void EveryNativeIconProducesVisibleForegroundPixels() => UiThread.Run(() =>
    {
        foreach (var icon in Enum.GetValues<IconKind>())
        {
            using var bitmap = AntdUI.SvgExtend.SvgToBmp(AntdTheme.Svg(icon), 24, 24, Color.Teal);
            Assert.NotNull(bitmap);
            Assert.True(Enumerable.Range(0, bitmap.Width).Any(x => Enumerable.Range(0, bitmap.Height).Any(y => bitmap.GetPixel(x, y).A > 0)), icon + " is invisible.");
        }
    });

    [Fact]
    public void DropdownIsPickOnlyAndCustomValuesUseOther() => UiThread.Run(() =>
    {
        using var choice = new ClinicSelect { DropDownStyle = ComboBoxStyle.DropDown };
        choice.Items.AddRange(["General dentistry", "Orthodontics", OtherChoice.Other]);
        var custom = new OtherChoice(choice, "Specify specialization", FieldLimits.Specialization);
        using var host = new Form(); host.Controls.Add(choice); host.Controls.Add(custom.Details); UiThread.Show(host);
        custom.SetValue("Unlisted specialty"); Assert.True(choice.List); Assert.Equal(OtherChoice.Other, choice.Text);
        Assert.True(custom.Details.Visible); Assert.Equal("Unlisted specialty", custom.Value); Assert.True(custom.Validate());
        custom.Details.Box.Input.Text = ""; Assert.False(custom.Validate());
        custom.SetValue("Orthodontics"); Assert.False(custom.Details.Visible); Assert.Equal("Orthodontics", custom.Value);
        using var required = new ClinicSelect(); required.Items.AddRange([15, 30, 45]); required.SelectedItem = 30;
        Assert.Equal(30, required.SelectedItem); Assert.Equal("30", required.Text); Assert.True(required.List);
    });

    [Fact]
    public void RequiredPickerClampsDatesAndRetainsTimeAndReadOnlyLocks() => UiThread.Run(() =>
    {
        var createdAt = DateTime.Now;
        using var initial = new ClinicDatePicker();
        Assert.InRange(initial.Value, createdAt, DateTime.Now);
        using var picker = new ClinicDatePicker { MinDate = new(2026, 1, 1), MaxDate = new(2026, 12, 31), Format = DateTimePickerFormat.Custom, CustomFormat = DisplayFormat.DateTimePattern };
        Assert.Contains("H", ((AntdUI.DatePicker)picker).Format);
        Assert.Contains("m", ((AntdUI.DatePicker)picker).Format);
        picker.Value = new(2026, 6, 1, 14, 30, 0); Assert.Equal(14, picker.Value.Hour);
        picker.Value = new(2027, 1, 1); Assert.Equal(picker.MaxDate, picker.Value);
        ((AntdUI.DatePicker)picker).Value = null; Assert.Equal(picker.MaxDate, picker.Value);
        picker.Enabled = false;
        using var box = new FieldBox(picker, FieldKind.Date); using var form = new Form(); form.Controls.Add(box);
        form.Enabled = false; form.Enabled = true; Assert.False(picker.Enabled);
    });

    [Fact]
    public void PasswordVisibilityAndPhonePasteUseAntdInputBehavior() => UiThread.Run(() =>
    {
        using var password = new AntdUI.Input { Text = "sample" }; using var field = new FieldBox(password, FieldKind.Password);
        Assert.True(password.UseSystemPasswordChar); field.TogglePassword(); Assert.False(password.UseSystemPasswordChar);
        field.TogglePassword(); Assert.True(password.UseSystemPasswordChar);
        password.Enabled = false; field.TogglePassword(); Assert.True(password.UseSystemPasswordChar);
        using var phone = new VerifiedInput(); phone.VerifyChar += InputRules.PhoneVerifyChar;
        phone.InsertVerified("09x17+ 123"); Assert.Equal("0917+ 123", phone.Text);
        phone.MaxLength = 3; phone.Clear(); phone.EnterText("123456", true, false); Assert.Equal("123", phone.Text);
    });

    // AntdUI's keyboard and clipboard paste paths use the protected Verify hook
    // before EnterText. Exercise that path without changing the user's clipboard.
    private sealed class VerifiedInput : AntdUI.Input
    {
        public void InsertVerified(string text)
        {
            var verified = new System.Text.StringBuilder();
            foreach (var character in text) if (Verify(character, out var replacement)) verified.Append(replacement ?? character.ToString());
            EnterText(verified.ToString(), true, false);
        }
    }

    private sealed record TableRow(int RowId, string Name, string PasswordHash);
    [Fact]
    public void TableSelectionReturnsOriginalRecordAfterSortingAndClearsAfterRebind() => UiThread.Run(() =>
    {
        using var grid = new ClinicTable { Dock = DockStyle.Fill }; using var host = new Form(); host.Controls.Add(grid);
        var first = new TableRow(1, "Zoe", "secret"); var second = new TableRow(2, "Ana", "secret");
        GridHelper.Bind(grid, new[] { first, second }, r => r.RowId, "No people", "RowId"); UiThread.Show(host);
        Assert.DoesNotContain(grid.Columns, c => c.Key == "PasswordHash");
        grid.SetSortList([second, first]); grid.SetSelected(second, true);
        Assert.Same(second, grid.SelectedRecord);
        GridTheme.SelectAndFlash(grid, 1); Assert.Same(first, grid.SelectedRecord);
        GridHelper.Bind(grid, Array.Empty<TableRow>()); Assert.Null(grid.SelectedRecord); Assert.Empty(grid.Records);
    });

    [Theory]
    [InlineData(Roles.Admin, 7)]
    [InlineData(Roles.Receptionist, 3)]
    [InlineData(Roles.Dentist, 3)]
    public void RoleNavigationUsesAntdButtonsWithoutChangingPermissions(string role, int pages) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); using var login = new frmLogin(services);
        using var dashboard = new frmDashboard(ClinicFixture.Actor(role), login, services);
        typeof(frmDashboard).GetField("_loggingOut", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(dashboard, true);
        dashboard.WindowState = FormWindowState.Normal; dashboard.ClientSize = new(1440, 900); UiThread.Show(dashboard);
        var buttons = UiThread.Controls(dashboard).OfType<AntdUI.Button>().Where(b => b.Visible && b.Name.StartsWith("btn") && b.Name != "btnLogout");
        Assert.Equal(pages, buttons.Count()); UiThread.Capture(dashboard, "dashboard-" + role);
        UiThread.Show(login);
        Assert.IsType<AntdUI.Input>(UiThread.Named<AntdUI.Input>(login, "txtPassword"));
        Assert.True(UiThread.Named<AntdUI.Input>(login, "txtPassword").UseSystemPasswordChar);
        UiThread.Capture(login, "login");
    });

    [Fact]
    public void ReportsPopulateEverySectionWithoutSelectingTabs() => UiThread.Run(() =>
    {
        var reports = ServiceStub.For<IReportService>(
            (nameof(IReportService.GetAppointmentStatusCountsAsync), Task.FromResult(ServiceResult<IReadOnlyList<AppointmentStatusCount>>.Ok([new("Scheduled", 2)]))),
            (nameof(IReportService.GetRevenueByDayAsync), Task.FromResult(ServiceResult<IReadOnlyList<RevenueDay>>.Ok([new(DateTime.Today, 1250m)]))),
            (nameof(IReportService.GetTopTreatmentTypesAsync), Task.FromResult(ServiceResult<IReadOnlyList<TopTreatmentType>>.Ok([new("Cleaning", 2, 1250m)]))),
            (nameof(IReportService.GetDentistWorkloadAsync), Task.FromResult(ServiceResult<IReadOnlyList<DentistWorkload>>.Ok([new("Miguel Reyes", 2, 1, 1250m)]))));
        using var page = new ucReports(reports, ClinicFixture.Actor()) { Dock = DockStyle.Fill };
        using var host = new Form { ClientSize = new(1200, 800) }; host.Controls.Add(page); UiThread.Show(host);
        Assert.Empty(UiThread.Controls(page).OfType<AntdUI.Tabs>());
        Assert.NotNull(page.Snapshot); Assert.Equal(5, page.Snapshot.Status.Count);
        foreach (var grid in UiThread.Controls(page).OfType<ClinicTable>()) Assert.Single(grid.Records);
        Assert.Equal("2", UiThread.Named<Label>(page, "reportCountScheduled").Text);
        Assert.Equal("0", UiThread.Named<Label>(page, "reportCountNoShow").Text);
        Assert.Equal(1250m, page.Snapshot.Days.Sum(row => row.Billed));
        UiThread.Capture(host, "reports");
    });

    [Fact]
    public void FailedSaveStaysInDialogAndReleasesBusyState() => UiThread.Run(() =>
    {
        using var fields = new Panel(); fields.Controls.Add(new FieldBox(new AntdUI.Input()));
        using var dialog = new RecordDialog("Validation", fields, () => { UiMessages.ShowError(ServiceResult.Fail("Please enter a valid value.")); return Task.FromResult(false); });
        UiThread.Show(dialog); dialog.ConfirmButton.PerformClick(); Application.DoEvents();
        Assert.True(dialog.Visible); Assert.True(dialog.Alert.Visible); Assert.False(dialog.ConfirmButton.IsBusy); Assert.True(dialog.DismissButton.Enabled);
        Assert.NotEqual(DialogResult.OK, dialog.DialogResult);
    });

    [Fact]
    public void NativeLoadingKeepsSaveSingleAndRestoresButtonsAfterCompletion() => UiThread.Run(() =>
    {
        var completion = new TaskCompletionSource<bool>(); var saves = 0;
        using var fields = new Panel();
        using var dialog = new RecordDialog("Save", fields, () => { saves++; return completion.Task; });
        UiThread.Show(dialog); var width = dialog.ConfirmButton.Width;
        dialog.ConfirmButton.PerformClick(); dialog.ConfirmButton.PerformClick();
        Assert.Equal(1, saves); Assert.True(dialog.ConfirmButton.Loading); Assert.False(dialog.DismissButton.Enabled);
        Assert.Equal(width, dialog.ConfirmButton.Width);
        completion.SetResult(false); Application.DoEvents();
        Assert.False(dialog.ConfirmButton.Loading); Assert.Equal("Save", dialog.ConfirmButton.Text);
        Assert.True(dialog.DismissButton.Enabled); Assert.True(dialog.Visible);
    });

    private static UserControl Page(string name, AppServices services, User actor) => name switch
    {
        "patients" => new ucPatientRecords(services.Patients, actor), "dentists" => new ucDentistRecords(services.Dentists, actor),
        "users" => new ucUserManagement(services.Users, services.Dentists, actor), "appointments" => new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, services.TreatmentTypes, actor),
        _ => new ucTreatmentRecords(services.Treatments, services.Appointments, services.TreatmentTypes, actor)
    };
    internal static void InspectDialog(Form owner, Action open, Action<Form> inspect)
    {
        Exception? error = null; var inspected = false; var attempts = 0;
        using var timer = new System.Windows.Forms.Timer { Interval = 25 };
        timer.Tick += (_, _) =>
        {
            var dialog = owner.OwnedForms.FirstOrDefault(f => f.Visible);
            if (dialog is null) { if (++attempts > 80) timer.Stop(); return; }
            timer.Stop();
            try { inspect(dialog); inspected = true; }
            catch (Exception failure) { error = failure; }
            finally { dialog.DialogResult = DialogResult.Cancel; dialog.Close(); }
        };
        timer.Start(); open(); Assert.True(inspected, error?.ToString() ?? "Dialog did not open.");
    }
}
