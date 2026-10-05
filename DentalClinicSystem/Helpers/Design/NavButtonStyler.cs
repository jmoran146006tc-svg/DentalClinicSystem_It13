using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers.Design;

public static class NavButtonStyler
{
    public static void Attach(Button button, IconKind icon)
    {
        Theme.MarkPrimitive(button);
        button.Font = Typography.Nav; button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = Padding.Empty; button.MinimumSize = Size.Empty;
        button.AutoSizeMode = AntdUI.TAutoSize.None; button.WaveSize = 0; button.BorderWidth = 0;
        button.TabStop = false; button.DefaultBack = Palette.SidebarBg;
        button.Height = Metrics.Scale(button, Metrics.NavHeight); button.Width = Metrics.Scale(button, Metrics.SidebarWidth - Space.Xl * 2);
        button.Margin = new Padding(0, Space.Xs, 0, 0);
        var visual = new NavItemButton(button.Text ?? string.Empty, icon);
        visual.Click += (_, _) => button.PerformClick();
        button.Controls.Add(visual);
    }
    public static void Select(Button button, bool selected)
    {
        foreach (var visual in button.Controls.OfType<NavItemButton>()) visual.Selected = selected;
    }
}
