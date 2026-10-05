using System.ComponentModel;
using System.Text.RegularExpressions;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers;

public static class GridHelper
{
    private static readonly Dictionary<string, string> Headers = new()
    {
        ["FirstName"] = "First name", ["LastName"] = "Last name", ["FullName"] = "Full name",
        ["DateOfBirth"] = "Date of birth", ["ContactNumber"] = "Contact", ["Email"] = "Email", ["Address"] = "Address",
        ["AppointmentDateTime"] = "Appointment date", ["DatePerformed"] = "Date performed", ["IsActive"] = "Active",
        ["LicenseNumber"] = "License", ["ToothNumber"] = "Tooth", ["Username"] = "Username"
    };
    public static void Bind<T>(ClinicTable grid, IEnumerable<T> rows, params string[] hiddenColumns) =>
        Bind(grid, rows, null, "Try changing the filters or add a record.", hiddenColumns);
    public static void Bind<T>(ClinicTable grid, IEnumerable<T> rows, Func<T, object>? key, string emptyMessage, params string[] hiddenColumns)
    {
        Theme.StyleGrid(grid);
        var hidden = hiddenColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        grid.Columns.Clear();
        foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(typeof(T)))
        {
            var name = property.Name;
            // Do not expose IDs, model objects, credentials or internal fields.
            if (hidden.Contains(name) || name.Equals(typeof(T).Name + "Id", StringComparison.OrdinalIgnoreCase)
                || name is "PasswordHash" or "Password" or "Record"
                || name == "FullName" && (typeof(T) == typeof(Patient) || typeof(T) == typeof(Dentist))) continue;
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var column = new AntdUI.Column(name, Headers.GetValueOrDefault(name, Regex.Replace(name, "(?<=[a-z])([A-Z])", " $1")))
            {
                SortOrder = true, Editable = false, Ellipsis = true,
                Align = type == typeof(decimal) || type == typeof(int) || type == typeof(double) ? AntdUI.ColumnAlign.Right : AntdUI.ColumnAlign.Left,
                MinWidth = (name.Contains("Date", StringComparison.Ordinal) || name is "Email" or "Address" or "Notes" ? Metrics.GridWideColumn
                    : name.Contains("Name", StringComparison.Ordinal) || name is "Patient" or nameof(DentistWorkload.Dentist) or "Status" ? Metrics.GridIdentityColumn : Metrics.GridCompactColumn).ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
            column.Render = (value, record, index) => Render(name, value);
            grid.Columns.Add(column);
        }
        grid.EmptyText = emptyMessage;
        if (key is not null) GridTheme.SetKey(grid, item => key((T)item));
        grid.BindRecords(rows.ToList());
    }
    private static object Render(string name, object? value)
    {
        if (name is "Status" or "Role")
        {
            var text = value?.ToString() ?? "n/a";
            var style = name == "Status" ? Theme.StatusStyle(text) : Palette.Neutral;
            return new AntdUI.CellTag(name == "Status" ? AppointmentStatus.Display(text) : text) { Back = style.Background, Fore = style.Text, BorderWidth = 0 };
        }
        if (value is null || value is string blank && string.IsNullOrWhiteSpace(blank)) return new AntdUI.CellText("n/a", Palette.Ink500);
        if (name == "ContactNumber") return DisplayFormat.Phone(value.ToString());
        if (value is DateTime date) return date.ToString(DisplayFormat.ColumnDatePattern(name), System.Globalization.CultureInfo.InvariantCulture);
        if (value is decimal amount && name is "Cost" or "Net" or "Billed") return DisplayFormat.Currency(amount);
        if (value is bool enabled) return new AntdUI.CellTag(enabled ? "Active" : "Inactive", enabled ? AntdUI.TTypeMini.Success : AntdUI.TTypeMini.Default);
        return value;
    }
    public static void FlashRow(ClinicTable grid, object key) => GridTheme.FlashRow(grid, key);
    public static void IdentityColumn<T>(ClinicTable grid, string column, Func<T, (string Name, string Detail)> identity) =>
        GridTheme.IdentityColumn(grid, column, item => identity((T)item));
}
