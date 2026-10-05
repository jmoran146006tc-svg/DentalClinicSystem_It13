using System.ComponentModel;

namespace DentalClinicSystem.Helpers.Design.Controls;

// Pick-only choices keep custom values in a separate, validated text field.
public sealed class OtherChoice
{
    public const string Other = "Other";
    private readonly ClinicSelect _select;
    private readonly AntdUI.Input _details;
    public FormField Details { get; }
    public bool IsOther => _select.Text == Other;
    public string Value => IsOther ? _details.Text.Trim() : (_select.Text ?? string.Empty).Trim();
    public OtherChoice(ClinicSelect select, string caption, int maxLength)
    {
        _select = select; select.DropDownStyle = ComboBoxStyle.DropDownList;
        _details = new AntdUI.Input { Name = select.Name + "Other", MaxLength = maxLength };
        Details = FieldBox.Wrap(_details, caption); Details.Visible = false;
        select.SelectedIndexChanged += (_, _) => { Details.Visible = IsOther; Details.SetError(string.Empty); };
    }
    public void SetValue(string? value)
    {
        value = value?.Trim() ?? string.Empty;
        var selected = _select.Items.FirstOrDefault(item => Display(item).Equals(value, StringComparison.OrdinalIgnoreCase));
        _select.SelectedItem = value.Length == 0 ? null : selected ?? Other;
        if (value.Length == 0) _select.Text = string.Empty;
        _details.Text = IsOther ? value : string.Empty;
        Details.Visible = IsOther; Details.SetError(string.Empty);
    }
    private string Display(object item) => (_select.DisplayMember.Length == 0 ? item.ToString()
        : TypeDescriptor.GetProperties(item)[_select.DisplayMember]?.GetValue(item)?.ToString()) ?? item.ToString() ?? string.Empty;
    public bool Validate()
    {
        if (!IsOther || Value.Length > 0) { Details.SetError(string.Empty); return true; }
        Details.SetError("This field is required."); _details.Focus(); return false;
    }
}
