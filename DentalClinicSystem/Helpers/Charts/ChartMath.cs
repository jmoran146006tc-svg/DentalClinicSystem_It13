namespace DentalClinicSystem.Helpers.Charts;

public sealed record ChartDatum(string Label, double Value, Color Color);
public sealed record ChartTicks(IReadOnlyList<double> Values, double Max);
public readonly record struct ChartSweep(double Start, double Sweep);

public static class ChartMath
{
    public static double NonNegative(double value) => double.IsFinite(value) ? Math.Max(0, value) : 0;

    public static ChartTicks NiceTicks(double min, double max, int desiredCount = 5)
    {
        min = NonNegative(min); max = Math.Max(min, NonNegative(max));
        if (max == 0) return new([0], 0);
        var count = Math.Clamp(desiredCount, 2, 20);
        var raw = (max - min) / (count - 1);
        if (raw <= 0) raw = max / (count - 1);
        var power = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        if (power == 0) power = double.Epsilon;
        var fraction = raw / power;
        var step = (fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10) * power;
        if (!double.IsFinite(step)) step = raw;
        var rounded = Math.Ceiling(max / step) * step;
        if (!double.IsFinite(rounded)) rounded = max;
        List<double> ticks = [];
        var first = Math.Floor(min / step);
        for (var i = 0; i <= count * 2; i++)
        {
            var tick = (first + i) * step;
            if (!double.IsFinite(tick) || tick > rounded) break;
            ticks.Add(tick);
        }
        return new(ticks, rounded);
    }

    public static double Scale(double value, (double Min, double Max) domain, (double Min, double Max) range)
    {
        if (!double.IsFinite(value) || domain.Max <= domain.Min || !double.IsFinite(domain.Max - domain.Min)) return range.Min;
        var progress = Math.Clamp((value - domain.Min) / (domain.Max - domain.Min), 0, 1);
        return range.Min * (1 - progress) + range.Max * progress;
    }

    public static IReadOnlyList<ChartSweep> SweepAngles(IEnumerable<double> values)
    {
        var rows = values.Select(NonNegative).ToArray();
        var max = rows.DefaultIfEmpty().Max();
        if (max == 0) return [];
        var total = rows.Sum(value => value / max);
        var start = 0d;
        List<ChartSweep> result = [];
        foreach (var value in rows)
        {
            var sweep = value / max / total * 360;
            result.Add(new(start, sweep)); start += sweep;
        }
        return result;
    }

    public static IReadOnlyList<double> BarLengths(IEnumerable<double> values, double maximum, double width) =>
        values.Select(value => Scale(NonNegative(value), (0, NonNegative(maximum)), (0, NonNegative(width)))).ToArray();
}
