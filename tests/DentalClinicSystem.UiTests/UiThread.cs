using System.Runtime.ExceptionServices;
using DentalClinicSystem.Helpers.Design;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DentalClinicSystem.UiTests;

internal static class UiThread
{
    public static void Run(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { MotionSystem.Enabled = false; AntdTheme.Initialize(); action(); }
            catch (Exception failure) { error = failure; }
            finally { AntdUI.Notification.close_all(); MotionSystem.UseSystemPreference(); }
        });
        thread.SetApartmentState(ApartmentState.STA); thread.IsBackground = true; thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "UI check did not finish in 30 seconds.");
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }
    public static IEnumerable<Control> Controls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var descendant in Controls(child)) yield return descendant;
        }
    }
    public static T Named<T>(Control root, string name) where T : Control => Controls(root).OfType<T>().FirstOrDefault(c => c.Name == name)
        ?? (T)root.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(root)!;
    public static void Show(Form form)
    {
        form.StartPosition = FormStartPosition.Manual; form.Location = new(-20000, -20000);
        form.ShowInTaskbar = false; form.Show(); Application.DoEvents();
    }
    public static void Capture(Form form, string name)
    {
        var directory = Environment.GetEnvironmentVariable("DENTAL_UI_SNAPSHOTS");
        if (string.IsNullOrEmpty(directory)) return;
        Directory.CreateDirectory(directory);
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
        bitmap.Save(Path.Combine(directory, name + ".png"));
    }
}
