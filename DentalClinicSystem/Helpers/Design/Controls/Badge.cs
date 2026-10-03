namespace DentalClinicSystem.Helpers.Design.Controls;

public class Badge : DesignControl
{
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public SemanticStyle Style { get; set; } = Palette.Neutral;
    public Badge(string text, Semantic semantic = Semantic.Neutral)
    {
        Text = text; Style = Theme.SemanticStyle(semantic); Height = Metrics.CompactHeight; Width = Metrics.ControlHeight * 3;
        AccessibleRole = AccessibleRole.StaticText;
        Fit(); TextChanged += (_, _) => Fit(); DpiChangedAfterParent += (_, _) => Fit();
    }
    private void Fit() => MinimumSize = new(TextRenderer.MeasureText(Text, Typography.Label).Width + Metrics.Scale(this, Space.Md * 2 + Metrics.StatusDot + Space.Sm), Metrics.Scale(this, Metrics.CompactHeight));
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        DesignPaint.Surface(e.Graphics, ClientRectangle, Height / 2f, Style.Background);
        var dot = Metrics.Scale(this, Metrics.StatusDot);
        using var brush = new SolidBrush(Style.Text);
        e.Graphics.FillEllipse(brush, Space.Md, (Height - dot) / 2f, dot, dot);
        var rect = new Rectangle(Space.Md + dot + Space.Sm, 0, Math.Max(0, Width - Space.Xl - dot), Height);
        TextRenderer.DrawText(e.Graphics, Text, Typography.Label, rect, Style.Text, DesignPaint.TextFlags);
    }
}

public sealed class StatusBadge : Badge
{
    public StatusBadge(string status) : base(status) { Style = Theme.StatusStyle(status); }
    public void SetStatus(string status) { Text = status; Style = Theme.StatusStyle(status); Invalidate(); }
}
