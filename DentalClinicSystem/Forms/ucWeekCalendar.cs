using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public sealed class ucWeekCalendar : UserControl
{
    private readonly ComboBox _dentist = new() { Name = "cboCalendarDentist", DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(DisplayOption.Display) };
    private readonly Label _range = new() { AutoSize = true, Font = Typography.Label, ForeColor = Palette.Ink700, Margin = new Padding(Space.Sm, Space.Md, Space.Sm, 0) };
    private readonly Panel _viewport = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Palette.Surface };
    private readonly WeekCalendarCanvas _canvas = new();
    private readonly EmptyState _empty = new("No appointments this week", "Choose another week or dentist to view appointments.", IconKind.Appointments) { Visible = false };
    private IReadOnlyList<Appointment> _appointments = [];
    private IReadOnlyDictionary<int, string> _patients = new Dictionary<int, string>();
    private IReadOnlyDictionary<int, string> _dentists = new Dictionary<int, string>();
    private DateTime _week = DashboardPresentation.WeekStart(DateTime.Today), _today = DateTime.Today;
    private bool _binding;
    public event Action<DateTime, int>? WeekRequested;
    public event Action<int>? AppointmentActivated;
    public DateTime WeekStart => _week;
    public ucWeekCalendar()
    {
        Theme.MarkPrimitive(this); DesignPaint.Enable(this);
        Name = "weekCalendar"; BackColor = Palette.Surface; Height = Metrics.CalendarViewportHeight;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Palette.Surface };
        layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.RowStyles.Add(new(SizeType.AutoSize)); layout.RowStyles.Add(new(SizeType.AutoSize)); layout.RowStyles.Add(new(SizeType.Percent, 100));
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Palette.Surface };
        var previous = UiFactory.Button("Previous week", ButtonVariant.Ghost, IconKind.ChevronLeft, ButtonSize.Compact);
        var next = UiFactory.Button("Next week", ButtonVariant.Ghost, IconKind.ChevronRight, ButtonSize.Compact);
        var today = UiFactory.Button("Today", ButtonVariant.Secondary, size: ButtonSize.Compact);
        previous.Click += (_, _) => Request(_week.AddDays(-DashboardPresentation.DaysInWeek));
        next.Click += (_, _) => Request(_week.AddDays(DashboardPresentation.DaysInWeek));
        today.Click += (_, _) => Request(DashboardPresentation.WeekStart(_today));
        toolbar.Controls.AddRange([previous, today, next, _range]);
        var filter = FieldBox.Wrap(_dentist, "Dentist", FieldKind.Choice); filter.Dock = DockStyle.Top; filter.Width = Metrics.FormWidth;
        _dentist.SelectedIndexChanged += (_, _) => { if (!_binding) ApplyFilter(); };
        _viewport.Controls.Add(_canvas); _viewport.Controls.Add(_empty);
        _viewport.SizeChanged += (_, _) => ResizeCanvas();
        _canvas.AppointmentActivated += id => AppointmentActivated?.Invoke(id);
        layout.Controls.Add(toolbar, 0, 0); layout.Controls.Add(filter, 0, 1); layout.Controls.Add(_viewport, 0, 2); Controls.Add(layout);
    }
    private void Request(DateTime week) => WeekRequested?.Invoke(week, Math.Sign((week - _week).Days));
    public void SetAppointments(DateTime monday, DateTime today, IReadOnlyList<Appointment> appointments,
        IReadOnlyDictionary<int, string> patients, IReadOnlyDictionary<int, string> dentists, int direction = 0)
    {
        var selected = (_dentist.SelectedItem as DisplayOption)?.Id ?? 0;
        _week = monday.Date; _today = today.Date; _appointments = appointments; _patients = patients; _dentists = dentists;
        _range.Text = DashboardPresentation.WeekLabel(monday); _binding = true;
        try
        {
            _dentist.Items.Clear(); _dentist.Items.Add(new DisplayOption(0, "All dentists"));
            _dentist.Items.AddRange(dentists.OrderBy(d => d.Value).Select(d => new DisplayOption(d.Key, d.Value)).ToArray());
            _dentist.SelectedIndex = Math.Max(0, _dentist.Items.Cast<DisplayOption>().ToList().FindIndex(d => d.Id == selected));
        }
        finally { _binding = false; }
        ApplyFilter(direction);
    }
    private void ApplyFilter(int direction = 0)
    {
        var id = (_dentist.SelectedItem as DisplayOption)?.Id ?? 0;
        var visible = _appointments.Where(a => id == 0 || a.DentistId == id).ToArray();
        _empty.Visible = visible.Length == 0; _canvas.Visible = visible.Length > 0;
        _canvas.SetAppointments(_week, _today, visible, _patients, _dentists, direction);
        if (_empty.Visible) _empty.BringToFront();
        ResizeCanvas();
    }
    private void ResizeCanvas() => _canvas.Width = Math.Max(CalendarGeometry.MinimumWidth(DeviceDpi), _viewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
}
