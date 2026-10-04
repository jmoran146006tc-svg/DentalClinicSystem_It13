using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Tests;

public class TreatmentTests
{
    [Theory]
    [InlineData("1250.50", true)]
    [InlineData(" 0.00 ", true)]
    [InlineData("99999999.99", true)]
    [InlineData("100000000", false)]
    [InlineData("-1", false)]
    [InlineData("1,23", false)]
    [InlineData("NaN", false)]
    [InlineData("", false)]
    public void CostInputUsesInvariantDecimalAndServiceBounds(string text, bool valid) => Assert.Equal(valid, InputRules.TryCost(text, out _));
    [Fact]
    public void SelectionRestoresActualCostAndSavesAnUpdate() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var a = new Appointment { AppointmentId = 1, DentistId = 2, PatientId = 3, AppointmentDateTime = DateTime.Today };
            var appointments = TestServices.Create<IAppointmentService>((nameof(IAppointmentService.GetAllAppointmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Ok([a]))),
                (nameof(IAppointmentService.GetDetailsAsync), Task.FromResult(ServiceResult<AppointmentDetails>.Ok(new(a, new Patient { FirstName = "Ana", LastName = "Santos" }, new Dentist())))));
            var treatments = TestServices.Create<ITreatmentService>((nameof(ITreatmentService.GetAllTreatmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Treatment>>.Ok([new() { TreatmentId = 7, AppointmentId = 1, TreatmentTypeId = 8, Cost = 300, DatePerformed = DateTime.Today }]))));
            var types = TestServices.Create<ITreatmentTypeService>((nameof(ITreatmentTypeService.GetAllTreatmentTypesAsync), Task.FromResult(ServiceResult<IReadOnlyList<TreatmentType>>.Ok([new() { TreatmentTypeId = 8, Name = "Cleaning", DefaultCost = 200 }]))));
            using var page = new ucTreatmentRecords(treatments, appointments, types, new User { Role = Roles.Dentist, DentistId = 2 });
            using var host = new Form { ClientSize = new(1440, 1000) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); PageLayoutTests.ShowOffscreen(host);
            var grid = PageLayoutTests.Named<DataGridView>(page, "dgvTreatments"); Assert.Single(grid.Rows.Cast<DataGridViewRow>());
            grid.CurrentCell = grid.Rows[0].Cells["Patient"]; grid.Rows[0].Selected = true;
            var edit = PageLayoutTests.Descendants(page).OfType<AppButton>().Single(button => button.Text == "Edit treatment");
            PageLayoutTests.InspectNextDialog(host, dialog =>
            {
                var performed = PageLayoutTests.Named<DateTimePicker>(dialog, "dtpDatePerformed");
                Assert.False(performed.Enabled); Assert.Equal(a.AppointmentDateTime.Date, performed.Value.Date);
                Assert.Equal(2, PageLayoutTests.Named<TextBox>(dialog, "txtToothNumber").MaxLength);
                Assert.Equal("300.00", PageLayoutTests.Named<TextBox>(dialog, "txtCost").Text);
                PageLayoutTests.Named<TextBox>(dialog, "txtCost").Text = "325.50"; dialog.ConfirmButton.PerformClick();
            }, edit.PerformClick, save: true);
            var proxy = (TestServices)(object)treatments; Assert.Equal(1, proxy.Calls.GetValueOrDefault(nameof(ITreatmentService.UpdateTreatmentAsync)));
            Assert.Equal(0, proxy.Calls.GetValueOrDefault(nameof(ITreatmentService.AddTreatmentAsync)));
            var saved = Assert.IsType<Treatment>(proxy.Arguments[nameof(ITreatmentService.UpdateTreatmentAsync)][1]); Assert.Equal(7, saved.TreatmentId); Assert.Equal(325.50m, saved.Cost);
            foreach (var field in PageLayoutTests.Descendants(page).OfType<FormField>()) Assert.True(field.Box.Input.Parent!.Height >= field.Box.Input.Height);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
}
