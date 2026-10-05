using System.Globalization;
using System.Text;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.UiTests;

public sealed class ChartAndCsvTests
{
    [Theory]
    [InlineData(0)] [InlineData(.000003)] [InlineData(83)] [InlineData(1e25)] [InlineData(1e300)] [InlineData(double.MaxValue)]
    public void TicksAreFiniteOrderedAndCoverDomain(double max)
    {
        var ticks = ChartMath.NiceTicks(0, max); Assert.True(double.IsFinite(ticks.Max)); Assert.True(ticks.Max >= max);
        Assert.All(ticks.Values, tick => Assert.True(double.IsFinite(tick)));
        Assert.Equal(ticks.Values.Order().ToArray(), ticks.Values); Assert.InRange(ticks.Values.Count, 1, 20);
    }
    [Fact]
    public void SweepsAndScalingGuardEmptyZeroAndOverflow()
    {
        Assert.Empty(ChartMath.SweepAngles([0, 0])); Assert.Empty(ChartMath.SweepAngles([]));
        Assert.Equal(360, ChartMath.SweepAngles([1, 3, 6]).Sum(segment => segment.Sweep), 8);
        Assert.Equal(360, ChartMath.SweepAngles([double.MaxValue, double.MaxValue]).Sum(segment => segment.Sweep), 8);
        Assert.Equal(10, ChartMath.Scale(5, (0, 0), (10, 20)));
        Assert.Equal(20, ChartMath.Scale(5, (0, 10), (20, 20)));
        Assert.Equal(new double[] { 0, 50, 100 }, ChartMath.BarLengths([-1, 5, 20], 10, 100));
    }
    [Fact]
    public void CsvQuotesAndNeutralizesTextWithoutChangingInvariantNumbers()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var bytes = CsvExporter.Export([new("Report", ["Name", "Billed (PHP)"], [["a,b\"c\nline", 1234.50m], ["=SUM(A1)", -12m], ["+text", "-text", "@text", new DateTime(2026, 10, 5)]])]);
            Assert.Equal(new byte[] { 0xef, 0xbb, 0xbf }, bytes.Take(3));
            var text = Encoding.UTF8.GetString(bytes.AsSpan(3));
            Assert.Contains("\"a,b\"\"c\nline\",1234.50\r\n", text);
            Assert.Contains("'=SUM(A1),-12\r\n", text); Assert.Contains("'+text,'-text,'@text,2026-10-05\r\n", text);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(25)]
    public void ChartsPaintEmptySingleAndManyDataAtEveryProgress(int count) => UiThread.Run(() =>
    {
        using var bar = new BarChart(); using var line = new LineChart(); using var donut = new DonutChart();
        var data = Enumerable.Range(0, count).Select(i => new ChartDatum("Item " + i, i == 0 ? 1e300 : i, Palette.Brand)).ToArray();
        bar.SetData(data, value => value.ToString("G3"), "Empty"); line.SetData(data, value => value.ToString("G3"), "Empty"); donut.SetData(data, value => value.ToString("G3"), "Empty");
        foreach (var progress in new[] { 0f, .5f, 1f })
        {
            bar.SetAnimationProgress(progress); line.SetAnimationProgress(progress); donut.SetAnimationProgress(progress);
            foreach (var chart in new DesignControl[] { bar, line, donut })
            {
                chart.Size = Size.Empty; Paint(chart); chart.Size = new(600, 400); Paint(chart);
                Assert.False(string.IsNullOrEmpty(chart.AccessibleName)); Assert.False(string.IsNullOrEmpty(chart.AccessibleDescription));
            }
        }
    });
    private static void Paint(Control chart)
    {
        using var bitmap = new Bitmap(Math.Max(1, chart.Width), Math.Max(1, chart.Height));
        using var graphics = Graphics.FromImage(bitmap);
        using var args = new PaintEventArgs(graphics, new Rectangle(Point.Empty, bitmap.Size));
        typeof(Control).GetMethod("OnPaint", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(chart, [args]);
    }
}
