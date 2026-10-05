using System.Globalization;

namespace DentalClinicSystem.Helpers.Design;

public static class AntdTheme
{
    public static void Initialize()
    {
        AntdUI.Config.Mode = AntdUI.TMode.Light;
        AntdUI.Config.Animation = Motion.Motion.Enabled;
        AntdUI.Config.FocusBorderEnabled = true;
        AntdUI.Config.ScrollBarHide = false;
        var culture = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
        // DefaultLanguage describes the library's fallback strings, which are Chinese.
        AntdUI.Localization.DefaultLanguage = "zh-CN";
        AntdUI.Localization.Provider = new EnglishLocalization();
        AntdUI.Localization.SetLanguage(culture.Name);
        AntdUI.Style.SetPrimary(Palette.Brand);
        AntdUI.Style.SetSuccess(Palette.Success.Text);
        AntdUI.Style.SetError(Palette.Danger.Text);
        AntdUI.Style.SetWarning(Palette.WarningAccent);
        AntdUI.Style.SetInfo(Palette.Info.Text);
        ConfigureAlerts();
    }

    private static void ConfigureAlerts()
    {
        foreach (var (baseColor, background, border, pair) in new[]
        {
            (AntdUI.Colour.Info, AntdUI.Colour.InfoBg, AntdUI.Colour.InfoBorder, Palette.Info),
            (AntdUI.Colour.Success, AntdUI.Colour.SuccessBg, AntdUI.Colour.SuccessBorder, Palette.Success),
            (AntdUI.Colour.Warning, AntdUI.Colour.WarningBg, AntdUI.Colour.WarningBorder, Palette.Warning),
            (AntdUI.Colour.Error, AntdUI.Colour.ErrorBg, AntdUI.Colour.ErrorBorder, Palette.Danger)
        })
        {
            AntdUI.Style.Set(baseColor, pair.Text, "Alert");
            AntdUI.Style.Set(background, pair.Background);
            AntdUI.Style.Set(border, pair.Background);
        }
    }

    public static string Svg(IconKind icon)
    {
        // Native icon keys inherit the control's foreground and disabled colors.
        return icon switch
        {
            IconKind.Plus => "PlusOutlined",
            IconKind.Close => "CloseOutlined",
            IconKind.Search => "SearchOutlined",
            IconKind.Eye => "EyeOutlined",
            IconKind.EyeOff => "EyeInvisibleOutlined",
            IconKind.Lock => "LockOutlined",
            IconKind.Edit => "EditOutlined",
            IconKind.ChevronDown => "DownOutlined",
            IconKind.Logout => "LogoutOutlined",
            IconKind.Appointments => "CalendarOutlined",
            IconKind.Reports => "BarChartOutlined",
            IconKind.Dashboard => "AppstoreOutlined",
            IconKind.Treatments => "MedicineBoxOutlined",
            IconKind.Warning => "WarningOutlined",
            IconKind.Check => "CheckOutlined",
            IconKind.Info => "InfoCircleOutlined",
            IconKind.ChevronLeft => "LeftOutlined",
            IconKind.ChevronRight => "RightOutlined",
            IconKind.Clock => "ClockCircleOutlined",
            IconKind.Phone => "PhoneOutlined",
            IconKind.Mail => "MailOutlined",
            IconKind.Dentist => "SmileOutlined",
            IconKind.Users => "SafetyCertificateOutlined",
            IconKind.Patients => "UserOutlined",
            _ => "UserOutlined"
        };
    }
}
