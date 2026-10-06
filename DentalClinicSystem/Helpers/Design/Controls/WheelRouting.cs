namespace DentalClinicSystem.Helpers.Design.Controls;

internal static class WheelRouting
{
    // Forward once and mark the native message handled to avoid a second scroll
    // from Control.WmMouseWheel. At a boundary, continue to the outer page.
    public static void Forward(Control source, MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs pending) pending.Handled = false;
        for (var parent = source.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is not ScrollableControl { AutoScroll: true } scroll) continue;
            var maximum = Math.Max(0, scroll.DisplayRectangle.Height - scroll.ClientSize.Height);
            var position = -scroll.AutoScrollPosition.Y;
            if (maximum == 0 || (e.Delta > 0 ? position <= 0 : position >= maximum)) continue;
            var lines = SystemInformation.MouseWheelScrollLines;
            var step = lines < 0 ? scroll.ClientSize.Height : Math.Max(1, lines) * scroll.Font.Height;
            var destination = Math.Clamp(position - e.Delta * step / SystemInformation.MouseWheelScrollDelta, 0, maximum);
            scroll.AutoScrollPosition = new(-scroll.AutoScrollPosition.X, destination);
            scroll.Refresh();
            if (e is HandledMouseEventArgs handled) handled.Handled = true;
            return;
        }
    }
}
