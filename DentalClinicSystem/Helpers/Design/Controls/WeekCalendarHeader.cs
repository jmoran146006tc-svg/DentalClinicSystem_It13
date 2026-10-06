using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WeekCalendarHeader : DesignControl
{
    private IReadOnlyList<CalendarColumn> _columns = [];
    private DateTime _today;
    private CalendarView _view;
    private int _gutter;
    public WeekCalendarHeader() { Name = "weekCalendarHeader"; TabStop = false; }
    public void SetColumns(IReadOnlyList<CalendarColumn> columns, DateTime today, CalendarView view, int gutter)
    { _columns = columns; _today = today.Date; _view = view; _gutter = gutter; Invalidate(); }
    public void SetGutter(int gutter) { _gutter = gutter; Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= _gutter || Height <= 0 || _columns.Count == 0) return;
        DesignPaint.Begin(e.Graphics, this); e.Graphics.Clear(Palette.Surface);
        var gap = Metrics.Scale(this, Space.Xs);
        var column = (Width - _gutter) / (double)_columns.Count;
        if (column <= 0) return;
        using var font = Typography.PixelFont(Typography.Label, DeviceDpi);
        using var caption = Typography.PixelFont(Typography.Caption, DeviceDpi);
        using var pen = new Pen(Palette.Line, Metrics.Scale(this, Metrics.Border));
        for (var index = 0; index < _columns.Count; index++)
        {
            var item = _columns[index]; var left = _gutter + (int)Math.Round(index * column);
            var right = _gutter + (int)Math.Round((index + 1) * column);
            var bounds = new Rectangle(left + gap, gap, Math.Max(0, right - left - gap * 2), Math.Max(0, Height - gap * 2));
            if (_view == CalendarView.Week && item.Date == _today)
                DesignPaint.Surface(e.Graphics, bounds, Metrics.Scale(this, Metrics.ControlRadius), Palette.BrandSoft, Palette.BrandSoft);
            var secondary = _view == CalendarView.Day ? $"{item.Count} appointment{(item.Count == 1 ? "" : "s")}" :
                ClinicRules.ClosedDays.Contains(item.Date.DayOfWeek) ? "Closed · booked visits" : "";
            var first = bounds; if (secondary.Length > 0) first.Height = bounds.Height / 2;
            TextRenderer.DrawText(e.Graphics, item.Title, font, first, Palette.Ink700, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix);
            if (secondary.Length > 0) TextRenderer.DrawText(e.Graphics, secondary, caption,
                new Rectangle(bounds.Left, first.Bottom, bounds.Width, bounds.Bottom - first.Bottom), Palette.Ink500, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
            e.Graphics.DrawLine(pen, left, 0, left, Height);
        }
        e.Graphics.DrawLine(pen, 0, Height - Metrics.Border, Width, Height - Metrics.Border);
    }
}
