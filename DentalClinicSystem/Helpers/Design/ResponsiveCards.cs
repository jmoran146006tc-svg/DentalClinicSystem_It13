namespace DentalClinicSystem.Helpers.Design;

public static class ResponsiveCards
{
    public static void Arrange(TableLayoutPanel grid, IReadOnlyList<Control> cards, int columns, int height, int spanFrom, int finalCardHeight)
    {
        columns = Math.Max(1, columns);
        if (grid.IsDisposed || cards.Count == 0) return;
        spanFrom = Math.Clamp(spanFrom, 0, cards.Count);
        var firstRows = (spanFrom + columns - 1) / columns;
        var rows = firstRows + cards.Count - spanFrom;
        var rowHeight = Metrics.Scale(grid, height + Space.Lg);
        var lastHeight = Metrics.Scale(grid, finalCardHeight + Space.Lg);
        if (grid.ColumnCount == columns && grid.RowCount == rows && grid.RowStyles.Count == rows
            && grid.RowStyles[0].Height == rowHeight && grid.RowStyles[^1].Height == lastHeight) return;
        grid.SuspendLayout();
        try
        {
            grid.ColumnCount = columns; grid.RowCount = rows;
            grid.ColumnStyles.Clear(); grid.RowStyles.Clear();
            for (var c = 0; c < columns; c++) grid.ColumnStyles.Add(new(SizeType.Percent, 100f / columns));
            for (var r = 0; r < rows; r++) grid.RowStyles.Add(new(SizeType.Absolute, r == rows - 1 ? lastHeight : rowHeight));
            for (var i = 0; i < cards.Count; i++)
            {
                var spanning = i >= spanFrom;
                grid.SetColumnSpan(cards[i], spanning ? columns : 1);
                grid.SetCellPosition(cards[i], new(spanning ? 0 : i % columns, spanning ? firstRows + i - spanFrom : i / columns));
                cards[i].Margin = new(!spanning && i % columns > 0 ? Space.Sm : 0, 0,
                    !spanning && i % columns < columns - 1 ? Space.Sm : 0, Space.Lg);
            }
        }
        finally { grid.ResumeLayout(true); }
    }
}
