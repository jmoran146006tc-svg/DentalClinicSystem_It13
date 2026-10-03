using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms
{
    public sealed record NavItem(
        string Text,
        Permission RequiredPermission,
        Func<Control>? CreatePage,
        Button Button);
}
