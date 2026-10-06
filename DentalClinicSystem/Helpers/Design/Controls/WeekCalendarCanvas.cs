using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WeekCalendarCanvas : DesignControl
{
    private sealed record Entry(CalendarBlock Block, string Patient, string Slot, string Tooltip, SemanticStyle Style);
    private IReadOnlyList<Appointment> _appointments = [];
    private IReadOnlyList<CalendarBlock> _blocks = [];
    private IReadOnlyList<Entry> _entries = [];
    private CalendarGeometry _geometry = CalendarGeometry.ForWeek([], DateTime.Today);
    private string[] HourLabels = [];
    private readonly System.Windows.Forms.Timer _nowTimer = new() { Interval = Metrics.CalendarNowInterval };
    private readonly List<Control> _visibilityAncestors = [];
    private IReadOnlyDictionary<int, string> _patients = new Dictionary<int, string>();
    private IReadOnlyDictionary<int, string> _dentists = new Dictionary<int, string>();
    private DateTime _monday = DashboardPresentation.WeekStart(DateTime.Today);
    private DateTime _today = DateTime.Today;
    private float _revealMilliseconds, _weekProgress = 1;
    private int _direction, _selected = -1, _hovered = -1;
    public event Action<int>? AppointmentActivated;
    public IReadOnlyList<CalendarBlock> Blocks => _blocks;
    public int StartHour => _geometry.StartHour;
    public int EndHour => _geometry.EndHour;
    public WeekCalendarCanvas()
    {
        Name = "weekCalendarCanvas"; TabStop = true; AccessibleRole = AccessibleRole.List;
        AccessibleName = "Week appointments. Use arrow keys to choose an appointment and Enter to open it.";
        Size = new(CalendarGeometry.MinimumWidth(DeviceDpi), _geometry.Height(DeviceDpi));
        GotFocus += (_, _) => { if (_selected < 0 && _blocks.Count > 0) _selected = 0; Invalidate(); };
        LostFocus += (_, _) => Invalidate();
        _nowTimer.Tick += (_, _) => Invalidate();
    }
    public void SetAppointments(DateTime monday, DateTime today, IReadOnlyList<Appointment> appointments,
        IReadOnlyDictionary<int, string> patients, IReadOnlyDictionary<int, string> dentists, int direction = 0)
    {
        MotionSystem.Animator.Cancel(this);
        _monday = monday.Date; _today = today.Date; _appointments = appointments;
        _patients = patients; _dentists = dentists; _direction = Math.Sign(direction); _selected = -1; _hovered = -1;
        _geometry = CalendarGeometry.ForWeek(appointments, monday);
        Rebuild();
        var total = (float)MotionSystem.Base.TotalMilliseconds + Math.Min(Math.Max(0, _blocks.Count - 1), MotionSystem.MaxStagger - 1) * MotionSystem.StaggerStep;
        MotionSystem.Animator.Run(this, "blocks-reveal", 0, total, TimeSpan.FromMilliseconds(total), Easing.Linear,
            t => { _revealMilliseconds = t; InvalidateAppointments(); });
        MotionSystem.Animator.Run(this, "week-slide", 0, 1, MotionSystem.Base, Easing.EaseOutCubic,
            t => { _weekProgress = Math.Clamp(t, 0, 1); InvalidateAppointments(); });
        Invalidate();
    }
    private void Rebuild()
    {
        Height = _geometry.Height(DeviceDpi);
        HourLabels = Enumerable.Range(StartHour, EndHour - StartHour + 1)
            .Select(hour => DisplayFormat.Time(DateTime.MinValue.AddHours(hour))).ToArray();
        _blocks = _geometry.Blocks(_appointments, _monday, Width, DeviceDpi);
        _entries = _blocks.Select(block =>
        {
            var a = block.Appointment;
            var names = $"{_patients.GetValueOrDefault(a.PatientId, $"Patient #{a.PatientId}")} / {_dentists.GetValueOrDefault(a.DentistId, $"Dentist #{a.DentistId}")}";
            var status = AppointmentStatus.Display(a.Status);
            var patient = _patients.GetValueOrDefault(a.PatientId, $"Patient #{a.PatientId}");
            var dentist = _dentists.GetValueOrDefault(a.DentistId);
            const string title = "Dr. ";
            if (dentist?.StartsWith(title, StringComparison.OrdinalIgnoreCase) == true) dentist = dentist[title.Length..];
            var slot = $"{a.AppointmentDateTime.ToString("h:mm", System.Globalization.CultureInfo.InvariantCulture)}–{DisplayFormat.Time(a.AppointmentDateTime.AddMinutes(a.DurationMinutes))} · Dr. {DisplayFormat.Initials(dentist)}";
            return new Entry(block, patient, slot,
                $"{DisplayFormat.DateTime(a.AppointmentDateTime)} – {DisplayFormat.Time(a.AppointmentDateTime.AddMinutes(a.DurationMinutes))}\n{names}\n{status}\nReason: {DisplayFormat.Optional(a.Reason)}", Theme.StatusStyle(a.Status));
        }).ToArray();
    }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Rebuild(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Rebuild(); }
    private void InvalidateAppointments()
    {
        Invalidate();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this); e.Graphics.Clear(Palette.Surface);
        DrawGrid(e.Graphics);
        DrawNow(e.Graphics);
        for (var index = 0; index < _entries.Count; index++) DrawAppointment(e.Graphics, _entries[index], index);
    }
    private void DrawGrid(Graphics graphics)
    {
        var gutter = Metrics.Scale(this, Metrics.CalendarGutter);
        var hourHeight = Metrics.Scale(this, Metrics.CalendarHourHeight);
        var column = (Width - gutter) / (double)DashboardPresentation.DaysInWeek;
        if (column <= 0) return;
        using var todayBrush = new SolidBrush(Palette.BrandSoft);
        using var closedBrush = new SolidBrush(Palette.SurfaceAlt);
        using var pen = new Pen(Palette.Line, Metrics.Scale(this, Metrics.Border));
        for (var day = 0; day < DashboardPresentation.DaysInWeek; day++)
        {
            var date = _monday.AddDays(day); var x = gutter + (int)Math.Round(day * column);
            var right = gutter + (int)Math.Round((day + 1) * column);
            if (date == _today) graphics.FillRectangle(todayBrush, x, 0, right - x, Height);
            if (ClinicRules.ClosedDays.Contains(date.DayOfWeek)) graphics.FillRectangle(closedBrush, x, 0, right - x, Height);
            else
            {
                var opening = (ClinicRules.OpeningTime.ToTimeSpan().TotalHours - StartHour) * hourHeight;
                var closing = (ClinicRules.ClosingTime.ToTimeSpan().TotalHours - StartHour) * hourHeight;
                if (opening > 0) graphics.FillRectangle(closedBrush, x, 0, right - x, (float)opening);
                if (closing < Height) graphics.FillRectangle(closedBrush, x, (float)closing, right - x, Height - (float)closing);
            }
            graphics.DrawLine(pen, x, 0, x, Height);
        }
        using var font = Typography.PixelFont(Typography.Caption, DeviceDpi);
        var labelHeight = TextRenderer.MeasureText("Ag", font).Height;
        for (var hour = StartHour; hour <= EndHour; hour++)
        {
            var y = (hour - StartHour) * hourHeight;
            graphics.DrawLine(pen, gutter, y, Width, y);
            TextRenderer.DrawText(graphics, HourLabels[hour - StartHour], font,
                new Rectangle(Metrics.Scale(this, Space.Xs), Math.Clamp(y - labelHeight / 2, 0, Math.Max(0, Height - labelHeight)), gutter - Metrics.Scale(this, Space.Sm), labelHeight), Palette.Ink500, DesignPaint.TextFlags);
        }
    }
    private void DrawNow(Graphics graphics)
    {
        var now = DateTime.Now;
        var day = (now.Date - _monday).Days;
        if (day < 0 || day >= DashboardPresentation.DaysInWeek || now.TimeOfDay.TotalHours < StartHour || now.TimeOfDay.TotalHours > EndHour) return;
        var gutter = Metrics.Scale(this, Metrics.CalendarGutter); var column = (Width - gutter) / (double)DashboardPresentation.DaysInWeek;
        if (column <= 0) return;
        var y = (float)((now.TimeOfDay.TotalHours - StartHour) * Metrics.Scale(this, Metrics.CalendarHourHeight));
        using var pen = new Pen(Palette.BrandAccent, Metrics.Scale(this, Metrics.FocusRing));
        graphics.DrawLine(pen, (float)(gutter + day * column), y, (float)(gutter + (day + 1) * column), y);
        using var dot = new SolidBrush(Palette.BrandAccent); var size = Metrics.Scale(this, Metrics.StatusDot);
        graphics.FillEllipse(dot, gutter - Metrics.Scale(this, Space.Sm) - size, y - size / 2f, size, size);
    }
    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        UpdateClock();
    }
    private void UpdateClock() { if (Visible && IsHandleCreated) _nowTimer?.Start(); else _nowTimer?.Stop(); }
    private void AncestorVisibilityChanged(object? sender, EventArgs e) => UpdateClock();
    private void AncestorParentChanged(object? sender, EventArgs e) => WatchVisibility();
    protected override void OnParentChanged(EventArgs e) { base.OnParentChanged(e); WatchVisibility(); }
    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); UpdateClock(); }
    private void WatchVisibility()
    {
        if (_visibilityAncestors is null) return;
        foreach (var ancestor in _visibilityAncestors) { ancestor.VisibleChanged -= AncestorVisibilityChanged; ancestor.ParentChanged -= AncestorParentChanged; }
        _visibilityAncestors.Clear();
        for (var ancestor = Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            ancestor.VisibleChanged += AncestorVisibilityChanged; ancestor.ParentChanged += AncestorParentChanged; _visibilityAncestors.Add(ancestor);
        }
        UpdateClock();
    }
    private Rectangle AnimatedBounds(CalendarBlock block, int index, out float progress)
    {
        var local = Math.Clamp((_revealMilliseconds - Math.Min(index, MotionSystem.MaxStagger - 1) * MotionSystem.StaggerStep) / (float)MotionSystem.Base.TotalMilliseconds, 0, 1);
        progress = Easings.Evaluate(Easing.EaseOutCubic, local) * _weekProgress;
        var bounds = block.Bounds;
        bounds.Offset((int)(_direction * Metrics.Scale(this, Metrics.Slide) * (1 - _weekProgress)), (int)(Metrics.Scale(this, Space.Xs) * (1 - progress)));
        return bounds;
    }
    private void DrawAppointment(Graphics graphics, Entry entry, int index)
    {
        var bounds = AnimatedBounds(entry.Block, index, out var progress);
        var style = entry.Style;
        var radius = Metrics.Scale(this, Metrics.ControlRadius); var gap = Metrics.Scale(this, Space.Xs);
        DesignPaint.Surface(graphics, bounds, radius, Theme.Lerp(Palette.Surface, style.Background, progress),
            Theme.Lerp(Palette.Surface, index == _hovered ? Palette.LineStrong : style.Background, progress));
        using var bar = new SolidBrush(Theme.Lerp(Palette.Surface, style.Text, progress));
        graphics.FillRectangle(bar, bounds.Left, bounds.Top + gap, Metrics.Scale(this, Metrics.CalendarStatusBar), Math.Max(0, bounds.Height - gap * 2));
        var text = Rectangle.Inflate(bounds, -Metrics.Scale(this, Space.Sm), -Metrics.Scale(this, Space.Xs));
        using var label = Typography.PixelFont(Typography.Label, DeviceDpi);
        using var caption = Typography.PixelFont(Typography.Caption, DeviceDpi);
        var firstHeight = TextRenderer.MeasureText("Ag", label).Height; var secondHeight = TextRenderer.MeasureText("Ag", caption).Height;
        var narrow = bounds.Width < Metrics.Scale(this, Metrics.CalendarNarrowBlockWidth);
        var firstLine = text; firstLine.Height = Math.Min(text.Height, firstHeight);
        if (firstLine.Width > 0 && firstLine.Height > 0)
            TextRenderer.DrawText(graphics, narrow ? DisplayFormat.Initials(entry.Patient) : entry.Patient, label, firstLine, Theme.Lerp(Palette.Surface, style.Text, progress), DesignPaint.TextFlags);
        if (!narrow && text.Height >= firstHeight + secondHeight)
        {
            text.Y += firstHeight; text.Height -= firstHeight;
            TextRenderer.DrawText(graphics, entry.Slot, caption, text, Theme.Lerp(Palette.Surface, style.Text, progress), DesignPaint.TextFlags);
        }
        if (Focused && _selected == index)
            DesignPaint.Surface(graphics, Rectangle.Inflate(bounds, Metrics.Scale(this, Metrics.FocusRing), Metrics.Scale(this, Metrics.FocusRing)), radius,
                Palette.WithAlpha(Palette.Surface, 0), Palette.BrandAccent);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); var hit = Hit(e.Location);
        if (hit == _hovered) return;
        _hovered = hit; Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default; InvalidateAppointments();
        Tooltips.Attach(this, hit >= 0 ? _entries[hit].Tooltip : "");
    }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = -1; Cursor = Cursors.Default; InvalidateAppointments(); }
    private int Hit(Point location)
    {
        for (var index = _blocks.Count - 1; index >= 0; index--)
            if (AnimatedBounds(_blocks[index], index, out _).Contains(location)) return index;
        return -1;
    }
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left || !Enabled) return;
        Focus(); _selected = Hit(e.Location);
        if (_selected >= 0) AppointmentActivated?.Invoke(_blocks[_selected].Appointment.AppointmentId);
    }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_blocks.Count == 0 || !Enabled) return;
        if (e.KeyCode is Keys.Left or Keys.Up or Keys.Right or Keys.Down)
        {
            _selected = Math.Clamp(_selected + (e.KeyCode is Keys.Left or Keys.Up ? -1 : 1), 0, _blocks.Count - 1);
            if (Parent is ScrollableControl scroll) scroll.ScrollControlIntoView(this);
            AccessibleDescription = _entries[_selected].Tooltip;
            e.Handled = true; InvalidateAppointments();
        }
        else if (e.KeyCode is Keys.Enter or Keys.Space && _selected >= 0)
        { e.Handled = true; e.SuppressKeyPress = true; AppointmentActivated?.Invoke(_blocks[_selected].Appointment.AppointmentId); }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var ancestor in _visibilityAncestors) { ancestor.VisibleChanged -= AncestorVisibilityChanged; ancestor.ParentChanged -= AncestorParentChanged; }
            _visibilityAncestors.Clear(); _nowTimer.Stop(); _nowTimer.Dispose(); MotionSystem.Animator.Cancel(this);
        }
        base.Dispose(disposing);
    }
}
