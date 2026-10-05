namespace DentalClinicSystem.Helpers.Design;

public static class AntdTheme
{
    public static void Initialize()
    {
        AntdUI.Config.Mode = AntdUI.TMode.Light;
        AntdUI.Config.Animation = Motion.Motion.Enabled;
        AntdUI.Config.FocusBorderEnabled = true;
        AntdUI.Config.ScrollBarHide = false;
        AntdUI.Localization.DefaultLanguage = "en-US";
        AntdUI.Style.SetPrimary(Palette.Brand);
        AntdUI.Style.SetSuccess(Palette.Success.Text);
        AntdUI.Style.SetError(Palette.Danger.Text);
        AntdUI.Style.SetWarning(Palette.Warning.Text);
        AntdUI.Style.SetInfo(Palette.Info.Text);
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
            IconKind.Dentist or IconKind.Users or IconKind.Patients => "TeamOutlined",
            _ => "UserOutlined"
        };
    }
}
