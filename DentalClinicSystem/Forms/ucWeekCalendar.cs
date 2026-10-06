using System.ComponentModel;
using System.Globalization;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class ucWeekCalendar : UserControl
{
    private readonly ComboBox _dentist = new() { Name = "cboCalendarDentist", AccessibleName = "Filter by dentist", DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = nameof(DisplayOption.Display) };
    private readonly Label _range = new() { Name = "calendarRange", AutoSize = true, Font = Typography.Label, ForeColor = Palette.Ink700, TextAlign = ContentAlignment.MiddleLeft, Margin = new(Space.Sm, 0, Space.Sm, 0) };
    private readonly BufferedPanel _viewport = new() { Name = "calendarViewport", Dock = DockStyle.Fill, AutoScroll = true, BackColor = Palette.Surface, Margin = Padding.Empty };
    private readonly WeekCalendarCanvas _canvas = new();
    private readonly WeekCalendarHeader _header = new();
    private readonly WeekSummaryCanvas _summary = new() { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
    private readonly CalendarTimeGutter _gutter;
    private readonly Label _headerGutter = new() { Dock = DockStyle.Left, BackColor = Palette.Surface, ForeColor = Palette.Ink500, Text = "Time", TextAlign = ContentAlignment.MiddleCenter, Font = Typography.Caption };
    private readonly TableLayoutPanel _layout;
    private readonly FlowLayoutPanel _toolbar;
    private readonly FlowLayoutPanel _legend = new() { Name = "calendarStatusLegend", Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Palette.Surface, Margin = Padding.Empty };
    private readonly Panel _viewHost = new() { Name = "calendarViewHost", BackColor = Palette.Surface, Margin = new(0, 0, Space.Sm, 0) };
    private readonly AntdUI.Segmented _views = new()
    {
        Name = "calendarViews", Font = Typography.Body, Full = true, Radius = Metrics.ControlRadius, Gap = 0,
        BackColor = Palette.Surface, ForeColor = Palette.Ink700, BackActive = Palette.BrandSoft,
        ForeActive = Palette.BrandSoftText, BackHover = Palette.BrandSoft, ForeHover = Palette.BrandSoftText,
        TextAlign = AntdUI.TAlignFlow.Center, IconAlign = AntdUI.TAlignMini.None,
        BorderWidth = Metrics.Border, BorderColor = Palette.LineStrong, SelectIndex = 0,
        AccessibleName = "Calendar view", TabStop = true, Dock = DockStyle.Fill, Margin = Padding.Empty
    };
    private readonly AppButton _previous = UiFactory.Button("", ButtonVariant.Secondary, IconKind.ChevronLeft, ButtonSize.Compact);
    private readonly AppButton _next = UiFactory.Button("", ButtonVariant.Secondary, IconKind.ChevronRight, ButtonSize.Compact);
    private readonly AppButton _expand = UiFactory.Button("Expand", ButtonVariant.Secondary, IconKind.Expand, ButtonSize.Compact);
    private IReadOnlyList<Appointment> _appointments = [];
    private IReadOnlyDictionary<int, string> _patients = new Dictionary<int, string>();
    private IReadOnlyDictionary<int, string> _dentists = new Dictionary<int, string>();
    private IReadOnlySet<int>? _activeDentistIds;
    private DateTime _week = DashboardPresentation.WeekStart(DateTime.Today), _today = DateTime.Today, _selectedDay = DateTime.Today;
    private CalendarView _view;
    private bool _binding, _hasData, _resizing, _fullScreen;
    private int _preferredHeight;
    public event Action<DateTime, int>? WeekRequested;
    public event Action<int>? AppointmentActivated;
    public event Action? ExpandRequested;
    public event Action? PreferredHeightChanged;
    public DateTime WeekStart => _week;
    public int PreferredCalendarHeight => _preferredHeight;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DateTime SelectedDay
    {
        get => _selectedDay;
        set => SelectDay(value.Date);
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CalendarView View
    {
        get => _view;
        set
        {
            if (_view == value) return;
            _view = value; _binding = true; _views.SelectIndex = (int)value; _binding = false; ApplyFilter();
        }
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedDentistId
    {
        get => (_dentist.SelectedItem as DisplayOption)?.Id ?? 0;
        set
        {
            var index = _dentist.Items.Cast<DisplayOption>().ToList().FindIndex(d => d.Id == value);
            _dentist.SelectedIndex = Math.Max(0, index);
        }
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool FullScreen
    {
        get => _fullScreen;
        set { _fullScreen = value; _expand.Visible = !value; ResizeContent(); }
    }
    public ucWeekCalendar()
    {
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this); Theme.MarkPrimitive(_views);
        Name = "weekCalendar"; BackColor = Palette.Surface;
        _preferredHeight = Height = Metrics.CalendarDayHourHeight * 8 + Metrics.CalendarHeaderHeight + Metrics.ControlHeight + Metrics.CompactHeight + Space.Sm;
        _gutter = new(_canvas) { Dock = DockStyle.Left, Width = Metrics.CalendarGutter };
        _layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Palette.Surface, Margin = Padding.Empty };
        _layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        _layout.RowStyles.Add(new(SizeType.AutoSize)); _layout.RowStyles.Add(new(SizeType.AutoSize));
        _layout.RowStyles.Add(new(SizeType.Absolute, Metrics.CalendarHeaderHeight)); _layout.RowStyles.Add(new(SizeType.Percent, 100));
        _toolbar = new FlowLayoutPanel { Name = "calendarToolbar", Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, BackColor = Palette.Surface, Margin = Padding.Empty };
        var today = UiFactory.Button("Today", ButtonVariant.Secondary, size: ButtonSize.Compact); today.Name = "calendarToday";
        _previous.Name = "calendarPrevious"; _next.Name = "calendarNext"; _expand.Name = "calendarExpand";
        var navigation = new FlowLayoutPanel { Name = "calendarNavigation", AutoSize = true, WrapContents = false, Margin = new(0, 0, Space.Sm, 0) };
        foreach (var button in new[] { _previous, today, _next })
        {
            // These synchronous navigation actions never show a busy label.
            button.MinimumSize = new(button == today ? TextRenderer.MeasureText(button.Text, button.Font).Width + Space.Lg * 2 : Metrics.CompactHeight, Metrics.CompactHeight);
            button.Width = button.MinimumSize.Width; button.Margin = new(0, Space.Xs, Space.Xs, Space.Xs); navigation.Controls.Add(button);
        }
        _previous.Click += (_, _) => Navigate(-1); _next.Click += (_, _) => Navigate(1);
        today.Click += (_, _) => SelectDay(_today);
        foreach (var text in new[] { "Day", "Week" }) _views.Items.Add(new AntdUI.SegmentedItem { Text = text });
        _viewHost.Controls.Add(_views);
        _views.SelectIndexChanged += (_, _) => { if (!_binding) View = (CalendarView)_views.SelectIndex; };
        _dentist.Margin = new(Space.Sm, 0, Space.Sm, 0);
        _expand.Width = _expand.MinimumSize.Width; _expand.Margin = new(0, Space.Xs, 0, Space.Xs);
        _expand.Click += (_, _) => ExpandRequested?.Invoke();
        _toolbar.Controls.AddRange([navigation, _viewHost, _range, _dentist, _expand]);
        foreach (var status in AppointmentStatus.All)
        {
            var pair = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new(0, Space.Xs, Space.Lg, Space.Xs) };
            pair.Controls.Add(new Panel { Size = new(Metrics.StatusDot, Metrics.StatusDot), BackColor = Theme.StatusStyle(status).Text, Margin = new(0, Space.Xs, Space.Xs, 0) });
            pair.Controls.Add(new Label { Text = AppointmentStatus.Display(status), AutoSize = true, Font = Typography.Caption, ForeColor = Palette.Ink500, Margin = Padding.Empty });
            _legend.Controls.Add(pair);
        }
        _dentist.SelectedIndexChanged += (_, _) => { if (!_binding) ApplyFilter(); };
        var headerHost = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Palette.Surface };
        var headerClip = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Surface };
        headerClip.Controls.Add(_header); headerHost.Controls.Add(headerClip); headerHost.Controls.Add(_headerGutter);
        var body = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty, BackColor = Palette.Surface };
        _viewport.Controls.Add(_canvas); _viewport.Controls.Add(_summary);
        body.Controls.Add(_viewport); body.Controls.Add(_gutter);
        _viewport.SizeChanged += (_, _) => ResizeContent(); _viewport.Scroll += (_, _) => SyncHeader();
        _canvas.LocationChanged += (_, _) => SyncHeader();
        _canvas.AppointmentActivated += id => AppointmentActivated?.Invoke(id);
        _summary.DayActivated += date => { _selectedDay = date; View = CalendarView.Day; _canvas.Focus(); };
        _layout.Controls.Add(_toolbar, 0, 0); _layout.Controls.Add(_legend, 0, 1); _layout.Controls.Add(headerHost, 0, 2); _layout.Controls.Add(body, 0, 3); Controls.Add(_layout);
        _toolbar.SizeChanged += (_, _) => ResizeContent();
        _legend.SizeChanged += (_, _) => ResizeContent();
        MeasureToolbar(); UpdateRange(); ResizeContent();
    }
    private void MeasureToolbar()
    {
        var height = Metrics.Scale(this, Metrics.ControlHeight);
        using var font = Typography.PixelFont(_views.Font, DeviceDpi);
        _viewHost.Size = new(_views.Items.Cast<AntdUI.SegmentedItem>().Sum(item => TextRenderer.MeasureText(item.Text, font).Width + Metrics.Scale(this, Space.Lg * 2)), height);
        _range.MinimumSize = new(0, height);
        _dentist.Size = new(Metrics.Scale(this, Metrics.CalendarDentistWidth), height);
        _layout.RowStyles[2].Height = Metrics.Scale(this, Metrics.CalendarHeaderHeight);
        foreach (var button in _toolbar.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<AppButton>())
        {
            using var buttonFont = Typography.PixelFont(button.Font, DeviceDpi);
            var buttonHeight = Metrics.Scale(this, Metrics.CompactHeight);
            var width = button == _previous || button == _next ? buttonHeight : TextRenderer.MeasureText(button.Text, buttonFont).Width + Metrics.Scale(this, Space.Lg * 2);
            button.MinimumSize = new(width, buttonHeight); button.Size = button.MinimumSize;
        }
    }
    protected override void OnDpiChangedAfterParent(EventArgs e)
    { base.OnDpiChangedAfterParent(e); MeasureToolbar(); if (_hasData) ApplyFilter(); else ResizeContent(); }
    private void Navigate(int direction) => SelectDay(_selectedDay.AddDays(direction * (_view == CalendarView.Day ? 1 : DashboardPresentation.DaysInWeek)), direction);
    private void SelectDay(DateTime day, int direction = 0)
    {
        _selectedDay = day.Date;
        var week = DashboardPresentation.WeekStart(day);
        if (_hasData && week != _week) { WeekRequested?.Invoke(week, direction == 0 ? Math.Sign((week - _week).Days) : direction); return; }
        if (_hasData) ApplyFilter(direction);
    }
    public void SetAppointments(DateTime monday, DateTime today, IReadOnlyList<Appointment> appointments,
        IReadOnlyDictionary<int, string> patients, IReadOnlyDictionary<int, string> dentists, int direction = 0, IReadOnlySet<int>? activeDentistIds = null)
    {
        var selected = SelectedDentistId;
        _week = monday.Date; _today = today.Date; _appointments = appointments; _patients = patients; _dentists = dentists; _activeDentistIds = activeDentistIds;
        if (!_hasData || _selectedDay < _week || _selectedDay >= _week.AddDays(7))
            _selectedDay = _today >= _week && _today < _week.AddDays(7) ? _today :
                Enumerable.Range(0, 7).Select(i => _week.AddDays(i)).FirstOrDefault(day => !ClinicRules.ClosedDays.Contains(day.DayOfWeek), _week);
        _hasData = true; _binding = true;
        try
        {
            _dentist.Items.Clear(); _dentist.Items.Add(new DisplayOption(0, "All dentists"));
            _dentist.Items.AddRange(dentists.OrderBy(d => d.Value).Select(d => new DisplayOption(d.Key, d.Value)).ToArray());
            SelectedDentistId = selected;
        }
        finally { _binding = false; }
        ApplyFilter(direction);
    }
    public void CopyStateTo(ucWeekCalendar target)
    {
        target.SetAppointments(_week, _today, _appointments, _patients, _dentists, activeDentistIds: _activeDentistIds);
        target.SelectedDay = _selectedDay; target.View = _view; target.SelectedDentistId = SelectedDentistId;
    }
    private void UpdateRange()
    {
        _range.Text = _view == CalendarView.Day ? _selectedDay.ToString("ddd, MMM d, yyyy", CultureInfo.InvariantCulture) : DashboardPresentation.WeekLabel(_week);
        _previous.AccessibleName = _view == CalendarView.Day ? "Previous day" : "Previous week";
        _next.AccessibleName = _view == CalendarView.Day ? "Next day" : "Next week";
        Tooltips.Attach(_previous, _previous.AccessibleName); Tooltips.Attach(_next, _next.AccessibleName);
    }
    private void ApplyFilter(int direction = 0)
    {
        if (!_hasData) return;
        UpdateRange(); var id = SelectedDentistId;
        var matrix = _view == CalendarView.Week && id == 0;
        var visible = _appointments.Where(a => (id == 0 || a.DentistId == id) && (_view == CalendarView.Week || a.AppointmentDateTime.Date == _selectedDay)).ToArray();
        // Changing between a wide day, matrix and time grid invalidates the
        // old scroll extent. Clear it before measuring the new visible surface.
        _viewport.AutoScroll = false;
        _viewport.SuspendLayout();
        try
        {
            _viewport.AutoScrollPosition = Point.Empty; _summary.Visible = matrix; _canvas.Visible = !matrix; _gutter.Visible = !matrix;
            _canvas.Anchor = _view == CalendarView.Week ? AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right : AnchorStyles.Top | AnchorStyles.Left;
            if (matrix)
            {
                var summary = WeekSummary.Create(_week, _appointments, _dentists, _activeDentistIds);
                _summary.SetSummary(summary, _today);
                _header.SetColumns(summary.Days.Select(day => new CalendarColumn(day, 0, day.ToString("ddd  MMM d", CultureInfo.InvariantCulture), 0)).ToArray(), _today, CalendarView.Week, 0);
            }
            else
            {
                var columns = _view == CalendarView.Day ? DayScheduleGeometry.Columns(_selectedDay, visible, _dentists, _activeDentistIds, id) :
                    WeekSummary.VisibleDays(_week, _appointments).Select(day => new CalendarColumn(day, id, day.ToString("ddd  MMM d", CultureInfo.InvariantCulture), visible.Count(a => a.AppointmentDateTime.Date == day))).ToArray();
                _canvas.SetSchedule(_week, _today, _view, columns, visible, _patients, _dentists, direction);
                _header.SetColumns(columns, _today, _view, 0);
            }
        }
        finally { _viewport.ResumeLayout(true); }
        ResizeContent(); _viewport.AutoScroll = true; ResizeContent(); ScrollToContext(visible);
    }
    private void ScrollToContext(IReadOnlyList<Appointment> appointments)
    {
        if (!_viewport.VerticalScroll.Visible || !_canvas.Visible) return;
        var now = DateTime.Now;
        var hour = _canvas.Columns.Any(c => c.Date == now.Date) ? now.TimeOfDay.TotalHours :
            appointments.OrderBy(a => a.AppointmentDateTime).FirstOrDefault()?.AppointmentDateTime.TimeOfDay.TotalHours ?? _canvas.StartHour;
        _viewport.AutoScrollPosition = new(0, Math.Clamp((int)((hour - _canvas.StartHour - 1) * _canvas.HourHeight), 0, Math.Max(0, _canvas.Height - _viewport.ClientSize.Height)));
        SyncHeader();
    }
    private void ResizeContent()
    {
        if (_resizing || _gutter is null || _layout is null) return;
        _resizing = true;
        try
        {
            var matrix = _view == CalendarView.Week && SelectedDentistId == 0;
            var width = Math.Max(1, _viewport.ClientSize.Width);
            if (matrix)
            {
                _summary.Width = width;
                _header.Width = Math.Max(1, width - _summary.LabelWidth);
                _headerGutter.Text = "Dentist";
                _headerGutter.Width = _summary.LabelWidth;
            }
            else
            {
                var gutterWidth = Metrics.Scale(this, Metrics.CalendarGutter);
                _gutter.Width = _headerGutter.Width = gutterWidth;
                _canvas.Width = _view == CalendarView.Day ? DayScheduleGeometry.Width(width + gutterWidth, _canvas.Columns.Count, DeviceDpi) - gutterWidth : width;
                _canvas.FitHeight(Math.Max(1, _viewport.ClientSize.Height - (_canvas.Width > width ? SystemInformation.HorizontalScrollBarHeight : 0)), _fullScreen);
                _header.Width = _canvas.Width;
                _headerGutter.Text = "Time";
            }
            _header.Height = _headerGutter.Height = Metrics.Scale(this, Metrics.CalendarHeaderHeight);
            _header.SetGutter(0);
            // Keep ordinary hours in the page's scroll flow. Extra hours and
            // dentist columns get inner scrollbars only when they cannot fit.
            var standard = matrix ? _summary.Height : Metrics.Scale(this, (_view == CalendarView.Day ? Metrics.CalendarDayHourHeight : Metrics.CalendarHourHeight) * 8);
            var preferred = _toolbar.Height + _legend.Height + _header.Height + standard + Metrics.Scale(this, Space.Xs) + (!matrix && _canvas.Width > width ? SystemInformation.HorizontalScrollBarHeight : 0);
            if (_preferredHeight != preferred) { _preferredHeight = preferred; PreferredHeightChanged?.Invoke(); }
            if (!_fullScreen && Dock == DockStyle.Top && Height != preferred) Height = preferred;
            SyncHeader();
        }
        finally { _resizing = false; }
    }
    private void SyncHeader()
    {
        _header.Left = _summary.Visible ? _summary.Left : _canvas.Left;
        _gutter.Invalidate();
    }
}
