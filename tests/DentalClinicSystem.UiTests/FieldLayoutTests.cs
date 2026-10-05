using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;

namespace DentalClinicSystem.UiTests;

public sealed class FieldLayoutTests
{
    [Theory]
    [InlineData("text")] [InlineData("select")] [InlineData("date")]
    public void EditorAndPaintedBorderFillTheField(string kind) => UiThread.Run(() =>
    {
        AntdUI.Input editor = kind switch { "select" => new ClinicSelect(), "date" => new ClinicDatePicker(), _ => new AntdUI.Input() };
        using var field = FieldBox.Wrap(editor, "Caption"); using var host = new Form { ClientSize = new(420, 140), BackColor = Palette.Surface };
        host.Controls.Add(field); UiThread.Show(host);
        Assert.Equal(Metrics.Scale(editor, Metrics.ControlHeight), editor.Height);
        Assert.Equal(field.Box.ClientRectangle, editor.Bounds);
        Assert.Equal(Metrics.Scale(editor, Metrics.ControlHeight), editor.ReadRectangle.Height + Metrics.Scale(editor, Metrics.Border * 2));
        Assert.Equal(Palette.Placeholder, editor.PlaceholderColor);
        using var bitmap = new Bitmap(editor.Width, editor.Height); editor.DrawToBitmap(bitmap, editor.ClientRectangle);
        var rows = Enumerable.Range(0, editor.Height).Where(y => bitmap.GetPixel(editor.Width / 2, y).ToArgb() != Palette.Surface.ToArgb()).ToArray();
        Assert.NotEmpty(rows);
        Assert.Equal(Metrics.Scale(editor, Metrics.ControlHeight), rows.Max() - rows.Min() + 1);
        UiThread.Capture(host, "fix-D-field-" + kind);
    });

    [Theory]
    [InlineData("patients")] [InlineData("appointments")] [InlineData("dentists")]
    public void ToolbarControlsShareTheInputRowCenter(string kind) => UiThread.Run(() =>
    {
        var services = ClinicFixture.Services(); var actor = ClinicFixture.Actor();
        using UserControl page = kind switch
        {
            "appointments" => new ucAppointmentScheduler(services.Appointments, services.Patients, services.Dentists, services.TreatmentTypes, actor),
            "dentists" => new ucDentistRecords(services.Dentists, actor),
            _ => new ucPatientRecords(services.Patients, actor)
        };
        using var host = new Form { ClientSize = new(1440, 900) }; page.Dock = DockStyle.Fill; host.Controls.Add(page); UiThread.Show(host);
        var toolbar = UiThread.Named<FlowLayoutPanel>(page, "recordsToolbar");
        var controls = toolbar.Controls.Cast<Control>().Where(c => c.Visible).SelectMany(c => c switch
        {
            FormField field => new[] { field.Box.Input },
            FlowLayoutPanel group => group.Controls.Cast<Control>().Where(child => child.Visible),
            _ => new[] { c }
        }).ToArray();
        Assert.True(controls.Length >= 3);
        var centers = controls.Select(c => c.PointToScreen(Point.Empty).Y + c.Height / 2d).ToArray();
        Assert.InRange(centers.Max() - centers.Min(), 0, 1);
        Assert.All(controls, c => Assert.Equal(Metrics.Scale(c, Metrics.ControlHeight), c.Height));
        UiThread.Capture(host, "fix-D-toolbar-" + kind);
    });

    [Fact]
    public void PlaceholderContrastIsAtLeastThreeToOne() => Assert.True(Contrast.Ratio(Palette.Placeholder, Palette.Surface) >= 3);
}
