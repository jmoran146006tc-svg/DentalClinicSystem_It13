using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Models;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Tests;

public class PageLayoutTests
{
    internal static IEnumerable<Control> Descendants(Control control) => control.Controls.Cast<Control>().SelectMany(child => new[] { child }.Concat(Descendants(child)));
    internal static T Named<T>(Control page, string name) where T : Control => Descendants(page).OfType<T>().Single(control => control.Name == name);
    internal static void ShowOffscreen(Form form)
    {
        form.StartPosition = FormStartPosition.Manual; form.Location = new(-20000, -20000); form.ShowInTaskbar = false; form.Show(); form.PerformLayout(); Application.DoEvents();
    }
    [Fact]
    public void LoginReusesInputsAndAcceptsEnter() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            using var login = new frmLogin(TestServices.Empty); ShowOffscreen(login);
            Assert.Same(Named<Button>(login, "btnLogin"), login.AcceptButton);
            Assert.True(Named<TextBox>(login, "txtUsername").Visible); Assert.True(Named<TextBox>(login, "txtPassword").UseSystemPasswordChar);
            Assert.False(login.MaximizeBox); Assert.Equal(FormBorderStyle.FixedSingle, login.FormBorderStyle);
            foreach (var input in new[] { Named<TextBox>(login, "txtUsername"), Named<TextBox>(login, "txtPassword") })
                Assert.True(input.Parent!.Height >= input.Height);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void PatientFormReusesDesignerInputsAndSplitStacksAtNarrowWidths() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var services = TestServices.Empty;
            using var page = new ucPatientRecords(services.Patients, new User { Role = Roles.Admin });
            using var host = new Form { ClientSize = new(1440, 1000) }; host.Controls.Add(page); page.Dock = DockStyle.Fill; ShowOffscreen(host);
            Assert.DoesNotContain(Descendants(page), control => control.Name == "btnDelete");
            Assert.True(Named<TextBox>(page, "txtFirstName").Visible);
            var split = Descendants(page).OfType<ResponsiveSplit>().Single(); Assert.Equal(2, split.ColumnCount);
            host.ClientSize = new(1000, 900); host.PerformLayout(); Assert.Equal(1, split.ColumnCount); Assert.Equal(2, split.RowCount);
            var grid = Named<DataGridView>(page, "dgvPatients"); Assert.True(grid.ReadOnly); Assert.False(grid.AllowUserToAddRows);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void UserRoleControlsDentistLinkAndSelfDeactivation() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var services = TestServices.Empty;
            using var page = new ucUserManagement(services.Users, services.Dentists, new User { Role = Roles.Admin, UserId = 1 });
            using var host = new Form { ClientSize = new(1440, 1000) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); ShowOffscreen(host);
            var roles = Named<ComboBox>(page, "cboRole"); var dentist = Named<ComboBox>(page, "cboDentist");
            roles.SelectedItem = Roles.Dentist; Assert.True(dentist.Enabled); roles.SelectedItem = Roles.Receptionist; Assert.False(dentist.Enabled);
            Assert.False(Named<Button>(page, "btnDeactivate").Enabled);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
}
