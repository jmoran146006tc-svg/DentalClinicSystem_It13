namespace DentalClinicSystem.Helpers.Design.Controls;

public class Badge : AntdUI.Tag
{
    private SemanticStyle _style = Palette.Neutral;
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public SemanticStyle Style
    {
        get => _style;
        set { _style = value; BackColor = value.Background; ForeColor = value.Text; }
    }
    public Badge(string text, Semantic semantic = Semantic.Neutral)
    {
        Theme.MarkPrimitive(this); Text = text; Style = Theme.SemanticStyle(semantic);
        Font = Typography.Label; Radius = Metrics.ControlRadius; BorderWidth = 0;
        Height = Metrics.CompactHeight; Width = Metrics.ControlHeight * 3;
        AccessibleRole = AccessibleRole.StaticText;
        TextChanged += (_, _) => Fit(); DpiChangedAfterParent += (_, _) => Fit(); Fit();
    }
    private void Fit() => MinimumSize = new(TextRenderer.MeasureText(Text, Typography.Label).Width + Metrics.Scale(this, Space.Xl), Metrics.Scale(this, Metrics.CompactHeight));
}

public sealed class StatusBadge : Badge
{
    public StatusBadge(string status) : base(Models.AppointmentStatus.Display(status)) { Style = Theme.StatusStyle(status); }
    public void SetStatus(string status) { Text = Models.AppointmentStatus.Display(status); Style = Theme.StatusStyle(status); Invalidate(); }
}
