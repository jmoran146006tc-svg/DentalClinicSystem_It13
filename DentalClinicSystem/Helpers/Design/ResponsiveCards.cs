namespace DentalClinicSystem.Helpers.Design;

public static class ResponsiveCards
{
    public static void Arrange(TableLayoutPanel grid, IReadOnlyList<Control> cards, int columns, int height)
    {
        columns = Math.Max(1, columns);
        var rowHeight = Metrics.Scale(grid, height);
        if (grid.IsDisposed || cards.Count == 0 || grid.ColumnCount == columns && grid.RowCount > 0
            && grid.RowStyles.Count > 0 && grid.RowStyles[0].Height == rowHeight) return;
        grid.SuspendLayout();
        try
        {
            grid.ColumnCount = columns; grid.RowCount = (cards.Count + columns - 1) / columns;
            grid.ColumnStyles.Clear(); grid.RowStyles.Clear();
            for (var c = 0; c < columns; c++) grid.ColumnStyles.Add(new(SizeType.Percent, 100f / columns));
            for (var r = 0; r < grid.RowCount; r++) grid.RowStyles.Add(new(SizeType.Absolute, rowHeight));
            for (var i = 0; i < cards.Count; i++) grid.SetCellPosition(cards[i], new(i % columns, i / columns));
        }
        finally { grid.ResumeLayout(true); }
    }
}
