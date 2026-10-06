using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WeekCalendarCanvas : DesignControl
{
    private sealed record Entry(CalendarBlock Block, string Patient, string Slot, string Tooltip, SemanticStyle Style, IReadOnlyList<CalendarTextLine> Lines);
    private IReadOnlyList<Appointment> _appointments = [];
    private IReadOnlyList<CalendarBlock> _blocks = [];
    private IReadOnlyList<Entry> _entries = [];
    private CalendarGeometry _geometry = CalendarGeometry.ForWeek([], DateTime.Today);
    private IReadOnlyList<CalendarColumn> _columns = [];
    private CalendarView _view;
    private int _hourHeight = Metrics.CalendarDayHourHeight;
    private bool _rebuilding;
    private string[] HourLabels = [];
    private readonly System.Windows.Forms.Timer _nowTimer = new() { Interval = Metrics.CalendarNowInterval };
    private readonly List<Control> _visibilityAncestors = [];
    private IReadOnlyDictionary<int, string> _patients = new Dictionary<int, string>();
    private IReadOnlyDictionary<int, string> _dentists = new Dictionary<int, string>();
    private DateTime _today = DateTime.Today;
    private float _revealMilliseconds, _weekProgress = 1;
    private int _direction, _selected = -1, _hovered = -1;
    public event Action<int>? AppointmentActivated;
    public IReadOnlyList<CalendarBlock> Blocks => _blocks;
    public int StartHour => _geometry.StartHour;
    public int EndHour => _geometry.EndHour;
    public int HourHeight => _hourHeight;
    public IReadOnlyList<CalendarColumn> Columns => _columns;
    public IReadOnlyList<(CalendarBlock Block, IReadOnlyList<CalendarTextLine> Lines)> BlockContent => _entries.Select(e => (e.Block, e.Lines)).ToArray();
    public WeekCalendarCanvas()
    {
        Name = "weekCalendarCanvas"; TabStop = true; AccessibleRole = AccessibleRole.List;
        AccessibleName = "Calendar appointments. Use arrow keys to choose an appointment and Enter to open it.";
        Size = new(CalendarGeometry.MinimumWidth(DeviceDpi), _geometry.Height(DeviceDpi));
        GotFocus += (_, _) => { if (_selected < 0 && _blocks.Count > 0) _selected = 0; Invalidate(); };
        LostFocus += (_, _) => Invalidate();
        _nowTimer.Tick += (_, _) => Invalidate();
    }
    public void SetSchedule(DateTime monday, DateTime today, CalendarView view, IReadOnlyList<CalendarColumn> columns,
        IReadOnlyList<Appointment> appointments, IReadOnlyDictionary<int, string> patients, IReadOnlyDictionary<int, string> dentists, int direction = 0)
    {
        MotionSystem.Animator.Cancel(this);
        _today = today.Date; _appointments = appointments;
        _patients = patients; _dentists = dentists; _direction = Math.Sign(direction); _selected = -1; _hovered = -1;
        _view = view; _columns = columns;
        _geometry = CalendarGeometry.ForWeek(appointments, monday);
        _hourHeight = Metrics.Scale(this, view == CalendarView.Day ? Metrics.CalendarDayHourHeight : Metrics.CalendarHourHeight);
        Rebuild();
        var total = (float)MotionSystem.Base.TotalMilliseconds + Math.Min(Math.Max(0, _blocks.Count - 1), MotionSystem.MaxStagger - 1) * MotionSystem.StaggerStep;
        MotionSystem.Animator.Run(this, "blocks-reveal", 0, total, TimeSpan.FromMilliseconds(total), Easing.Linear,
            t => { _revealMilliseconds = t; InvalidateAppointments(); });
        MotionSystem.Animator.Run(this, "week-slide", 0, 1, MotionSystem.Base, Easing.EaseOutCubic,
            t => { _weekProgress = Math.Clamp(t, 0, 1); InvalidateAppointments(); });
        Invalidate();
    }
    public void FitHeight(int viewportHeight, bool fullScreen)
    {
        var minimum = Metrics.Scale(this, fullScreen || _view == CalendarView.Day ? Metrics.CalendarDayHourHeight : Metrics.CalendarHourHeight);
        var height = fullScreen ? Math.Max(minimum, viewportHeight / Math.Max(1, EndHour - StartHour)) : minimum;
        if (height == _hourHeight) return;
        _hourHeight = height; Rebuild(); Invalidate();
    }
    private void Rebuild()
    {
        if (_rebuilding) return;
        _rebuilding = true;
        try
        {
            Height = Math.Max(1, (EndHour - StartHour) * _hourHeight);
            HourLabels = Enumerable.Range(StartHour, EndHour - StartHour + 1)
                .Select(hour => DisplayFormat.Time(DateTime.MinValue.AddHours(hour))).ToArray();
            var gutter = Metrics.Scale(this, Metrics.CalendarGutter);
            var blocks = _view == CalendarView.Day ? DayScheduleGeometry.Blocks(_geometry, _appointments, _columns, Width + gutter, DeviceDpi, _hourHeight)
                : _geometry.Blocks(_appointments, _columns.Select(c => c.Date).ToArray(), Width + gutter, DeviceDpi, _hourHeight);
            _blocks = blocks.Select(block => block with { Bounds = new(block.Bounds.X - gutter, block.Bounds.Y, block.Bounds.Width, block.Bounds.Height) }).ToArray();
            using var label = Typography.PixelFont(Typography.Label, DeviceDpi);
            using var caption = Typography.PixelFont(Typography.Caption, DeviceDpi);
            var lineHeight = Math.Max(TextRenderer.MeasureText("Ag", label).Height, TextRenderer.MeasureText("Ag", caption).Height);
            _entries = _blocks.Select(block =>
            {
                var a = block.Appointment;
                var names = $"{_patients.GetValueOrDefault(a.PatientId, $"Patient #{a.PatientId}")} / {_dentists.GetValueOrDefault(a.DentistId, $"Dentist #{a.DentistId}")}";
                var status = AppointmentStatus.Display(a.Status);
                var patient = _patients.GetValueOrDefault(a.PatientId, $"Patient #{a.PatientId}");
                var slot = CalendarBlockText.TimeRange(a);
                return new Entry(block, patient, slot,
                    $"{DisplayFormat.DateTime(a.AppointmentDateTime)} – {DisplayFormat.Time(a.AppointmentDateTime.AddMinutes(a.DurationMinutes))}\n{names}\n{status}\nReason: {DisplayFormat.Optional(a.Reason)}", Theme.StatusStyle(a.Status),
                    CalendarBlockText.Lines(a, patient, block.Bounds, lineHeight, DeviceDpi));
            }).ToArray();
        }
        finally { _rebuilding = false; }
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
        const int gutter = 0;
        var hourHeight = _hourHeight;
        var column = (Width - gutter) / (double)Math.Max(1, _columns.Count);
        if (column <= 0) return;
        using var todayBrush = new SolidBrush(Palette.BrandSoft);
        using var closedBrush = new SolidBrush(Palette.SurfaceAlt);
        using var pen = new Pen(Palette.Line, Metrics.Scale(this, Metrics.Border));
        for (var day = 0; day < _columns.Count; day++)
        {
            var date = _columns[day].Date; var x = gutter + (int)Math.Round(day * column);
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
        for (var hour = StartHour; hour <= EndHour; hour++)
        {
            var y = (hour - StartHour) * hourHeight;
            graphics.DrawLine(pen, gutter, y, Width, y);
        }
    }
    public void DrawTimeGutter(Graphics graphics, int width)
    {
        using var font = Typography.PixelFont(Typography.Caption, DeviceDpi);
        var labelHeight = TextRenderer.MeasureText("Ag", font).Height;
        for (var hour = StartHour; hour <= EndHour; hour++)
            TextRenderer.DrawText(graphics, HourLabels[hour - StartHour], font,
                new Rectangle(Metrics.Scale(this, Space.Xs), Math.Clamp((hour - StartHour) * _hourHeight - labelHeight / 2, 0, Math.Max(0, Height - labelHeight)),
                    width - Metrics.Scale(this, Space.Sm), labelHeight), Palette.Ink500, DesignPaint.TextFlags | TextFormatFlags.PreserveGraphicsTranslateTransform);
    }
    private void DrawNow(Graphics graphics)
    {
        var now = DateTime.Now;
        if (now.TimeOfDay.TotalHours < StartHour || now.TimeOfDay.TotalHours > EndHour) return;
        const int gutter = 0; var column = Width / (double)Math.Max(1, _columns.Count);
        if (column <= 0) return;
        var y = (float)((now.TimeOfDay.TotalHours - StartHour) * _hourHeight);
        using var pen = new Pen(Palette.BrandAccent, Metrics.Scale(this, Metrics.FocusRing));
        for (var day = 0; day < _columns.Count; day++)
            if (_columns[day].Date == now.Date) graphics.DrawLine(pen, (float)(gutter + day * column), y, (float)(gutter + (day + 1) * column), y);
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
        using var label = Typography.PixelFont(Typography.Label, DeviceDpi);
        using var caption = Typography.PixelFont(Typography.Caption, DeviceDpi);
        var state = graphics.Save();
        graphics.SetClip(bounds, System.Drawing.Drawing2D.CombineMode.Intersect);
        foreach (var line in entry.Lines)
        {
            var text = line.Bounds; text.Offset(bounds.X - entry.Block.Bounds.X, bounds.Y - entry.Block.Bounds.Y);
            if (text.Width > 0 && text.Height > 0) TextRenderer.DrawText(graphics, line.Text, line.Bold ? label : caption, text,
                Theme.Lerp(Palette.Surface, style.Text, progress), DesignPaint.TextFlags | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping);
        }
        graphics.Restore(state);
        if (Focused && _selected == index)
            DesignPaint.Surface(graphics, bounds, radius,
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
            MoveSelection(e.KeyCode);
            if (Parent is ScrollableControl scroll)
            {
                var block = _blocks[_selected].Bounds;
                var x = -scroll.AutoScrollPosition.X; var y = -scroll.AutoScrollPosition.Y;
                if (block.Left < x) x = block.Left;
                else if (block.Right > x + scroll.ClientSize.Width) x = block.Right - scroll.ClientSize.Width;
                if (block.Top < y) y = block.Top; else if (block.Bottom > y + scroll.ClientSize.Height) y = block.Bottom - scroll.ClientSize.Height;
                scroll.AutoScrollPosition = new(x, y);
            }
            AccessibleDescription = _entries[_selected].Tooltip;
            e.Handled = true; InvalidateAppointments();
        }
        else if (e.KeyCode is Keys.Enter or Keys.Space && _selected >= 0)
        { e.Handled = true; e.SuppressKeyPress = true; AppointmentActivated?.Invoke(_blocks[_selected].Appointment.AppointmentId); }
    }
    private void MoveSelection(Keys key)
    {
        if (_selected < 0) { _selected = 0; return; }
        var current = _blocks[_selected].Appointment;
        int ColumnOf(Appointment a) => _columns.ToList().FindIndex(c => c.Date == a.AppointmentDateTime.Date && (_view == CalendarView.Week || c.DentistId == a.DentistId));
        var column = ColumnOf(current);
        var candidates = _blocks.Select((b, i) => (Appointment: b.Appointment, Index: i));
        if (key is Keys.Up or Keys.Down)
        {
            var ordered = candidates.Where(c => ColumnOf(c.Appointment) == column).OrderBy(c => c.Appointment.AppointmentDateTime).ThenBy(c => c.Appointment.AppointmentId).ToList();
            var position = ordered.FindIndex(c => c.Index == _selected);
            _selected = ordered[Math.Clamp(position + (key == Keys.Up ? -1 : 1), 0, ordered.Count - 1)].Index;
        }
        else
        {
            var step = key == Keys.Left ? -1 : 1;
            for (var target = column + step; target >= 0 && target < _columns.Count; target += step)
            {
                var nearest = candidates.Where(c => ColumnOf(c.Appointment) == target)
                    .OrderBy(c => Math.Abs((c.Appointment.AppointmentDateTime.TimeOfDay - current.AppointmentDateTime.TimeOfDay).TotalMinutes)).ThenBy(c => c.Appointment.AppointmentId).ToArray();
                if (nearest.Length == 0) continue;
                _selected = nearest[0].Index; break;
            }
        }
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
