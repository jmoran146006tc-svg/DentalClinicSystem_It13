using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.UiTests;

public sealed class PageTitleTests
{
    [Fact]
    public void EveryDestinationHasOnePageHeadingAndAClockOnlyTopBar() => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); using var login = new frmLogin(services);
        using var shell = new frmDashboard(ClinicFixture.Actor(), login, services);
        typeof(frmDashboard).GetField("_loggingOut", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(shell, true);
        shell.WindowState = FormWindowState.Normal; shell.ClientSize = new(1280, 900);
        UiThread.Show(shell);
        var bar = UiThread.Named<TableLayoutPanel>(shell, "shellTopBar");
        var commands = UiThread.Controls(shell).OfType<AntdUI.Button>()
            .Where(b => b.Visible && b.Name != "btnLogout" && b.Controls.OfType<NavItemButton>().Any()).ToArray();
        Assert.Equal(7, commands.Length);
        foreach (var command in commands)
        {
            command.PerformClick(); Application.DoEvents();
            EnglishUi.AssertTree(shell);
            Assert.Equal(command.Text + " - Dental Care", shell.Text);
            Assert.Single(UiThread.Controls(bar).OfType<Label>(), l => l.Visible);
            Assert.DoesNotContain(UiThread.Controls(bar).OfType<Label>(), l => l.Text.Contains(command.Text ?? string.Empty));
            var header = Assert.Single(UiThread.Controls(shell).OfType<PageHeader>(), h => h.Visible);
            var labels = UiThread.Controls(header).OfType<Label>().Where(l => l.Visible);
            Assert.Single(labels, l => command.Text == "Dashboard" ? l.Text.StartsWith("Good ") : l.Text == command.Text);
        }
    });
}
