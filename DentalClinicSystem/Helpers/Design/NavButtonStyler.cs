using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers.Design;

public static class NavButtonStyler
{
    public static void Attach(Button button, IconKind icon)
    {
        ButtonStyler.Attach(button, ButtonVariant.Ghost, icon);
        button.Font = Typography.Body; button.TextAlign = ContentAlignment.MiddleLeft;
        button.TextCenterHasIcon = false;
        button.Height = Metrics.NavHeight; button.Width = Metrics.SidebarWidth - Space.Xl * 2;
        button.Margin = new Padding(0, Space.Xs, 0, 0);
    }
    public static void Select(Button button, bool selected)
    {
        button.Ghost = !selected; button.Type = AntdUI.TTypeMini.Default;
        button.DefaultBack = selected ? Palette.BrandSoft : Palette.Surface;
        button.ForeColor = selected ? Palette.BrandSoftText : Palette.Ink700;
        button.Invalidate();
    }
}
