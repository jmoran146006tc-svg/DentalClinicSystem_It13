using System.Globalization;
using System.Runtime.ExceptionServices;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DentalClinicSystem.Tests;

public class PresentationTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("ar-SA")]
    public void DatesUseSharedCultureIndependentPatterns(string culture)
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new(culture);
            var value = new DateTime(1998, 4, 12, 13, 5, 0);
            Assert.Equal("Apr 12, 1998", DisplayFormat.Date(value));
            Assert.Equal("Apr 12, 1998 1:05 PM", DisplayFormat.DateTime(value));
            Assert.Equal(DisplayFormat.DatePattern, DisplayFormat.ColumnDatePattern("DateOfBirth"));
            Assert.Equal(DisplayFormat.DatePattern, DisplayFormat.ColumnDatePattern("DatePerformed"));
            Assert.Equal(DisplayFormat.DateTimePattern, DisplayFormat.ColumnDatePattern("AppointmentDateTime"));
            Assert.Equal("₱1,250.50", DisplayFormat.Currency(1250.50m));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void ThemePreservesExplicitFontAndIsIdempotent() => Sta(() =>
    {
        using var root = new Panel(); using var headingFont = new Font("Segoe UI", 16, FontStyle.Bold);
        var heading = new Label { Text = "Dental clinic overview", Font = headingFont };
        var body = new Label();
        root.Controls.AddRange([heading, body]);
        Theme.Apply(root); Theme.Apply(root);
        Assert.Same(headingFont, heading.Font); Assert.Equal(Typography.Body, body.Font);
        using var laterFont = new Font("Segoe UI", 18, FontStyle.Bold);
        body.Font = laterFont; Theme.Apply(root); Assert.Same(laterFont, body.Font);
        var newChild = new Label(); root.Controls.Add(newChild); Theme.Apply(root); Assert.Equal(Typography.Body, newChild.Font);
    });

    [Fact]
    public void ThemeSkipsPrimitiveSubtreesAndRestylesLegacyFont() => Sta(() =>
    {
        using var root = new Panel(); using var legacyFont = new Font("Bahnschrift", 12);
        var legacy = new Label { Font = legacyFont }; var card = new RoundedPanel("Heading");
        var child = new Label { Font = Control.DefaultFont }; card.Content.Controls.Add(child);
        root.Controls.AddRange([legacy, card]); Theme.Apply(root);
        Assert.Equal(Typography.Body, legacy.Font); Assert.Equal(Control.DefaultFont, child.Font); Assert.NotNull(card.Tag);
    });

    [Fact]
    public void ButtonsCannotShrinkBelowStyledTextAndRemeasure() => Sta(() =>
    {
        using var button = new Button { Text = "Refresh", AutoSize = true, Width = 30 };
        ButtonStyler.Attach(button, ButtonVariant.Secondary);
        Assert.True(button.MinimumSize.Width >= TextRenderer.MeasureText(button.Text, button.Font).Width + Space.Lg * 2);
        button.Width = 1; Assert.Equal(button.MinimumSize.Width, button.Width);
        var previous = button.MinimumSize.Width; button.Text = "Refresh all appointments";
        Assert.True(button.MinimumSize.Width > previous);
        using var bigger = new Font(button.Font.FontFamily, 20, FontStyle.Bold); button.Font = bigger;
        Assert.True(button.MinimumSize.Width >= TextRenderer.MeasureText(button.Text, bigger).Width + Space.Lg * 2);
    });

    [Fact]
    public void GridKeepsColumnsButHidesOwnKeyAndUsesReadableHeaders() => Sta(() =>
    {
        using var root = new Form(); using var grid = new DataGridView { AllowUserToAddRows = false };
        GridHelper.Bind(grid, new[] { new Patient { PatientId = 7, FirstName = "Ana", LastName = "Santos", DateOfBirth = new(1998, 4, 12) } });
        Assert.DoesNotContain(grid.Controls.Cast<Control>(), control => control is EmptyState);
        root.Controls.Add(grid); root.CreateControl(); grid.BindingContext = new BindingContext();
        Assert.False(grid.Columns["PatientId"]!.Visible); Assert.False(grid.Columns["FullName"]!.Visible);
        Assert.Equal("First name", grid.Columns["FirstName"]!.HeaderText);
        Assert.Equal("Contact", grid.Columns["ContactNumber"]!.HeaderText);
        Assert.Equal(DisplayFormat.DatePattern, grid.Columns["DateOfBirth"]!.DefaultCellStyle.Format);
        Assert.All(grid.Columns.Cast<DataGridViewColumn>(), column => Assert.True(column.MinimumWidth >= TextRenderer.MeasureText(column.HeaderText, Typography.Label).Width + Space.Lg * 2));
        GridHelper.Bind(grid, new[] { new Treatment { TreatmentId = 1, AppointmentId = 2 } });
        Assert.False(grid.Columns["TreatmentId"]!.Visible); Assert.True(grid.Columns["AppointmentId"]!.Visible);
    });

    [Fact]
    public void NativeFieldsFitTheirViewportAndHoverDoesNotRelayoutCard() => Sta(() =>
    {
        using var field = FieldBox.Wrap(new TextBox(), "Name");
        field.PerformLayout(); field.Box.PerformLayout();
        Assert.True(field.Box.Input.Parent!.Height >= field.Box.Input.Height);
        using var card = new KpiCard("Appointments"); var bounds = card.Content.Bounds;
        card.Elevation = ElevationLevel.E2; Assert.Equal(bounds, card.Content.Bounds);
    });

    internal static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { error = ex; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (error is not null) ExceptionDispatchInfo.Capture(error).Throw();
    }
}
