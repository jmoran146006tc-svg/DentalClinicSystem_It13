namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ReportToolbar : FlowLayoutPanel
{
    private readonly Panel _spacer = new() { Height = Metrics.ControlHeight };
    private readonly Panel _rangeHost = new() { Name = "reportRangeHost", Height = Metrics.ControlHeight, BackColor = Palette.Canvas };
    private bool _arranging;
    public AntdUI.Segmented Ranges { get; } = new()
    {
        Name = "reportRanges", Height = Metrics.ControlHeight,
        Font = Typography.Body, Full = true, Radius = Metrics.ControlRadius, Gap = 0,
        BackColor = Palette.Surface, ForeColor = Palette.Ink700, BackActive = Palette.BrandSoft,
        ForeActive = Palette.BrandSoftText, BackHover = Palette.BrandSoft, ForeHover = Palette.BrandSoftText,
        TextAlign = AntdUI.TAlignFlow.Center, IconAlign = AntdUI.TAlignMini.None,
        BorderWidth = Metrics.Border, BorderColor = Palette.LineStrong, SelectIndex = 1,
        AccessibleName = "Report date range", TabStop = true
    };
    public ReportToolbar(ClinicDatePicker from, ClinicDatePicker to, AppButton refresh, AppButton export)
    {
        Name = "reportToolbar"; Dock = DockStyle.Top; AutoSize = true; WrapContents = true;
        BackColor = Palette.Canvas; Margin = Padding.Empty; Theme.MarkPrimitive(Ranges);
        foreach (var text in new[] { "This week", "This month", "Last 30 days", "Custom" })
            Ranges.Items.Add(new AntdUI.SegmentedItem { Text = text });
        // AntdUI subtracts Margin from its painting rectangle as well as the
        // parent's layout. Keep toolbar alignment on a host, not on Segmented.
        Ranges.Dock = DockStyle.Fill; Ranges.Margin = Padding.Empty; _rangeHost.Controls.Add(Ranges);
        MeasureRanges();
        foreach (var (picker, caption) in new[] { (from, "From"), (to, "To") })
        {
            var field = UiFactory.Field(picker, caption, FieldKind.Date); field.Width = Metrics.ReportDateWidth;
            Controls.Add(field);
        }
        refresh.Width = refresh.MinimumSize.Width; export.Width = export.MinimumSize.Width;
        Controls.AddRange([_rangeHost, _spacer, refresh, export]); ToolbarLayout.Attach(this, trimEnd: true);
        Layout += (_, _) => AlignActions();
    }
    private void MeasureRanges()
    {
        using var font = Typography.PixelFont(Ranges.Font, DeviceDpi);
        _rangeHost.Width = Ranges.Items.Cast<AntdUI.SegmentedItem>()
            .Sum(item => TextRenderer.MeasureText(item.Text, font).Width + Metrics.Scale(this, Space.Xl * 2));
    }
    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e); _rangeHost.Height = Metrics.Scale(this, Metrics.ControlHeight); MeasureRanges(); AlignActions();
    }
    private void AlignActions()
    {
        if (_arranging) return;
        _arranging = true;
        try
        {
            var fields = Controls.OfType<FormField>().ToArray();
            var actionsWidth = Controls.Cast<Control>().Where(c => c != _spacer && c is not FormField)
                .Sum(c => c.Width + c.Margin.Horizontal);
            var available = (ClientSize.Width - actionsWidth - fields.Sum(f => f.Margin.Horizontal)) / Math.Max(1, fields.Length);
            using var font = Typography.PixelFont(Ranges.Font, DeviceDpi);
            var dateText = Enumerable.Range(1, 12).Max(month => TextRenderer.MeasureText(DisplayFormat.Date(new DateTime(2026, month, 28)), font).Width);
            var minimum = dateText + Metrics.Scale(this, Metrics.IconSize + Space.Sm);
            foreach (var field in fields) field.Width = Math.Max(minimum, Math.Min(Metrics.Scale(this, Metrics.ReportDateWidth), available));
            if (fields.Length > 0 && available >= minimum && available < Metrics.Scale(this, Metrics.ReportDateWidth))
                fields[^1].Width += Math.Max(0, ClientSize.Width - actionsWidth - fields.Sum(f => f.Width + f.Margin.Horizontal));
            var required = Controls.Cast<Control>().Where(c => c != _spacer).Sum(c => c.Width + c.Margin.Horizontal);
            _spacer.Visible = ClientSize.Width >= required + Space.Sm;
            _spacer.Width = Math.Max(0, ClientSize.Width - required - _spacer.Margin.Horizontal);
        }
        finally { _arranging = false; }
    }
}
