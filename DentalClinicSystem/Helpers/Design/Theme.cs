using System.Runtime.CompilerServices;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Helpers.Design;

public static class Theme
{
    private static readonly ConditionalWeakTable<Control, object> Applied = new();
    public static SemanticStyle SemanticStyle(Semantic semantic) => semantic switch
    {
        Semantic.Info => Palette.Info, Semantic.Success => Palette.Success, Semantic.Danger => Palette.Danger,
        Semantic.Warning => Palette.Warning, _ => Palette.Neutral
    };
    public static SemanticStyle StatusStyle(string? status) => status switch
    {
        AppointmentStatus.Scheduled => Palette.Info, AppointmentStatus.Completed => Palette.Success,
        AppointmentStatus.NoShow => Palette.Danger, _ => Palette.Neutral
    };
    public static Color Lerp(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        if (t == 0) return a;
        if (t == 1) return b;
        return Color.FromArgb(Channel(a.A, b.A), Channel(a.R, b.R), Channel(a.G, b.G), Channel(a.B, b.B));
        int Channel(byte start, byte end) => (int)Math.Round(start + (end - start) * t);
    }
    public static void Apply(Control root)
    {
        if (root is DesignControl or RoundedPanel or FieldBox or FormField or AppButton or PageHeader or EmptyState or DialogShell) return;
        if (!Applied.TryGetValue(root, out _))
        {
            Applied.Add(root, new object());
            switch (root)
            {
                case Button button: ButtonStyler.Attach(button, Variant(button.Name)); break;
                case DataGridView grid: GridTheme.Apply(grid); break;
                case Label label: label.Font = Typography.Body; label.ForeColor = Palette.Ink700; break;
                case TextBoxBase or ComboBox or DateTimePicker: root.Font = Typography.Body; root.ForeColor = Palette.Ink700; break;
                case Form or Panel or UserControl: root.BackColor = Palette.Canvas; break;
            }
        }
        foreach (Control child in root.Controls) Apply(child);
    }
    private static ButtonVariant Variant(string name) => name.Contains("Delete", StringComparison.OrdinalIgnoreCase) || name.Contains("Deactivate", StringComparison.OrdinalIgnoreCase) || name.Contains("Cancel", StringComparison.OrdinalIgnoreCase)
        ? ButtonVariant.Danger : name.StartsWith("btnUpdate", StringComparison.Ordinal) || name is "btnAdd" or "btnSchedule" or "btnAddTreatment" or "btnLogin" or "btnSave"
        ? ButtonVariant.Primary : ButtonVariant.Secondary;
    public static void StyleGrid(DataGridView grid) => GridTheme.Apply(grid);
}
