namespace DentalClinicSystem.Helpers.Design.Controls;

// A sibling of the scrolling viewport, so horizontal scrolling cannot move it.
public sealed class CalendarTimeGutter : DesignControl
{
    public WeekCalendarCanvas Canvas { get; }
    public CalendarTimeGutter(WeekCalendarCanvas canvas) { Canvas = canvas; Name = "calendarTimeGutter"; }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Palette.Surface);
        var state = e.Graphics.Save(); e.Graphics.TranslateTransform(0, Canvas.Top);
        Canvas.DrawTimeGutter(e.Graphics, Width); e.Graphics.Restore(state);
        using var pen = new Pen(Palette.Line);
        e.Graphics.DrawLine(pen, Width - 1, 0, Width - 1, Height);
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if (Canvas.Parent is ScrollableControl scroll && scroll.VerticalScroll.Visible)
        {
            var position = -scroll.AutoScrollPosition.Y;
            var maximum = Math.Max(0, scroll.DisplayRectangle.Height - scroll.ClientSize.Height);
            if (e.Delta > 0 ? position > 0 : position < maximum)
            {
                scroll.AutoScrollPosition = new(-scroll.AutoScrollPosition.X, Math.Clamp(position - Math.Sign(e.Delta) * Font.Height * 3, 0, maximum));
                if (e is HandledMouseEventArgs handled) handled.Handled = true;
                return;
            }
        }
        WheelRouting.Forward(this, e);
    }
}
