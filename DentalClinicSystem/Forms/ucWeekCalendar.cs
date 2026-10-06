using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public sealed class ucWeekCalendar : UserControl
{
    private readonly ComboBox _dentist = new() { Name = "cboCalendarDentist", AccessibleName = "Filter by dentist", DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(DisplayOption.Display) };
    private readonly Label _range = new() { Dock = DockStyle.Fill, AutoEllipsis = true, Font = Typography.Label, ForeColor = Palette.Ink700, TextAlign = ContentAlignment.MiddleLeft, Margin = new(Space.Sm, 0, Space.Sm, 0) };
    private readonly BufferedPanel _viewport = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Palette.Surface };
    private readonly WeekCalendarCanvas _canvas = new();
    private readonly WeekCalendarHeader _header = new();
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
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this);
        Name = "weekCalendar"; BackColor = Palette.Surface; Height = Metrics.CalendarViewportHeight + Metrics.ControlHeight;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Palette.Surface };
        layout.Margin = Padding.Empty;
        layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.RowStyles.Add(new(SizeType.Absolute, Metrics.ControlHeight)); layout.RowStyles.Add(new(SizeType.Absolute, Metrics.CalendarHeaderHeight)); layout.RowStyles.Add(new(SizeType.Percent, 100));
        var toolbar = new TableLayoutPanel { Name = "calendarToolbar", Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Palette.Surface, Margin = Padding.Empty };
        toolbar.ColumnStyles.Add(new(SizeType.AutoSize)); toolbar.ColumnStyles.Add(new(SizeType.Percent, 100));
        toolbar.ColumnStyles.Add(new(SizeType.Absolute, Metrics.CalendarDentistWidth)); toolbar.ColumnStyles.Add(new(SizeType.AutoSize));
        var navigation = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Left, Margin = Padding.Empty };
        var previous = UiFactory.Button("Previous", ButtonVariant.Secondary, IconKind.ChevronLeft, ButtonSize.Compact);
        var next = UiFactory.Button("Next", ButtonVariant.Secondary, IconKind.ChevronRight, ButtonSize.Compact);
        var today = UiFactory.Button("Today", ButtonVariant.Secondary, size: ButtonSize.Compact);
        previous.AccessibleName = "Previous week"; next.AccessibleName = "Next week";
        foreach (var button in new[] { previous, today, next }) { button.Width = button.MinimumSize.Width; button.Margin = new(0, 0, Space.Xs, 0); navigation.Controls.Add(button); }
        previous.Click += (_, _) => Request(_week.AddDays(-DashboardPresentation.DaysInWeek));
        next.Click += (_, _) => Request(_week.AddDays(DashboardPresentation.DaysInWeek));
        today.Click += (_, _) => Request(DashboardPresentation.WeekStart(_today));
        var legend = new FlowLayoutPanel { Name = "calendarStatusLegend", AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Right, Margin = Padding.Empty };
        foreach (var status in AppointmentStatus.All)
        {
            var pair = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new(Space.Xs, 0, 0, 0) };
            pair.Controls.Add(new Panel { Size = new(Metrics.StatusDot, Metrics.StatusDot), BackColor = Theme.StatusStyle(status).Text, Margin = new(0, Space.Xs, Space.Xs, 0) });
            pair.Controls.Add(new Label { Text = AppointmentStatus.Display(status), AutoSize = true, Font = Typography.Caption, ForeColor = Palette.Ink500, Margin = Padding.Empty });
            legend.Controls.Add(pair);
        }
        _dentist.Dock = DockStyle.Fill; _dentist.Margin = Padding.Empty;
        toolbar.Controls.Add(navigation, 0, 0); toolbar.Controls.Add(_range, 1, 0); toolbar.Controls.Add(_dentist, 2, 0); toolbar.Controls.Add(legend, 3, 0);
        toolbar.SizeChanged += (_, _) => legend.Visible = Width >= Metrics.Scale(this, Metrics.CalendarLegendWidth) &&
            toolbar.Width >= navigation.PreferredSize.Width + legend.PreferredSize.Width + Metrics.Scale(this, Metrics.CalendarDentistWidth) + TextRenderer.MeasureText(_range.Text, _range.Font).Width + _range.Margin.Horizontal;
        _dentist.SelectedIndexChanged += (_, _) => { if (!_binding) ApplyFilter(); };
        _viewport.Controls.Add(_canvas); _viewport.Controls.Add(_empty);
        _viewport.SizeChanged += (_, _) => ResizeCanvas();
        _canvas.AppointmentActivated += id => AppointmentActivated?.Invoke(id);
        var headerHost = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Palette.Surface };
        headerHost.Controls.Add(_header); _viewport.Margin = Padding.Empty;
        _viewport.Scroll += (_, _) => _header.Left = _canvas.Left;
        layout.Controls.Add(toolbar, 0, 0); layout.Controls.Add(headerHost, 0, 1); layout.Controls.Add(_viewport, 0, 2); Controls.Add(layout);
        DpiChangedAfterParent += (_, _) =>
        {
            layout.RowStyles[0].Height = Metrics.Scale(this, Metrics.ControlHeight); layout.RowStyles[1].Height = Metrics.Scale(this, Metrics.CalendarHeaderHeight);
            toolbar.ColumnStyles[2].Width = Metrics.Scale(this, Metrics.CalendarDentistWidth); ResizeCanvas();
        };
    }
    private void Request(DateTime week) => WeekRequested?.Invoke(week, Math.Sign((week - _week).Days));
    public void SetAppointments(DateTime monday, DateTime today, IReadOnlyList<Appointment> appointments,
        IReadOnlyDictionary<int, string> patients, IReadOnlyDictionary<int, string> dentists, int direction = 0)
    {
        var selected = (_dentist.SelectedItem as DisplayOption)?.Id ?? 0;
        _week = monday.Date; _today = today.Date; _appointments = appointments; _patients = patients; _dentists = dentists;
        _header.SetWeek(_week, _today);
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
        ScrollToContext(visible);
    }
    private void ScrollToContext(IReadOnlyList<Appointment> appointments)
    {
        if (!_viewport.VerticalScroll.Visible) { _viewport.AutoScrollPosition = Point.Empty; return; }
        var now = DateTime.Now;
        var hour = now.Date >= _week && now.Date < _week.AddDays(DashboardPresentation.DaysInWeek) ? now.TimeOfDay.TotalHours
            : appointments.OrderBy(a => a.AppointmentDateTime).FirstOrDefault()?.AppointmentDateTime.TimeOfDay.TotalHours ?? _canvas.StartHour;
        var maximum = Math.Max(0, _canvas.Height - _viewport.ClientSize.Height);
        _viewport.AutoScrollPosition = new(0, Math.Clamp((int)((hour - _canvas.StartHour - 1) * Metrics.Scale(this, Metrics.CalendarHourHeight)), 0, maximum));
    }
    private void ResizeCanvas()
    {
        _canvas.Width = Math.Max(CalendarGeometry.MinimumWidth(DeviceDpi), _viewport.ClientSize.Width);
        _header.Bounds = new(_canvas.Left, 0, _canvas.Width, Metrics.Scale(this, Metrics.CalendarHeaderHeight));
    }
}
