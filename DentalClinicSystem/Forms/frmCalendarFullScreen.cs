using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Helpers.Native;

namespace DentalClinicSystem.Forms;

public sealed class frmCalendarFullScreen : Form
{
    public ucWeekCalendar Calendar { get; } = new() { Dock = DockStyle.Fill, FullScreen = true };
    public frmCalendarFullScreen(ucWeekCalendar source,
        Func<ucWeekCalendar, DateTime, int, Task<bool>> loadWeek, Func<int, IWin32Window, Task> openDetails)
    {
        Text = "Dental Care · Calendar"; Name = "calendarFullScreen"; AccessibleName = "Full screen appointment calendar";
        WindowState = FormWindowState.Maximized; BackColor = Palette.Surface; Padding = Space.Page;
        AutoScaleMode = AutoScaleMode.Dpi; KeyPreview = true; WindowChrome.Apply(this);
        var exit = UiFactory.Button("Exit full screen", ButtonVariant.Secondary, IconKind.Close);
        exit.Name = "calendarExitFullScreen"; exit.Width = exit.MinimumSize.Width; exit.Click += (_, _) => Close();
        var header = new PageHeader("Appointment calendar", "Esc to exit full screen", exit) { Dock = DockStyle.Top };
        Controls.Add(Calendar); Controls.Add(header); source.CopyStateTo(Calendar);
        Calendar.WeekRequested += async (week, direction) => await UiAction.RunAsync(this, async () => { await loadWeek(Calendar, week, direction); });
        Calendar.AppointmentActivated += async id => await UiAction.RunAsync(this, async () =>
        {
            await openDetails(id, this);
            if (!IsDisposed) await loadWeek(Calendar, Calendar.WeekStart, 0);
        });
    }
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape) { Close(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }
}
