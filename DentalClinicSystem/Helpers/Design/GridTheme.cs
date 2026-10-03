using System.Runtime.CompilerServices;
using DentalClinicSystem.Helpers.Design.Controls;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;
using DentalClinicSystem.Helpers.Design.Motion;

namespace DentalClinicSystem.Helpers.Design;

public static class GridTheme
{
    private sealed class State
    {
        public int Hover { get; set; } = -1;
        public EmptyState? Empty { get; set; }
        public string EmptyMessage { get; set; } = string.Empty;
        public Func<object, object>? Key { get; set; }
        public Dictionary<object, float> Flashes { get; } = [];
        public Dictionary<string, Func<object, (string Name, string Detail)>> Identities { get; } = [];
        public Func<object, bool>? Inactive { get; set; }
    }
    private static readonly ConditionalWeakTable<DataGridView, State> States = new();
    public static void Apply(DataGridView grid)
    {
        if (States.TryGetValue(grid, out _)) return;
        var state = new State(); States.Add(grid, state);
        DesignPaint.Enable(grid); grid.BackgroundColor = Palette.Surface; grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false; grid.RowHeadersVisible = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None; grid.GridColor = Palette.Line;
        grid.DefaultCellStyle = new() { Font = Typography.Body, ForeColor = Palette.Ink700, BackColor = Palette.Surface, SelectionBackColor = Palette.BrandSoft, SelectionForeColor = Palette.Ink900, Padding = new Padding(Space.Md, 0, Space.Md, 0) };
        grid.ColumnHeadersDefaultCellStyle = new() { Font = Typography.Label, ForeColor = Palette.Ink500, BackColor = Palette.SurfaceAlt, Padding = new Padding(Space.Md, 0, Space.Md, 0) };
        grid.RowTemplate.Height = Metrics.NavHeight; grid.ColumnHeadersHeight = Metrics.GridHeaderHeight;
        grid.CellMouseEnter += (_, e) => { var old = state.Hover; state.Hover = e.RowIndex; InvalidateRow(grid, old); InvalidateRow(grid, state.Hover); };
        grid.MouseLeave += (_, _) => { var old = state.Hover; state.Hover = -1; InvalidateRow(grid, old); };
        grid.CellPainting += (_, e) => PaintCell(grid, state, e);
        grid.CellFormatting += (_, e) => FormatCell(grid, e);
        grid.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.ValueType == typeof(decimal) || column.ValueType == typeof(int) || column.ValueType == typeof(double)) column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        };
        grid.Disposed += (_, _) => MotionSystem.Animator.Cancel(grid);
    }
    public static void SetEmptyState(DataGridView grid, bool empty, string message)
    {
        Apply(grid); var state = States.GetValue(grid, _ => new State());
        if (!empty) { state.Empty?.Dispose(); state.Empty = null; return; }
        if (state.Empty is null || state.EmptyMessage != message)
        {
            state.Empty?.Dispose(); state.Empty = new EmptyState("No records to show", message); state.EmptyMessage = message; grid.Controls.Add(state.Empty);
        }
        state.Empty.BringToFront();
    }
    public static void SetKey(DataGridView grid, Func<object, object> selector) { Apply(grid); States.GetValue(grid, _ => new State()).Key = selector; }
    public static void MuteInactive<T>(DataGridView grid, Func<T, bool> inactive) { Apply(grid); States.GetValue(grid, _ => new State()).Inactive = item => inactive((T)item); }
    public static void IdentityColumn(DataGridView grid, string column, Func<object, (string Name, string Detail)> identity)
    {
        Apply(grid); States.GetValue(grid, _ => new State()).Identities[column] = identity;
        grid.RowTemplate.Height = Metrics.IdentityHeight;
        foreach (DataGridViewRow row in grid.Rows) row.Height = Metrics.IdentityHeight;
        grid.Invalidate();
    }
    public static void FlashRow(DataGridView grid, object key)
    {
        if (!States.TryGetValue(grid, out var state) || state.Key is null) return;
        MotionSystem.Animator.Run(grid, $"row-{key}", 1, 0, MotionSystem.RowFlash, Easing.EaseOutCubic,
            t => { state.Flashes[key] = t; foreach (DataGridViewRow row in grid.Rows) if (row.DataBoundItem is { } item && Equals(state.Key(item), key)) InvalidateRow(grid, row.Index); },
            () => state.Flashes.Remove(key));
    }
    private static void InvalidateRow(DataGridView grid, int row) { if (!grid.IsDisposed && row >= 0 && row < grid.Rows.Count) grid.InvalidateRow(row); }
    private static void FormatCell(DataGridView grid, DataGridViewCellFormattingEventArgs e)
    {
        if (e.ColumnIndex < 0) return;
        var columnName = grid.Columns[e.ColumnIndex].Name;
        if (columnName == "ContactNumber") { e.Value = DisplayFormat.Phone(e.Value?.ToString()); e.FormattingApplied = true; }
        else if (e.Value is null || e.Value is string text && string.IsNullOrWhiteSpace(text)) { e.Value = "n/a"; e.CellStyle.ForeColor = Palette.Ink500; e.FormattingApplied = true; }
        if (e.Value is DateTime date)
        {
            var name = grid.Columns[e.ColumnIndex].Name;
            e.Value = date.ToString(DisplayFormat.ColumnDatePattern(name), System.Globalization.CultureInfo.InvariantCulture); e.FormattingApplied = true;
        }
        else if (e.Value is decimal value && grid.Columns[e.ColumnIndex].Name is "Cost" or "Revenue" or "TotalRevenue") { e.Value = DisplayFormat.Currency(value); e.FormattingApplied = true; }
    }
    private static void PaintCell(DataGridView grid, State state, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Graphics is not { } graphics) return;
        var row = grid.Rows[e.RowIndex]; var data = row.DataBoundItem; var name = grid.Columns[e.ColumnIndex].Name;
        var fill = row.Selected ? Palette.BrandSoft : state.Hover == e.RowIndex ? Palette.SurfaceAlt : Palette.Surface;
        if (!row.Selected && data is not null && state.Key is not null && state.Flashes.TryGetValue(state.Key(data), out var flash)) fill = Theme.Lerp(fill, Palette.BrandSoft, flash);
        using var brush = new SolidBrush(fill); graphics.FillRectangle(brush, e.CellBounds);
        if (data is not null && state.Inactive?.Invoke(data) == true) e.CellStyle!.ForeColor = Palette.Ink400;
        if (name is "Status" or "Role")
        {
            var status = e.FormattedValue?.ToString() ?? string.Empty; var style = name == "Role" ? Palette.Neutral : Theme.StatusStyle(status);
            var width = Math.Min(e.CellBounds.Width - Space.Xl, TextRenderer.MeasureText(status, Typography.Label).Width + Space.Xxl);
            var pill = new Rectangle(e.CellBounds.Left + Space.Md, e.CellBounds.Top + Space.Sm, Math.Max(0, width), Math.Max(0, e.CellBounds.Height - Space.Lg));
            DesignPaint.Surface(graphics, pill, pill.Height / 2f, style.Background);
            using var dot = new SolidBrush(style.Text); graphics.FillEllipse(dot, pill.Left + Space.Sm, pill.Top + (pill.Height - Metrics.StatusDot) / 2, Metrics.StatusDot, Metrics.StatusDot);
            TextRenderer.DrawText(graphics, status, Typography.Label, new Rectangle(pill.Left + Space.Xl, pill.Top, Math.Max(0, pill.Width - Space.Xl - Space.Xs), pill.Height), style.Text, DesignPaint.TextFlags);
        }
        else if (data is not null && state.Identities.TryGetValue(name, out var identity))
        {
            var (person, detail) = identity(data);
            Avatar.Draw(graphics, new(e.CellBounds.Left + Space.Sm, e.CellBounds.Top + Space.Xs, Metrics.NavHeight, Metrics.NavHeight), person);
            var left = e.CellBounds.Left + Metrics.NavHeight + Space.Lg;
            var inactive = state.Inactive?.Invoke(data) == true;
            TextRenderer.DrawText(graphics, person, Typography.Label, new Rectangle(left, e.CellBounds.Top + Space.Xs, Math.Max(0, e.CellBounds.Right - left - Space.Sm), Metrics.CompactHeight - Space.Sm), inactive ? Palette.Ink400 : Palette.Ink900, DesignPaint.TextFlags);
            TextRenderer.DrawText(graphics, detail, Typography.Caption, new Rectangle(left, e.CellBounds.Top + Metrics.CompactHeight - Space.Xs, Math.Max(0, e.CellBounds.Right - left - Space.Sm), Space.Xl), inactive ? Palette.Ink400 : Palette.Ink500, DesignPaint.TextFlags);
        }
        else e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.Border | DataGridViewPaintParts.ErrorIcon);
        e.Handled = true;
    }
}
