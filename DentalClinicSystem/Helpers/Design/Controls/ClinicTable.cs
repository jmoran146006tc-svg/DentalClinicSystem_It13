using System.ComponentModel;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ClinicTable : AntdUI.Table
{
    public ClinicTable()
    {
        Font = Typography.Body; Radius = Metrics.CardRadius;
        FixedHeader = true; MultipleRows = false; EditMode = AntdUI.TEditMode.None;
        ClipboardCopy = true; EmptyText = "No records to show";
        SelectIndexChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? SelectedRecord => SelectedsReal().FirstOrDefault();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<object> Records { get; private set; } = [];
    public event EventHandler? SelectionChanged;
    public void BindRecords<T>(IReadOnlyList<T> rows)
    {
        ClearSelection();
        Records = rows.Cast<object>().ToArray();
        DataSource = rows.ToArray();
        ClearSelection();
    }
    public void ClearSelection() => SelectedIndex = -1;
}
