using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ReportLegend : TableLayoutPanel
{
    private readonly List<Label> _counts = [], _percentages = [];
    public ReportLegend()
    {
        Name = "reportLegend"; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink; ColumnCount = 4; RowCount = AppointmentStatus.All.Length;
        MaximumSize = new(Metrics.FormWidth - Space.Xxxl, 0);
        BackColor = Palette.Surface; Margin = Padding.Empty;
        ColumnStyles.Add(new(SizeType.Absolute, Metrics.IconSize)); ColumnStyles.Add(new(SizeType.AutoSize));
        ColumnStyles.Add(new(SizeType.Absolute, Metrics.ReportLegendNumberWidth)); ColumnStyles.Add(new(SizeType.Absolute, Metrics.ReportLegendNumberWidth));
        foreach (var status in AppointmentStatus.All)
        {
            var row = _counts.Count; RowStyles.Add(new(SizeType.Absolute, Metrics.ControlHeight));
            var color = Theme.StatusStyle(status).Text;
            var dot = new Panel { BackColor = color, Size = new(Metrics.StatusDot, Metrics.StatusDot), Anchor = AnchorStyles.None };
            var label = Cell(AppointmentStatus.Display(status), "reportLabel" + status, ContentAlignment.MiddleLeft);
            var count = Cell("0", "reportCount" + status, ContentAlignment.MiddleRight);
            var percent = Cell("0%", "reportPercent" + status, ContentAlignment.MiddleRight); percent.ForeColor = Palette.Ink500;
            _counts.Add(count); _percentages.Add(percent);
            Controls.Add(dot, 0, row); Controls.Add(label, 1, row); Controls.Add(count, 2, row); Controls.Add(percent, 3, row);
        }
        SizeChanged += (_, _) => CenterRows(); DpiChangedAfterParent += (_, _) => CenterRows();
    }
    private void CenterRows()
    {
        var rowHeight = Metrics.Scale(this, Metrics.ControlHeight);
        foreach (RowStyle row in RowStyles) row.Height = rowHeight;
        var spare = Math.Max(0, ClientSize.Height - rowHeight * RowCount);
        Padding = new(0, spare / 2, 0, spare - spare / 2);
    }
    private static Label Cell(string text, string name, ContentAlignment align) => new()
    {
        Name = name, Text = text, AutoSize = true, Dock = DockStyle.Fill, Font = Typography.Caption,
        ForeColor = Palette.Ink700, TextAlign = align, Margin = Padding.Empty, AutoEllipsis = true
    };
    public void SetValues(IReadOnlyList<double> values)
    {
        var total = values.Sum();
        for (var i = 0; i < _counts.Count; i++)
        {
            var value = i < values.Count ? values[i] : 0;
            _counts[i].Text = value.ToString("N0"); _counts[i].ForeColor = value == 0 ? Palette.Ink500 : Palette.Ink900;
            _percentages[i].Text = total == 0 ? "0%" : (value / total).ToString("P0");
        }
    }
}
