using System.Reflection;
using System.Text.RegularExpressions;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.UiTests;

internal static class EnglishUi
{
    private static readonly Regex Cjk = new("[\\u4E00-\\u9FFF\\u3000-\\u303F\\uFF00-\\uFFEF]");
    public static void AssertTree(Control root)
    {
        foreach (var control in UiThread.Controls(root).Prepend(root))
        {
            AssertStrings(control);
            if (control is ClinicTable table) foreach (var column in table.Columns) AssertStrings(column);
            if (control is ClinicSelect select) Assert.True(select.List);
        }
    }
    private static void AssertStrings(object item)
    {
        var properties = item.GetType().GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanRead && p.GetIndexParameters().Length == 0
            && p.Name is "Text" or "PlaceholderText" or "EmptyText" or "TextTitle" or "Title" or "Tooltip" or "HeaderText" or "AccessibleName" or "AccessibleDescription");
        foreach (var property in properties) AssertEnglish(property.GetValue(item) as string, item.GetType().Name + "." + property.Name);
    }
    public static void AssertPopup(object popup)
    {
        var found = 0;
        for (var type = popup.GetType(); type is not null && type.Assembly == typeof(AntdUI.Input).Assembly; type = type.BaseType)
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (field.GetValue(popup) is string text) { AssertEnglish(text, field.Name); found++; }
                if (field.GetValue(popup) is string[] values) foreach (var value in values) { AssertEnglish(value, field.Name); found++; }
            }
        Assert.True(found > 0, "Popup string inspection found no fields.");
        if (popup is Control control) AssertTree(control);
    }
    public static void CapturePopup(object popup, string name)
    {
        var method = popup.GetType().GetMethod("PrintBit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(method);
        using var bitmap = Assert.IsType<Bitmap>(method.Invoke(popup, null));
        var colors = Enumerable.Range(0, bitmap.Width).Select(x => bitmap.GetPixel(x, bitmap.Height / 2).ToArgb()).Distinct().Count();
        Assert.True(colors > 2, "Layered popup capture is blank.");
        var directory = Environment.GetEnvironmentVariable("DENTAL_UI_SNAPSHOTS");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory); bitmap.Save(Path.Combine(directory, name + ".png"));
    }
    private static void AssertEnglish(string? text, string source) => Assert.False(Cjk.IsMatch(text ?? string.Empty), source + ": " + text);
}
