using System.Globalization;
using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Models;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WeekCalendarCanvas : DesignControl
{
    private sealed record Entry(CalendarBlock Block, string Label, string Status, string Tooltip, SemanticStyle Style);
    private IReadOnlyList<Appointment> _appointments = [];
    private IReadOnlyList<CalendarBlock> _blocks = [];
    private IReadOnlyList<Entry> _entries = [];
    private string[] _dayHeaders = [];
    private static readonly string[] HourLabels = Enumerable.Range(CalendarGeometry.StartHour, CalendarGeometry.EndHour - CalendarGeometry.StartHour + 1)
        .Select(hour => DisplayFormat.Time(DateTime.MinValue.AddHours(hour))).ToArray();
    private IReadOnlyDictionary<int, string> _patients = new Dictionary<int, string>();
    private IReadOnlyDictionary<int, string> _dentists = new Dictionary<int, string>();
    private DateTime _monday = DashboardPresentation.WeekStart(DateTime.Today);
    private DateTime _today = DateTime.Today;
    private float _revealMilliseconds, _weekProgress = 1;
    private int _direction, _selected = -1, _hovered = -1;
    public event Action<int>? AppointmentActivated;
    public IReadOnlyList<CalendarBlock> Blocks => _blocks;
    public WeekCalendarCanvas()
    {
        Name = "weekCalendarCanvas"; TabStop = true; AccessibleRole = AccessibleRole.List;
        AccessibleName = "Week appointments. Use arrow keys to choose an appointment and Enter to open it.";
        Size = new(CalendarGeometry.MinimumWidth(DeviceDpi), CalendarGeometry.Height(DeviceDpi));
        GotFocus += (_, _) => { if (_selected < 0 && _blocks.Count > 0) _selected = 0; Invalidate(); };
        LostFocus += (_, _) => Invalidate();
    }
    public void SetAppointments(DateTime monday, DateTime today, IReadOnlyList<Appointment> appointments,
        IReadOnlyDictionary<int, string> patients, IReadOnlyDictionary<int, string> dentists, int direction = 0)
    {
        MotionSystem.Animator.Cancel(this);
        _monday = monday.Date; _today = today.Date; _appointments = appointments;
        _patients = patients; _dentists = dentists; _direction = Math.Sign(direction); _selected = -1; _hovered = -1;
        _dayHeaders = Enumerable.Range(0, DashboardPresentation.DaysInWeek).Select(day => _monday.AddDays(day).ToString("ddd  MMM d", CultureInfo.InvariantCulture)).ToArray();
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
        Height = CalendarGeometry.Height(DeviceDpi);
        _blocks = CalendarGeometry.Blocks(_appointments, _monday, Width, DeviceDpi);
        _entries = _blocks.Select(block =>
        {
            var a = block.Appointment;
            var names = $"{_patients.GetValueOrDefault(a.PatientId, $"Patient #{a.PatientId}")} / {_dentists.GetValueOrDefault(a.DentistId, $"Dentist #{a.DentistId}")}";
            var status = AppointmentStatus.Display(a.Status);
            return new Entry(block, $"{DisplayFormat.Time(a.AppointmentDateTime)}  {names}", status,
                $"{DisplayFormat.DateTime(a.AppointmentDateTime)} – {DisplayFormat.Time(a.AppointmentDateTime.AddMinutes(a.DurationMinutes))}\n{names}\n{status}\nReason: {DisplayFormat.Optional(a.Reason)}", Theme.StatusStyle(a.Status));
        }).ToArray();
    }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Rebuild(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Rebuild(); }
    private void InvalidateAppointments()
    {
        var top = Metrics.Scale(this, Metrics.CalendarHeaderHeight);
        Invalidate(new Rectangle(0, top, Width, Math.Max(0, Height - top)));
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this); e.Graphics.Clear(Palette.Surface);
        DrawGrid(e.Graphics);
        for (var index = 0; index < _entries.Count; index++) DrawAppointment(e.Graphics, _entries[index], index);
    }
    private void DrawGrid(Graphics graphics)
    {
        var gutter = Metrics.Scale(this, Metrics.CalendarGutter);
        var header = Metrics.Scale(this, Metrics.CalendarHeaderHeight);
        var hourHeight = Metrics.Scale(this, Metrics.CalendarHourHeight);
        var column = (Width - gutter) / (double)DashboardPresentation.DaysInWeek;
        if (column <= 0) return;
        using var todayBrush = new SolidBrush(Palette.BrandSoft);
        using var pen = new Pen(Palette.Line, Metrics.Border);
        for (var day = 0; day < DashboardPresentation.DaysInWeek; day++)
        {
            var date = _monday.AddDays(day); var x = gutter + (int)Math.Round(day * column);
            var right = gutter + (int)Math.Round((day + 1) * column);
            if (date == _today) graphics.FillRectangle(todayBrush, x, 0, right - x, Height);
            graphics.DrawLine(pen, x, 0, x, Height);
            TextRenderer.DrawText(graphics, _dayHeaders.ElementAtOrDefault(day) ?? "", Typography.Caption,
                new Rectangle(x + Space.Xs, 0, right - x - Space.Sm, header), Palette.Ink700, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
        }
        for (var hour = CalendarGeometry.StartHour; hour <= CalendarGeometry.EndHour; hour++)
        {
            var y = header + (hour - CalendarGeometry.StartHour) * hourHeight;
            graphics.DrawLine(pen, gutter, y, Width, y);
            TextRenderer.DrawText(graphics, HourLabels[hour - CalendarGeometry.StartHour], Typography.Caption,
                new Rectangle(Space.Xs, y - Typography.Caption.Height / 2, gutter - Space.Sm, Typography.Caption.Height), Palette.Ink500, DesignPaint.TextFlags);
        }
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
        DesignPaint.Surface(graphics, bounds, Metrics.ControlRadius, Theme.Lerp(Palette.Surface, style.Background, progress),
            Theme.Lerp(Palette.Surface, index == _hovered ? Palette.LineStrong : style.Background, progress));
        using var bar = new SolidBrush(Theme.Lerp(Palette.Surface, style.Text, progress));
        graphics.FillRectangle(bar, bounds.Left, bounds.Top + Space.Xs, Metrics.Scale(this, Metrics.CalendarStatusBar), Math.Max(0, bounds.Height - Space.Sm));
        var text = Rectangle.Inflate(bounds, -Space.Sm, -Space.Xs);
        var firstLine = text; firstLine.Height = Math.Min(text.Height, Typography.Caption.Height);
        if (firstLine.Width > 0 && firstLine.Height > 0)
            TextRenderer.DrawText(graphics, entry.Label, Typography.Caption, firstLine, Theme.Lerp(Palette.Surface, style.Text, progress), DesignPaint.TextFlags);
        if (text.Height >= Typography.Caption.Height * 2)
        {
            text.Y += Typography.Caption.Height; text.Height -= Typography.Caption.Height;
            TextRenderer.DrawText(graphics, entry.Status, Typography.Caption, text, Theme.Lerp(Palette.Surface, style.Text, progress), DesignPaint.TextFlags);
        }
        if (Focused && _selected == index)
            DesignPaint.Surface(graphics, Rectangle.Inflate(bounds, Metrics.FocusRing, Metrics.FocusRing), Metrics.ControlRadius,
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
}
