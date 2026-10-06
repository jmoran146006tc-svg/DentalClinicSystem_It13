namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ReportSection : RoundedPanel
{
    private readonly Control _data;
    private readonly EmptyState _empty;
    private readonly Skeleton _skeleton = new() { Dock = DockStyle.Fill, Margin = Padding.Empty };
    public ReportSection(string title, Control data, string emptyText, IconKind icon)
    {
        Dock = DockStyle.Fill; _data = data; _data.Dock = DockStyle.Fill;
        var heading = new Label { Text = title, Dock = DockStyle.Top, Height = Metrics.ControlHeight,
            Font = Typography.Heading, ForeColor = Palette.Ink900 };
        var body = new Panel { Dock = DockStyle.Fill, BackColor = Palette.Surface, Padding = new(0, Metrics.Scale(this, Space.Sm), 0, 0) };
        DpiChangedAfterParent += (_, _) => body.Padding = new(0, Metrics.Scale(this, Space.Sm), 0, 0);
        _empty = new EmptyState("", emptyText, icon) { Visible = false };
        _empty.RowStyles.Add(new(SizeType.Percent, 50)); _empty.RowStyles.Add(new(SizeType.Absolute, 0));
        _empty.RowStyles.Add(new(SizeType.Percent, 50)); _empty.RowStyles.Add(new(SizeType.Absolute, 0));
        foreach (var label in _empty.Controls.OfType<Label>())
        {
            label.Visible = label.Text.Length > 0; label.Anchor = AnchorStyles.Top; label.Margin = new(0, Space.Sm, 0, 0);
        }
        _empty.Controls.OfType<IconTile>().Single().Anchor = AnchorStyles.Bottom;
        body.Controls.AddRange([_data, _empty, _skeleton]); _skeleton.BringToFront();
        Content.Controls.Add(body); Content.Controls.Add(heading);
    }
    public void SetEmpty(bool empty)
    {
        _skeleton.Visible = false; _data.Visible = !empty; _empty.Visible = empty;
        if (empty) _empty.BringToFront();
    }
    public void StopLoading() => _skeleton.Visible = false;
}
