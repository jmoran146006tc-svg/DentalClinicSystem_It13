using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogin));
            var background = Assert.IsAssignableFrom<Image>(resources.GetObject("$this.BackgroundImage"));
            var logo = Assert.IsAssignableFrom<Image>(resources.GetObject("pictureBox1.Image"));
            Assert.Equal(background.Size, login.BackgroundImage!.Size);
            Assert.Equal(ImageLayout.Zoom, login.BackgroundImageLayout);
            var brand = Descendants(login).OfType<LoginBrand>().Single();
            Assert.Same(login.BackgroundImage, brand.BackgroundImage);
            Assert.NotEqual(logo.Size, brand.LogoImage!.Size);
            Assert.InRange(Math.Abs((double)brand.Width / brand.Height - (double)background.Width / background.Height), 0, .005);
            Assert.Equal(Palette.Surface, Named<Button>(login, "btnLogin").Parent!.Parent!.BackColor);
            foreach (var input in new[] { Named<TextBox>(login, "txtUsername"), Named<TextBox>(login, "txtPassword") })
                Assert.True(input.Parent!.Height >= input.Height);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    [Fact]
    public void PatientFormReusesDesignerInputsInModalAndGridFillsPage() => PresentationTests.Sta(() =>
    {
        MotionSystem.Enabled = false;
        try
        {
            var services = TestServices.Empty;
            using var page = new ucPatientRecords(services.Patients, new User { Role = Roles.Admin });
            using var host = new Form { ClientSize = new(1440, 1000) }; host.Controls.Add(page); page.Dock = DockStyle.Fill; ShowOffscreen(host);
            Assert.DoesNotContain(Descendants(page), control => control.Name == "btnDelete");
            Assert.DoesNotContain(Descendants(page), control => control is ResponsiveSplit);
            Assert.False(page.AutoScroll);
            var newButton = Descendants(page).OfType<AppButton>().Single(button => button.Text == "New patient");
            InspectNextDialog(host, dialog => { Assert.IsType<PatientDialog>(dialog); Assert.True(Named<TextBox>(dialog, "txtFirstName").Visible); }, newButton.PerformClick);
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
            var newButton = Descendants(page).OfType<AppButton>().Single(button => button.Text == "New user");
            InspectNextDialog(host, dialog =>
            {
                var roles = Named<ComboBox>(dialog, "cboRole"); var dentist = Named<ComboBox>(dialog, "cboDentist");
                roles.SelectedItem = Roles.Dentist; Assert.True(dentist.Enabled); roles.SelectedItem = Roles.Receptionist; Assert.False(dentist.Enabled);
            }, newButton.PerformClick);
            Assert.False(Named<Button>(page, "btnDeactivate").Enabled);
        }
        finally { MotionSystem.UseSystemPreference(); }
    });
    internal static void InspectNextDialog(Form host, Action<RecordDialog> inspect, Action open, bool save = false)
    {
        Exception? failure = null;
        host.BeginInvoke(() =>
        {
            var dialog = Application.OpenForms.OfType<RecordDialog>().Single();
            try { inspect(dialog); }
            catch (Exception error) { failure = error; }
            finally { if (!save || failure is not null) dialog.CloseAnimated(DialogResult.Cancel); }
        });
        open();
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
