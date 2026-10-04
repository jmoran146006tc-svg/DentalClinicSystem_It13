using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers;

public static class GridHelper
{
    private sealed class Binding
    {
        public Type RowType { get; set; } = typeof(object);
        public HashSet<string> Hidden { get; set; } = [];
    }
    private static readonly ConditionalWeakTable<DataGridView, Binding> Bindings = new();
    private static readonly Dictionary<string, string> Headers = new()
    {
        ["FirstName"] = "First name", ["LastName"] = "Last name", ["FullName"] = "Full name",
        ["DateOfBirth"] = "Date of birth", ["ContactNumber"] = "Contact", ["Email"] = "Email", ["Address"] = "Address",
        ["AppointmentDateTime"] = "Appointment date", ["DatePerformed"] = "Date performed", ["IsActive"] = "Active",
        ["LicenseNumber"] = "License", ["ToothNumber"] = "Tooth", ["Username"] = "Username"
    };
    public static void Bind<T>(DataGridView grid, IEnumerable<T> rows, params string[] hiddenColumns) =>
        Bind(grid, rows, null, "Try changing the filters or add a record.", hiddenColumns);
    public static void Bind<T>(DataGridView grid, IEnumerable<T> rows, Func<T, object>? key, string emptyMessage, params string[] hiddenColumns)
    {
        Theme.StyleGrid(grid);
        if (!Bindings.TryGetValue(grid, out var binding))
        {
            binding = new Binding(); Bindings.Add(grid, binding);
            grid.DataBindingComplete += (_, _) => ConfigureColumns(grid, binding);
            grid.DpiChangedAfterParent += (_, _) => ConfigureColumns(grid, binding);
            grid.HandleCreated += (_, _) => ConfigureColumns(grid, binding);
        }
        binding.RowType = typeof(T); binding.Hidden = hiddenColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var items = rows.ToList();
        grid.ScrollBars = items.Count == 0 ? ScrollBars.None : ScrollBars.Both;
        grid.DataSource = new BindingList<T>(items);
        ConfigureColumns(grid, binding);
        grid.CurrentCell = null; grid.ClearSelection();
        if (key is not null) GridTheme.SetKey(grid, item => key((T)item));
        // Binding can complete after parenting. Source count is already reliable.
        GridTheme.SetEmptyState(grid, items.Count == 0, emptyMessage);
    }
    private static void ConfigureColumns(DataGridView grid, Binding binding)
    {
        if (grid.IsDisposed) return;
        GridTheme.Apply(grid);
        foreach (DataGridViewColumn column in grid.Columns)
        {
            var name = !string.IsNullOrEmpty(column.DataPropertyName) ? column.DataPropertyName : column.Name ?? string.Empty;
            if (binding.Hidden.Contains(name) || name.Equals(binding.RowType.Name + "Id", StringComparison.OrdinalIgnoreCase)
                || name == "FullName" && (binding.RowType == typeof(Patient) || binding.RowType == typeof(Dentist)))
                column.Visible = false;
            column.HeaderText = Headers.GetValueOrDefault(name, Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1"));
            if (Nullable.GetUnderlyingType(column.ValueType ?? typeof(object)) == typeof(DateTime) || column.ValueType == typeof(DateTime))
            {
                column.DefaultCellStyle.Format = DisplayFormat.ColumnDatePattern(name);
                column.DefaultCellStyle.FormatProvider = System.Globalization.CultureInfo.InvariantCulture;
            }
            // A Fill column's MinimumWidth setter creates the handle. That can run
            // page Load handlers and rebind/detach this column inside the setter.
            // Metadata is safe before the handle; sizing waits for HandleCreated.
            if (!grid.IsHandleCreated) continue;
            var shortWidth = Metrics.Scale(grid, Metrics.ControlHeight * 2);
            var width = name is "Email" or "Address" or "Notes" or "Reason" ? Metrics.FormWidth / 2
                : name.Contains("Date", StringComparison.Ordinal) ? Metrics.FormWidth / 2
                : name.Contains("Name", StringComparison.Ordinal) || name is "Patient" or "Dentist" or "ContactNumber" or "Status" ? Metrics.FormWidth / 3 : shortWidth;
            var font = column.DefaultCellStyle.Font ?? grid.DefaultCellStyle.Font ?? grid.Font ?? Typography.Body;
            var headerFont = column.HeaderCell.Style.Font ?? grid.ColumnHeadersDefaultCellStyle.Font ?? font;
            var header = TextRenderer.MeasureText(column.HeaderText ?? string.Empty, headerFont).Width;
            var contentWidth = name is "ContactNumber" ? Metrics.FormWidth / 3 + Space.Xl
                : name is "FullName" or "Name" or "Username" or "Patient" or "Dentist" or "Email" or "Address" ? Metrics.FormWidth / 2
                : column.ValueType == typeof(DateTime) || Nullable.GetUnderlyingType(column.ValueType ?? typeof(object)) == typeof(DateTime) ? name.Contains("Time", StringComparison.OrdinalIgnoreCase) ? Metrics.FormWidth / 2 + Space.Xl : Metrics.FormWidth / 3 + Space.Lg
                : name is "Status" or "Role" ? TextRenderer.MeasureText(AppointmentStatus.Completed, Typography.Label).Width + Space.Xxl + Space.Md : 0;
            column.MinimumWidth = Math.Max(Metrics.Scale(grid, contentWidth), Math.Max(shortWidth, header + Metrics.Scale(grid, Space.Lg * 2 + Metrics.IconSize)));
            column.FillWeight = width;
        }
    }
    public static void FlashRow(DataGridView grid, object key) => GridTheme.FlashRow(grid, key);
    public static void IdentityColumn<T>(DataGridView grid, string column, Func<T, (string Name, string Detail)> identity) =>
        GridTheme.IdentityColumn(grid, column, item => identity((T)item));
}
