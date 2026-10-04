namespace DentalClinicSystem.Models;

public static class DiscountTypes
{
    public const string None = "None", Senior = "Senior", Pwd = "PWD", Other = "Other";
    public static readonly string[] All = [None, Senior, Pwd, Other];
    public static string Display(string type) => type == Senior ? "Senior Citizen" : type;
    public static bool IsStandard(string type) => type is Senior or Pwd;
}
