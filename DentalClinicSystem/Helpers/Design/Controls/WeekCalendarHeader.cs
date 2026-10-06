using System.Globalization;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class WeekCalendarHeader : DesignControl
{
    private DateTime _week, _today;
    public WeekCalendarHeader() { Name = "weekCalendarHeader"; TabStop = false; }
    public void SetWeek(DateTime week, DateTime today) { _week = week.Date; _today = today.Date; Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this); e.Graphics.Clear(Palette.Surface);
        var gutter = Metrics.Scale(this, Metrics.CalendarGutter); var gap = Metrics.Scale(this, Space.Xs);
        var column = (Width - gutter) / (double)DashboardPresentation.DaysInWeek;
        if (column <= 0) return;
        using var font = Typography.PixelFont(Typography.Label, DeviceDpi);
        using var caption = Typography.PixelFont(Typography.Caption, DeviceDpi);
        using var pen = new Pen(Palette.Line, Metrics.Scale(this, Metrics.Border));
        for (var day = 0; day < DashboardPresentation.DaysInWeek; day++)
        {
            var date = _week.AddDays(day); var left = gutter + (int)Math.Round(day * column);
            var right = gutter + (int)Math.Round((day + 1) * column); var closed = ClinicRules.ClosedDays.Contains(date.DayOfWeek);
            var label = date.ToString("ddd  MMM d", CultureInfo.InvariantCulture);
            var textHeight = TextRenderer.MeasureText(label, font).Height;
            var bounds = new Rectangle(left + gap, closed ? gap : (Height - textHeight - gap * 2) / 2, right - left - gap * 2, textHeight + gap * 2);
            if (date == _today) DesignPaint.Surface(e.Graphics, bounds, Metrics.Scale(this, Metrics.ControlRadius), Palette.BrandSoft, Palette.BrandSoft);
            TextRenderer.DrawText(e.Graphics, label, font, bounds, date == _today ? Palette.BrandSoftText : Palette.Ink700,
                DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
            if (closed) TextRenderer.DrawText(e.Graphics, "Closed", caption,
                new Rectangle(left, bounds.Bottom, right - left, Math.Max(0, Height - bounds.Bottom)), Palette.Ink500,
                DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
            e.Graphics.DrawLine(pen, left, 0, left, Height);
        }
        e.Graphics.DrawLine(pen, 0, Height - Metrics.Border, Width, Height - Metrics.Border);
    }
}
