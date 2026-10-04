using DentalClinicSystem.Helpers;

namespace DentalClinicSystem.Helpers.Design.Controls;

public class RecordDialog : DialogShell
{
    private readonly Func<Task<bool>> _save;
    public InlineAlert Alert { get; } = new() { Visible = false, Dock = DockStyle.Top };
    public RecordDialog(string title, Control fields, Func<Task<bool>> save) : base(title, "Save", new Size(640, 600))
    {
        _save = save; Body.AutoScroll = true;
        var content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Surface };
        content.ColumnStyles.Add(new(SizeType.Percent, 100));
        fields.Dock = DockStyle.Top; content.Controls.Add(Alert, 0, 0); content.Controls.Add(fields, 0, 1); Body.Controls.Add(content);
        UiMessages.RegisterAlertHost(this, Alert);
        var keys = new KeyboardShortcuts(this); keys.Register(Keys.Control | Keys.S, ConfirmButton.PerformClick);
        keys.RegisterEnterNavigation(Inputs(fields), ConfirmButton.PerformClick);
        Shown += (_, _) => fields.SelectNextControl(null, true, true, true, false);
    }
    private static IEnumerable<Control> Inputs(Control root) => root.Controls.Cast<Control>()
        .SelectMany(control => control is FieldBox box ? new[] { box.Input } : Inputs(control));
    protected override async Task<bool> ConfirmAsync()
    {
        using var owner = UiMessages.UseOwner(this);
        Alert.Visible = false;
        return await _save();
    }
}

// Both patient create and edit use the existing PatientService validation.
public sealed class PatientDialog(Control fields, Func<Task<bool>> save, bool editing)
    : RecordDialog(editing ? "Edit patient" : "New patient", fields, save);
