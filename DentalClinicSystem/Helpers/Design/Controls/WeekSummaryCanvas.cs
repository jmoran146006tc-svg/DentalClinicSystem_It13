using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WeekSummaryCanvas : DesignControl
{
    private WeekSummary _summary = new([], [], []);
    private DateTime _today;
    private int _selected = -1, _hovered = -1;
    public event Action<DateTime>? DayActivated;
    public int LabelWidth => Math.Min(Metrics.Scale(this, Metrics.CalendarDentistWidth), Math.Max(0, Width / 4));
    public IReadOnlyList<(WeekSummaryCell Cell, Rectangle Bounds)> Cells => _summary.Cells.Select((cell, index) => (cell, BoundsFor(index))).ToArray();
    public WeekSummaryCanvas()
    {
        Name = "weekSummaryCanvas"; TabStop = true; AccessibleRole = AccessibleRole.Table;
        AccessibleName = "Weekly appointment counts. Use arrow keys to choose a day and Enter to view its appointments.";
        GotFocus += (_, _) => { if (_selected < 0 && _summary.Cells.Count > 0) _selected = 0; Invalidate(); };
        LostFocus += (_, _) => Invalidate();
    }
    public void SetSummary(WeekSummary summary, DateTime today)
    {
        _summary = summary; _today = today.Date; _selected = _hovered = -1;
        Height = Math.Max(1, summary.Dentists.Count) * Metrics.Scale(this, Metrics.CalendarSummaryRowHeight); Invalidate();
    }
    private Rectangle BoundsFor(int index)
    {
        var count = _summary.Days.Count; if (count == 0) return Rectangle.Empty;
        var width = (Width - LabelWidth) / (double)count;
        var left = LabelWidth + (int)Math.Round(index % count * width);
        var right = LabelWidth + (int)Math.Round((index % count + 1) * width);
        return new(left, index / count * Metrics.Scale(this, Metrics.CalendarSummaryRowHeight), Math.Max(0, right - left), Metrics.Scale(this, Metrics.CalendarSummaryRowHeight));
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Palette.Surface); DesignPaint.Prepare(e.Graphics);
        using var font = Typography.PixelFont(Typography.Label, DeviceDpi);
        using var caption = Typography.PixelFont(Typography.Caption, DeviceDpi);
        using var pen = new Pen(Palette.Line);
        var rowHeight = Metrics.Scale(this, Metrics.CalendarSummaryRowHeight); var gap = Metrics.Scale(this, Space.Sm);
        for (var row = 0; row < _summary.Dentists.Count; row++)
        {
            var text = new Rectangle(gap, row * rowHeight, Math.Max(0, LabelWidth - gap * 2), rowHeight);
            TextRenderer.DrawText(e.Graphics, _summary.Dentists[row].Title, font, text, Palette.Ink700, DesignPaint.TextFlags | TextFormatFlags.NoPrefix);
            e.Graphics.DrawLine(pen, 0, text.Bottom - 1, Width, text.Bottom - 1);
        }
        for (var index = 0; index < _summary.Cells.Count; index++)
        {
            var cell = _summary.Cells[index]; var bounds = BoundsFor(index);
            using var fill = new SolidBrush(cell.Date == _today ? Palette.BrandSoft : cell.Count == 0 ? Palette.SurfaceAlt : Palette.Surface);
            e.Graphics.FillRectangle(fill, bounds); e.Graphics.DrawRectangle(pen, bounds);
            var text = Rectangle.Inflate(bounds, -gap, -gap);
            text.Height = Math.Max(0, text.Height - Metrics.Scale(this, Space.Sm));
            TextRenderer.DrawText(e.Graphics, cell.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), cell.Count == 0 ? caption : font, text,
                cell.Count == 0 ? Palette.Ink400 : Palette.Ink900, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
            if (cell.Count > 0)
            {
                var bar = new Rectangle(bounds.Left + gap, bounds.Bottom - gap - Metrics.Scale(this, Metrics.CalendarStatusBar), Math.Max(0, bounds.Width - gap * 2), Metrics.Scale(this, Metrics.CalendarStatusBar));
                double offset = 0;
                foreach (var status in AppointmentStatus.All.Concat(cell.StatusCounts.Keys).Distinct())
                {
                    var length = cell.StatusCounts.GetValueOrDefault(status) * bar.Width / (double)cell.Count;
                    using var brush = new SolidBrush(Theme.StatusStyle(status).Text);
                    e.Graphics.FillRectangle(brush, bar.Left + (int)Math.Round(offset), bar.Top, (int)Math.Round(offset + length) - (int)Math.Round(offset), bar.Height);
                    offset += length;
                }
            }
            if (index == _hovered || Focused && index == _selected)
                DesignPaint.Surface(e.Graphics, Rectangle.Inflate(bounds, -1, -1), Metrics.Scale(this, Metrics.ControlRadius), Palette.Transparent, Palette.Brand);
        }
    }
    private int Hit(Point point) => Enumerable.Range(0, _summary.Cells.Count).FirstOrDefault(i => BoundsFor(i).Contains(point), -1);
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); var hit = Hit(e.Location); if (hit == _hovered) return;
        _hovered = hit; Cursor = hit >= 0 ? Cursors.Hand : Cursors.Default;
        Tooltips.Attach(this, hit < 0 ? "" : Description(hit)); Invalidate();
    }
    private string Description(int index)
    {
        var cell = _summary.Cells[index];
        return $"{_summary.Dentists[index / _summary.Days.Count].Title} · {DisplayFormat.Date(cell.Date)}\n{cell.Count} appointments\n" +
            string.Join("\n", cell.StatusCounts.Select(pair => $"{AppointmentStatus.Display(pair.Key)}: {pair.Value}"));
    }
    protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hovered = -1; Invalidate(); }
    protected override void OnMouseDown(MouseEventArgs e)
    { base.OnMouseDown(e); if (e.Button != MouseButtons.Left || !Enabled) return; Focus(); _selected = Hit(e.Location); Activate(); }
    private void Activate() { if (_selected >= 0) DayActivated?.Invoke(_summary.Cells[_selected].Date); }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e); if (!Enabled || _summary.Cells.Count == 0) return;
        if (e.KeyCode is Keys.Enter or Keys.Space) { Activate(); e.Handled = e.SuppressKeyPress = true; return; }
        var delta = e.KeyCode switch { Keys.Left => -1, Keys.Right => 1, Keys.Up => -_summary.Days.Count, Keys.Down => _summary.Days.Count, _ => 0 };
        if (delta == 0) return;
        _selected = Math.Clamp(Math.Max(0, _selected) + delta, 0, _summary.Cells.Count - 1);
        AccessibleDescription = Description(_selected); e.Handled = true; Invalidate();
    }
}
