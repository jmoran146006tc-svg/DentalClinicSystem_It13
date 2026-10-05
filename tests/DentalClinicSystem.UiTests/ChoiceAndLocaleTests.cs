using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

public sealed class ChoiceAndLocaleTests
{
    [Theory]
    [InlineData("reason")] [InlineData("specialization")]
    public void OtherFieldIsRequiredAndSavesItsText(string kind) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); var actor = ClinicFixture.Actor();
        using UserControl page = kind == "reason"
            ? new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, services.TreatmentTypes, actor)
            : new ucDentistRecords(services.Dentists, actor);
        using var host = new Form { ClientSize = new(1280, 900) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        EnglishUi.AssertTree(page);
        var create = UiThread.Controls(page).OfType<AppButton>().Single(b => b.Text == (kind == "reason" ? "Schedule" : "New dentist"));
        AntdIntegrationTests.InspectDialog(host, create.PerformClick, dialog =>
        {
            var select = UiThread.Named<ClinicSelect>(dialog, kind == "reason" ? "cmbReason" : "cboSpecialization");
            select.SelectedItem = OtherChoice.Other; Application.DoEvents();
            var details = UiThread.Named<AntdUI.Input>(dialog, select.Name + "Other");
            Assert.True(details.Visible); Assert.Equal(kind == "reason" ? FieldLimits.Reason : FieldLimits.Specialization, details.MaxLength);
            var save = page.GetType().GetMethod(kind == "reason" ? "ScheduleAsync" : "SaveAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(save);
            Assert.False(Assert.IsType<Task<bool>>(save.Invoke(page, null)).GetAwaiter().GetResult());
            details.Text = "Custom clinic value";
            Assert.True(Assert.IsType<Task<bool>>(save.Invoke(page, null)).GetAwaiter().GetResult());
            var stub = (ServiceStub)(object)(kind == "reason" ? (object)services.Appointments : services.Dentists);
            var saved = stub.Calls[kind == "reason" ? nameof(IAppointmentService.ScheduleAppointmentAsync) : nameof(IDentistService.AddDentistAsync)][1];
            Assert.Equal("Custom clinic value", saved is Appointment appointment ? appointment.Reason : Assert.IsType<Dentist>(saved).Specialization);
            EnglishUi.AssertTree(dialog); UiThread.Capture(dialog, "fix-E-other-" + kind);
        });
    });

    [Fact]
    public void PickerTextIsProtectedAndDateMonthYearTimePopupsAreEnglish() => UiThread.Run(() =>
    {
        using var host = new Form { ClientSize = new(500, 200) }; UiThread.Show(host);
        foreach (var panel in Enum.GetValues<AntdUI.TDatePicker>())
        {
            using var picker = new ClinicDatePicker { Picker = panel, Value = new(2026, 10, 5), Format = DateTimePickerFormat.Custom, CustomFormat = "MMM d, yyyy HH:mm" };
            host.Controls.Add(picker); picker.SelectAll(); var before = picker.Text;
            var press = typeof(AntdUI.Input).GetMethod("IKeyPress", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(press); press.Invoke(picker, ['x']);
            picker.HandKeyBoard(Keys.Back); Assert.Equal(before, picker.Text);
            picker.ExpandDrop = true; Application.DoEvents(); var popup = picker.SubForm(); Assert.NotNull(popup);
            EnglishUi.AssertPopup(popup);
            EnglishUi.CapturePopup(popup, "fix-E-picker-" + panel);
            picker.ExpandDrop = false; host.Controls.Remove(picker);
        }
    });

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void EmptyAndPopulatedSelectPopupsAreEnglish(bool populated) => UiThread.Run(() =>
    {
        using var select = new ClinicSelect(); if (populated) select.Items.AddRange(["First option", "Second option"]);
        using var host = new Form { ClientSize = new(500, 200) }; host.Controls.Add(select); UiThread.Show(host);
        select.ExpandDrop = true; Application.DoEvents(); var popup = select.SubForm(); Assert.NotNull(popup);
        EnglishUi.AssertPopup(popup); Assert.Equal("No data", AntdUI.Localization.Get("NoData", "暂无数据"));
        EnglishUi.CapturePopup(popup, "fix-E-select-" + populated);
        select.ExpandDrop = false;
    });

    [Fact]
    public void ExistingUnlistedSpecializationOpensAsOtherWithoutLosingText() => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services();
        ((ServiceStub)(object)services.Dentists).Results[nameof(IDentistService.GetAllDentistsAsync)] =
            Task.FromResult(ServiceResult<IReadOnlyList<Dentist>>.Ok([new() { DentistId = 2, FirstName = "Miguel", LastName = "Reyes", Specialization = "Unlisted specialty" }]));
        using var page = new ucDentistRecords(services.Dentists, ClinicFixture.Actor());
        using var host = new Form { ClientSize = new(1280, 900) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        var table = UiThread.Controls(page).OfType<ClinicTable>().Single(); table.SetSelected(table.Records[0], true);
        var edit = UiThread.Controls(page).OfType<AppButton>().Single(b => b.Text == "Edit dentist");
        AntdIntegrationTests.InspectDialog(host, edit.PerformClick, dialog =>
        {
            Assert.Equal(OtherChoice.Other, UiThread.Named<ClinicSelect>(dialog, "cboSpecialization").Text);
            var details = UiThread.Named<AntdUI.Input>(dialog, "cboSpecializationOther");
            Assert.True(details.Visible); Assert.Equal("Unlisted specialty", details.Text);
        });
    });

    [Theory]
    [InlineData(Semantic.Info)] [InlineData(Semantic.Success)] [InlineData(Semantic.Warning)] [InlineData(Semantic.Danger)] [InlineData(Semantic.Neutral)]
    public void AlertUsesExactSemanticBackground(Semantic semantic) => UiThread.Run(() =>
    {
        using var alert = new InlineAlert { Width = 600 }; alert.ShowMessage("Sample feedback", semantic);
        using var host = new Form { ClientSize = new(600, 100) }; host.Controls.Add(alert); UiThread.Show(host);
        using var bitmap = new Bitmap(alert.Width, alert.Height); alert.DrawToBitmap(bitmap, alert.ClientRectangle);
        Assert.Equal(Theme.SemanticStyle(semantic).Background.ToArgb(), bitmap.GetPixel(alert.Width / 2, Space.Sm).ToArgb());
        EnglishUi.AssertTree(host); UiThread.Capture(host, "fix-E-alert-" + semantic);
    });
}
