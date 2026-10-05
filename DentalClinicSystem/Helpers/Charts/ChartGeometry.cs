using DentalClinicSystem.Helpers.Design;
namespace DentalClinicSystem.Helpers.Charts;

internal static class ChartGeometry
{
    public static Rectangle Plot(Size size, int left, int bottom) => new(left, Space.Sm, Math.Max(0, size.Width - left - Space.Sm), Math.Max(0, size.Height - bottom - Space.Sm));
    public static Rectangle Bar(Rectangle plot, int index, int count, double length)
    {
        var height = plot.Height / Math.Max(1, count);
        return new(plot.Left, plot.Top + index * height + Space.Xs, (int)Math.Clamp(length, 0, plot.Width), Math.Max(0, height - Space.Sm));
    }
    public static PointF[] Points(Rectangle plot, IReadOnlyList<double> values, double maximum) => values.Select((value, i) =>
        new PointF(values.Count == 1 ? plot.Left + plot.Width / 2f : plot.Left + (float)i / (values.Count - 1) * plot.Width,
            (float)ChartMath.Scale(value, (0, maximum), (plot.Bottom, plot.Top)))).ToArray();
    public static Rectangle Donut(Size size)
    {
        var diameter = Math.Max(0, Math.Min(size.Width, size.Height) - Space.Lg);
        return new((size.Width - diameter) / 2, (size.Height - diameter) / 2, diameter, diameter);
    }
    public static int Segment(Point point, Rectangle ring, IReadOnlyList<ChartSweep> sweeps)
    {
        var dx = point.X - (ring.Left + ring.Width / 2d); var dy = point.Y - (ring.Top + ring.Height / 2d);
        var radius = ring.Width / 2d; var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance > radius || distance < radius * Metrics.DonutHoleRatio) return -1;
        var angle = (Math.Atan2(dy, dx) * 180 / Math.PI + 450) % 360;
        for (var i = 0; i < sweeps.Count; i++) if (angle >= sweeps[i].Start && angle < sweeps[i].Start + sweeps[i].Sweep) return i;
        return -1;
    }
}
