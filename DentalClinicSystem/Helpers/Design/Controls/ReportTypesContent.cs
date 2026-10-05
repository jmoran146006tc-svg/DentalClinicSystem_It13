using DentalClinicSystem.Helpers.Charts;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ReportTypesContent : TableLayoutPanel
{
    private readonly BarChart _chart;
    private readonly ClinicTable _table;
    private IReadOnlyList<TopTreatmentType> _rows = [];
    private int _limit = 8;
    private bool _arranging;
    public ReportTypesContent(BarChart chart, ClinicTable table)
    {
        Name = "reportTypesLayout"; _chart = chart; _table = table; Dock = DockStyle.Fill; BackColor = Palette.Surface;
        Controls.Add(chart); Controls.Add(table); SizeChanged += (_, _) => Arrange(); Arrange();
    }
    private void Arrange()
    {
        if (_arranging) return;
        _arranging = true; SuspendLayout();
        try
        {
            var wide = Width >= Metrics.Scale(this, Metrics.FormWidth * 2);
            ColumnCount = wide ? 2 : 1; RowCount = wide ? 1 : 2;
            ColumnStyles.Clear(); RowStyles.Clear();
            for (var c = 0; c < ColumnCount; c++) ColumnStyles.Add(new(SizeType.Percent, 100f / ColumnCount));
            for (var r = 0; r < RowCount; r++) RowStyles.Add(new(SizeType.Percent, 100f / RowCount));
            SetCellPosition(_chart, new(0, 0)); SetCellPosition(_table, new(wide ? 1 : 0, wide ? 0 : 1));
            var limit = wide ? 8 : Math.Clamp(Height / 2 / Metrics.Scale(this, Metrics.CompactHeight), 1, 8);
            if (limit != _limit) { _limit = limit; BindChart(); }
        }
        finally { ResumeLayout(true); _arranging = false; }
    }
    public void SetData(IReadOnlyList<TopTreatmentType> rows) { _rows = rows; BindChart(); }
    private void BindChart() => _chart.SetData(ReportsPresentation.TypeChart(_rows.Take(_limit).ToArray()), value => value.ToString("N0"), "No treatments in this range");
}
