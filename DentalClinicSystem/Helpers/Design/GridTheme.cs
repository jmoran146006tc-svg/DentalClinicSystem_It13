using System.Runtime.CompilerServices;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design;

public static class GridTheme
{
    private sealed class State
    {
        public Func<object, object>? Key { get; set; }
        public Func<object, bool>? Inactive { get; set; }
        public Dictionary<object, float> Flashes { get; } = [];
    }
    private static readonly ConditionalWeakTable<ClinicTable, State> States = new();
    public static void Apply(ClinicTable grid)
    {
        if (States.TryGetValue(grid, out _)) return;
        var state = new State(); States.Add(grid, state);
        grid.Font = Typography.Body; grid.BackColor = Palette.Surface; grid.ForeColor = Palette.Ink700;
        grid.ColumnFont = Typography.Label; grid.ColumnBack = Palette.SurfaceAlt; grid.ColumnFore = Palette.Ink500;
        grid.RowHeight = Metrics.NavHeight; grid.RowHeightHeader = Metrics.GridHeaderHeight;
        grid.RowHoverBg = Palette.SurfaceAlt; grid.RowSelectedBg = Palette.BrandSoft; grid.RowSelectedFore = Palette.Ink900;
        grid.BorderColor = Palette.Line; grid.Bordered = false;
        grid.AutoSizeColumnsMode = AntdUI.ColumnsMode.Fill;
        grid.EnableHeaderResizing = true;
        grid.SetRowStyle += (_, e) =>
        {
            var muted = state.Inactive?.Invoke(e.Record) == true;
            var flash = state.Key is not null && state.Flashes.TryGetValue(state.Key(e.Record), out var progress) ? progress : 0;
            return new AntdUI.Table.CellStyleInfo { ForeColor = muted ? Palette.Ink400 : Palette.Ink700,
                BackColor = flash > 0 ? Theme.Lerp(Palette.Surface, Palette.BrandSoft, flash) : null };
        };
        grid.Disposed += (_, _) => MotionSystem.Animator.Cancel(grid);
    }
    public static void SetEmptyState(ClinicTable grid, bool empty, string message) { Apply(grid); grid.EmptyText = message; }
    public static void SetKey(ClinicTable grid, Func<object, object> selector) { Apply(grid); States.GetOrCreateValue(grid).Key = selector; }
    public static void MuteInactive<T>(ClinicTable grid, Func<T, bool> inactive) { Apply(grid); States.GetOrCreateValue(grid).Inactive = item => inactive((T)item); }
    public static void IdentityColumn(ClinicTable grid, string column, Func<object, (string Name, string Detail)> identity)
    {
        Apply(grid);
        var target = grid.Columns.FirstOrDefault(c => c.Key == column);
        if (target is null) return;
        target.Render = (_, record, _) =>
        {
            var (name, detail) = identity(record);
            return new AntdUI.CellText(name + Environment.NewLine + detail);
        };
        target.LineBreak = true; grid.RowHeight = Metrics.IdentityHeight; grid.Refresh();
    }
    public static void FlashRow(ClinicTable grid, object key)
    {
        if (!States.TryGetValue(grid, out var state) || state.Key is null) return;
        MotionSystem.Animator.Run(grid, $"row-{key}", 1, 0, MotionSystem.RowFlash, Easing.EaseOutCubic,
            value => { state.Flashes[key] = value; grid.Invalidate(); }, () => { state.Flashes.Remove(key); grid.Invalidate(); });
    }
    public static void SelectAndFlash(ClinicTable grid, object key)
    {
        if (!States.TryGetValue(grid, out var state) || state.Key is null) return;
        var item = grid.Records.FirstOrDefault(record => Equals(state.Key(record), key));
        if (item is null) return;
        grid.SetSelected(item, true); grid.ScrollLine(item, true); FlashRow(grid, key);
    }
}
