using System.Reflection;
using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.UiTests;

public sealed class BackgroundTests
{
    [Theory]
    [InlineData("dashboard")] [InlineData("reports")] [InlineData("history")] [InlineData("patients")]
    public void PagePaddingPaintsCanvas(string kind) => UiThread.Run(() =>
    {
        var s = ClinicFixture.Services(); var actor = ClinicFixture.Actor();
        using UserControl page = kind switch
        {
            "dashboard" => new ucDashboardHome(s.Appointments, s.Patients, s.Dentists, s.Reports, s.PatientHistory, actor),
            "reports" => new ucReports(s.Reports, actor),
            "history" => new ucPatientHistory(s.PatientHistory, actor, 4),
            _ => new ucPatientRecords(s.Patients, actor)
        };
        using var host = new Form { ClientSize = new(1100, 800), BackColor = Palette.Canvas };
        page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        using var bitmap = new Bitmap(page.Width, page.Height); page.DrawToBitmap(bitmap, page.ClientRectangle);
        UiThread.Capture(host, "fix-A-" + kind);
        foreach (var point in new[] { new Point(Space.Page.Left / 2, page.ClientSize.Height / 2), new Point(page.ClientSize.Width / 2, Space.Page.Top / 2), new Point(page.ClientSize.Width - Space.Page.Right / 2, page.ClientSize.Height / 2) })
            Assert.Equal(Palette.Canvas.ToArgb(), bitmap.GetPixel(point.X, point.Y).ToArgb());
    });

    [Theory]
    [InlineData(Roles.Admin)] [InlineData(Roles.Receptionist)] [InlineData(Roles.Dentist)]
    public void SidebarEdgeIsOpaqueSurface(string role) => UiThread.Run(() =>
    {
        var s = ClinicFixture.Services(); using var login = new frmLogin(s);
        using var shell = new frmDashboard(ClinicFixture.Actor(role), login, s);
        typeof(frmDashboard).GetField("_loggingOut", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(shell, true);
        shell.WindowState = FormWindowState.Normal; shell.ClientSize = new(1100, 800); UiThread.Show(shell);
        var sidebar = UiThread.Named<Panel>(shell, "pnlSidebar");
        using var bitmap = new Bitmap(sidebar.Width, sidebar.Height); sidebar.DrawToBitmap(bitmap, sidebar.ClientRectangle);
        UiThread.Capture(shell, "fix-A-shell-" + role);
        Assert.Equal(Palette.SidebarBg.ToArgb(), bitmap.GetPixel(Space.Xs, sidebar.Height / 2).ToArgb());
        for (var x = bitmap.Width - 6; x < bitmap.Width; x++) for (var y = 0; y < bitmap.Height; y++)
            Assert.NotEqual(Palette.Black.ToArgb(), bitmap.GetPixel(x, y).ToArgb());
    });
}
