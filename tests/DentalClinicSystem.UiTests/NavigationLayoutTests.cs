using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.UiTests;

public sealed class NavigationLayoutTests
{
    [Theory]
    [InlineData(Roles.Admin)] [InlineData(Roles.Receptionist)] [InlineData(Roles.Dentist)]
    public void NavigationHasFixedColumnsAndUniformBoxes(string role) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); using var login = new frmLogin(services);
        using var shell = new frmDashboard(ClinicFixture.Actor(role), login, services);
        typeof(frmDashboard).GetField("_loggingOut", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(shell, true);
        shell.WindowState = FormWindowState.Normal; shell.ClientSize = new(1280, 900); UiThread.Show(shell);
        var items = UiThread.Controls(shell).OfType<NavItemButton>().Where(b => b.Visible).ToArray();
        Assert.NotEmpty(items);
        Assert.Single(items.Select(b => b.PointToScreen(Point.Empty).X).Distinct());
        Assert.Single(items.Select(b => b.Size).Distinct());
        Assert.Single(items.Select(b => b.Font).Distinct());
        Assert.Single(items.Select(b => b.IconBounds.X).Distinct());
        Assert.Single(items.Select(b => b.TextBounds.X).Distinct());
        Assert.All(items, b => Assert.Equal(Metrics.Scale(b, Metrics.NavHeight), b.Height));
        var title = UiThread.Named<Label>(shell, "brandTitle");
        Assert.Equal(title.PreferredHeight, title.Height);
        Assert.True(TextRenderer.MeasureText(title.Text, title.Font).Width <= title.Width);
        Assert.Equal(items[0].PointToScreen(Point.Empty).X, title.PointToScreen(Point.Empty).X);
        var patients = items.FirstOrDefault(b => b.Text == "Patients");
        if (patients is not null)
        {
            var size = patients.Size;
            typeof(NavItemButton).GetMethod("OnKeyUp", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(patients, [new KeyEventArgs(Keys.Enter)]);
            Assert.Equal("Patients - Dental Care", shell.Text); Assert.True(patients.Selected); Assert.Equal(size, patients.Size);
        }
        UiThread.Capture(shell, "fix-B-sidebar-" + role);
    });

    [Theory]
    [InlineData(96)] [InlineData(120)] [InlineData(144)]
    public void BrandTextFitsOneLineAtScaledFontMetrics(int dpi) => UiThread.Run(() =>
    {
        using var font = Typography.PixelFont(Typography.Heading, dpi);
        var text = TextRenderer.MeasureText("Dental Care", font, Size.Empty, TextFormatFlags.SingleLine);
        Assert.True(text.Width <= (Metrics.SidebarWidth - Space.Xl * 2) * dpi / Metrics.BaselineDpi);
    });

    [Fact]
    public void DestinationIconsAreDistinct() => UiThread.Run(() =>
    {
        Assert.Equal(3, new[] { IconKind.Patients, IconKind.Dentist, IconKind.Users }.Select(AntdTheme.Svg).Distinct().Count());
        using var patient = Icons.Build(IconKind.Patients); using var users = Icons.Build(IconKind.Users);
        Assert.False(patient.PathPoints.SequenceEqual(users.PathPoints));
    });
}
