using System.ComponentModel;

namespace DentalClinicSystem.Helpers
{
    public static class GridHelper
    {
        public static void Bind<T>(DataGridView grid, IEnumerable<T> rows, params string[] hiddenColumns)
        {
            grid.DataSource = new BindingList<T>(rows.ToList());
            foreach (var name in hiddenColumns)
                if (grid.Columns[name] is { } column) column.Visible = false;
            grid.CurrentCell = null;
            grid.ClearSelection();
        }
    }
}
