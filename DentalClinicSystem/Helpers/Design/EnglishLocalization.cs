namespace DentalClinicSystem.Helpers.Design;

// Keys verified against the AntdUI 2.4.12 assembly, including popup-only text.
internal sealed class EnglishLocalization : AntdUI.ILocalization
{
    internal static IReadOnlyDictionary<string, string> Entries { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["ID"] = "en-US", ["OK"] = "OK", ["Cancel"] = "Cancel", ["Now"] = "Now", ["ToDay"] = "Today",
        ["YearFormat"] = "yyyy", ["MonthFormat"] = "MMM",
        ["Mon"] = "Mon", ["Tue"] = "Tue", ["Wed"] = "Wed", ["Thu"] = "Thu", ["Fri"] = "Fri", ["Sat"] = "Sat", ["Sun"] = "Sun",
        ["NoData"] = "No data", ["ItemsPerPage"] = "items per page",
        ["Copy"] = "Copy", ["SelectAll"] = "Select all", ["Undo"] = "Undo", ["Redo"] = "Redo",
        ["Cut"] = "Cut", ["Paste"] = "Paste", ["Delete"] = "Delete",
        ["Close"] = "Close", ["Restore"] = "Restore", ["Full"] = "Full screen", ["Maximize"] = "Maximize", ["Minimize"] = "Minimize",
        ["Filter"] = "Filter", ["Filter.Equal"] = "Equals", ["Filter.NotEqual"] = "Does not equal",
        ["Filter.Greater"] = "Greater than / starts with", ["Filter.Less"] = "Less than / ends with",
        ["Filter.Contain"] = "Contains", ["Filter.NotContain"] = "Does not contain",
        ["Filter.Blank"] = "(Blank)", ["Filter.Clean"] = "Reset", ["Filter.SelectAll"] = "(Select all)", ["Filter.Search"] = "Search",
        ["SUM"] = "Sum", ["AVG"] = "Average", ["MIN"] = "Minimum", ["MAX"] = "Maximum", ["COUNT"] = "Count",
        ["Table.Summary.NONE"] = "No summary", ["Table.Summary.Mode"] = "Selected rows", ["Table.Summary.ModeALL"] = "All rows"
    };
    public string GetLocalizedString(string key) => Entries.GetValueOrDefault(key, key);
}
