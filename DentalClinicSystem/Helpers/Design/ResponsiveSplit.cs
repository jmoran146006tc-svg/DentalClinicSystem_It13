namespace DentalClinicSystem.Helpers.Design;

public sealed class ResponsiveSplit : TableLayoutPanel
{
    private readonly Control _grid;
    private readonly Control _form;
    private readonly Func<int> _formHeight;
    private bool _stacked;
    private bool _updating;
    public ResponsiveSplit(Control grid, Control form, Func<int> formHeight)
    {
        Theme.MarkPrimitive(this); _grid = grid; _form = form; _formHeight = formHeight;
        Dock = DockStyle.Fill; AutoScroll = true; BackColor = Palette.Canvas;
        grid.Dock = form.Dock = DockStyle.Fill; grid.Margin = new Padding(0, 0, Space.Lg, 0); form.Margin = Padding.Empty;
        ColumnCount = 2; RowCount = 1; Controls.Add(grid, 0, 0); Controls.Add(form, 1, 0);
        ColumnStyles.Add(new(SizeType.Percent, 100)); ColumnStyles.Add(new(SizeType.Absolute, Metrics.FormWidth)); RowStyles.Add(new(SizeType.Percent, 100));
        SizeChanged += (_, _) => Relayout(); form.VisibleChanged += (_, _) => Relayout();
    }
    private void Relayout()
    {
        if (_updating) return; _updating = true; SuspendLayout();
        try
        {
            _stacked = Width < Metrics.Scale(this, Metrics.MinimumWidth);
            ColumnStyles.Clear(); RowStyles.Clear();
            if (!_form.Visible)
            {
                ColumnCount = RowCount = 1; SetCellPosition(_grid, new(0, 0)); SetCellPosition(_form, new(0, 0));
                ColumnStyles.Add(new(SizeType.Percent, 100)); RowStyles.Add(new(SizeType.Percent, 100));
            }
            else if (_stacked)
            {
                ColumnCount = 1; RowCount = 2; SetCellPosition(_grid, new(0, 0)); SetCellPosition(_form, new(0, 1));
                ColumnStyles.Add(new(SizeType.Percent, 100));
                RowStyles.Add(new(SizeType.Absolute, Metrics.Scale(this, Metrics.EmptyHeight * 2)));
                RowStyles.Add(new(SizeType.Absolute, Math.Max(Metrics.Scale(this, Metrics.EmptyHeight), _formHeight())));
                _grid.Margin = new Padding(0, 0, 0, Space.Lg);
            }
            else
            {
                ColumnCount = 2; RowCount = 1; SetCellPosition(_grid, new(0, 0)); SetCellPosition(_form, new(1, 0));
                ColumnStyles.Add(new(SizeType.Percent, 100)); ColumnStyles.Add(new(SizeType.Absolute, Metrics.Scale(this, Metrics.FormWidth)));
                RowStyles.Add(new(SizeType.Percent, 100)); _grid.Margin = new Padding(0, 0, Space.Lg, 0);
            }
        }
        finally { ResumeLayout(true); _updating = false; }
    }
}
