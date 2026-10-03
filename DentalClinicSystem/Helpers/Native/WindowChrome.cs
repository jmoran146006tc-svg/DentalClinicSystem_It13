using System.Runtime.InteropServices;
using DentalClinicSystem.Helpers.Design;

namespace DentalClinicSystem.Helpers.Native;

public static class WindowChrome
{
    private const int CornerPreference = 33, BorderColor = 34, CaptionColor = 35, TextColor = 36, Round = 2;
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
    public static void Apply(Form form)
    {
        if (!form.IsHandleCreated) { form.HandleCreated += OnHandleCreated; return; }
        Attribute(form.Handle, CornerPreference, Round);
        Attribute(form.Handle, CaptionColor, ColorRef(Palette.Surface));
        Attribute(form.Handle, TextColor, ColorRef(Palette.Ink900));
        Attribute(form.Handle, BorderColor, ColorRef(Palette.Line));
    }
    private static void OnHandleCreated(object? sender, EventArgs e)
    {
        if (sender is Form form) { form.HandleCreated -= OnHandleCreated; Apply(form); }
    }
    private static int ColorRef(Color color) => color.R | color.G << 8 | color.B << 16;
    private static void Attribute(IntPtr handle, int attribute, int value)
    {
        try { DwmSetWindowAttribute(handle, attribute, ref value, sizeof(int)); }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
        catch (ExternalException) { }
    }
}
