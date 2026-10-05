using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.Helpers.Design;

// One input-row center for captioned fields, plain controls and action groups.
// The measured caption is reserved on each wrapped row as well.
public static class ToolbarLayout
{
    public static void Attach(FlowLayoutPanel toolbar)
    {
        var aligning = false;
        void Align()
        {
            if (aligning || toolbar.IsDisposed) return;
            aligning = true;
            try { Apply(toolbar); }
            finally { aligning = false; }
        }
        toolbar.Layout += (_, _) => Align();
        toolbar.ControlAdded += (_, _) => Align();
        toolbar.VisibleChanged += (_, _) => Align();
        Align();
    }
    private static void Apply(FlowLayoutPanel toolbar)
    {
        var controls = toolbar.Controls.Cast<Control>().Where(c => c.Visible).ToArray();
        foreach (var field in controls.OfType<FormField>()) field.PerformLayout();
        foreach (var group in controls.OfType<FlowLayoutPanel>())
        {
            foreach (Control item in group.Controls) item.Margin = Padding.Empty;
            group.PerformLayout();
        }
        var center = controls.Select(InputCenter).DefaultIfEmpty().Max();
        foreach (var control in controls)
        {
            var top = Math.Max(0, (int)Math.Round(center - InputCenter(control)));
            control.Margin = new Padding(0, top, Space.Sm, Space.Sm);
        }
    }
    private static double InputCenter(Control control) => control switch
    {
        FormField field => field.Box.Top + field.Box.Height / 2d,
        FlowLayoutPanel group when group.Controls.Cast<Control>().FirstOrDefault(c => c.Visible) is { } child
            => child.Top + child.Height / 2d,
        _ => control.Height / 2d
    };
}
