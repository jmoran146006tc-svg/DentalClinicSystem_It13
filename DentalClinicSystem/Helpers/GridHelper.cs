using System.ComponentModel;
using DentalClinicSystem.Helpers.Design;

namespace DentalClinicSystem.Helpers
{
    public static class GridHelper
    {
        public static void Bind<T>(DataGridView grid, IEnumerable<T> rows, params string[] hiddenColumns)
        {
            Theme.StyleGrid(grid);
            grid.DataSource = new BindingList<T>(rows.ToList());
            foreach (var name in hiddenColumns)
                if (grid.Columns[name] is { } column) column.Visible = false;
            grid.CurrentCell = null;
            grid.ClearSelection();
            GridTheme.SetEmptyState(grid, grid.Rows.Count == 0, "Try changing the filters or add a record.");
        }
        public static void Bind<T>(DataGridView grid, IEnumerable<T> rows, Func<T, object> key, string emptyMessage, params string[] hiddenColumns)
        {
            Bind(grid, rows, hiddenColumns);
            GridTheme.SetKey(grid, item => key((T)item));
            GridTheme.SetEmptyState(grid, grid.Rows.Count == 0, emptyMessage);
        }
        public static void FlashRow(DataGridView grid, object key) => GridTheme.FlashRow(grid, key);
        public static void IdentityColumn<T>(DataGridView grid, string column, Func<T, (string Name, string Detail)> identity) =>
            GridTheme.IdentityColumn(grid, column, item => identity((T)item));
    }
}
