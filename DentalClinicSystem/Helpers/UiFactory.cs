using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers;

public static class UiFactory
{
    public static AppButton Button(string text, ButtonVariant variant = ButtonVariant.Primary, IconKind? icon = null, ButtonSize size = ButtonSize.Regular) => new(text, variant, icon, size);
    public static FormField Field(Control input, string caption, FieldKind kind = FieldKind.Text) => FieldBox.Wrap(input, caption, kind);
    public static FormField Search(TextBox input, string caption = "Search") => Field(input, caption, FieldKind.Search);
    public static Badge Badge(string text, Semantic semantic = Semantic.Neutral) => new(text, semantic);
    public static StatusBadge Status(string status) => new(status);
    public static RoundedPanel Card(string? title = null, ElevationLevel elevation = ElevationLevel.E0) => new(title, elevation);
    public static Toggle Toggle(string text = "Show inactive") => new(text);
    public static ConfirmDialog Confirm(string message, string title = "Please confirm") => new(message, title);
    public static ReasonDialog Reason() => new();
    public static InlineAlert Alert() => new();
}
