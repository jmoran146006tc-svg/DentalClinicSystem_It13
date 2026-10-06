using DentalClinicSystem.Helpers.Charts;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ReportStatusContent : Panel
{
    private readonly DonutChart _chart;
    private readonly ReportLegend _legend;
    public ReportStatusContent(DonutChart chart, ReportLegend legend)
    {
        Name = "reportStatusGroup"; BackColor = Palette.Surface; _chart = chart; _legend = legend;
        chart.Dock = DockStyle.None; legend.Dock = DockStyle.None; Controls.AddRange([chart, legend]);
        Layout += (_, _) => Arrange(); DpiChangedAfterParent += (_, _) => Arrange();
    }
    private void Arrange()
    {
        var gap = Metrics.Scale(this, Space.Sm);
        var card = Parent?.Parent?.Parent;
        var stacked = (card?.Width ?? Width) < Metrics.Scale(this, Metrics.ReportStackWidth);
        var legendSize = _legend.GetPreferredSize(Size.Empty);
        _legend.MaximumSize = new(Metrics.Scale(this, Metrics.FormWidth - Space.Xxxl), 0);
        var square = Math.Max(0, Math.Min(Metrics.Scale(this, Metrics.ChartHeight), stacked
            ? Math.Min(Width, Height - legendSize.Height - gap) : Math.Min(Height, Width - legendSize.Width - gap)));
        var width = stacked ? Math.Max(square, legendSize.Width) : square + gap + legendSize.Width;
        var height = stacked ? square + gap + legendSize.Height : Math.Max(square, legendSize.Height);
        var x = Math.Max(0, (Width - width) / 2); var y = Math.Max(0, (Height - height) / 2);
        _chart.Bounds = new(stacked ? (Width - square) / 2 : x, y + (stacked ? 0 : (height - square) / 2), square, square);
        _legend.Location = new(stacked ? (Width - legendSize.Width) / 2 : x + square + gap,
            stacked ? y + square + gap : y + (height - legendSize.Height) / 2);
    }
}
