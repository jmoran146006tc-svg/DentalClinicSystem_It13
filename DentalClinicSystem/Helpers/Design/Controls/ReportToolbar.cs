namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ReportToolbar : FlowLayoutPanel
{
    private readonly Panel _spacer = new() { Height = Metrics.ControlHeight };
    private bool _arranging;
    public AntdUI.Segmented Ranges { get; } = new()
    {
        Name = "reportRanges", Height = Metrics.ControlHeight, Width = Metrics.ReportRangeWidth,
        Font = Typography.Body, Full = true, Radius = Metrics.ControlRadius, Gap = 0,
        BackColor = Palette.Surface, ForeColor = Palette.Ink700, BackActive = Palette.BrandSoft,
        ForeActive = Palette.BrandSoftText, BackHover = Palette.BrandSoft, ForeHover = Palette.BrandSoftText,
        BarPosition = AntdUI.TAlignMini.Bottom, BarBg = true, BarSize = Metrics.Border, BarColor = Palette.BrandAccent,
        BorderWidth = Metrics.Border, BorderColor = Palette.LineStrong, SelectIndex = 1,
        AccessibleName = "Report date range", TabStop = true
    };
    public ReportToolbar(ClinicDatePicker from, ClinicDatePicker to, AppButton refresh, AppButton export)
    {
        Name = "reportToolbar"; Dock = DockStyle.Top; AutoSize = true; WrapContents = true;
        BackColor = Palette.Canvas; Margin = Padding.Empty; Theme.MarkPrimitive(Ranges);
        foreach (var text in new[] { "This week", "This month", "Last 30 days", "Custom" })
            Ranges.Items.Add(new AntdUI.SegmentedItem { Text = text });
        foreach (var (picker, caption) in new[] { (from, "From"), (to, "To") })
        {
            var field = UiFactory.Field(picker, caption, FieldKind.Date); field.Width = Metrics.ReportDateWidth;
            Controls.Add(field);
        }
        refresh.Width = refresh.MinimumSize.Width; export.Width = export.MinimumSize.Width;
        Controls.AddRange([Ranges, _spacer, refresh, export]); ToolbarLayout.Attach(this, trimEnd: true);
        Layout += (_, _) => AlignActions();
    }
    private void AlignActions()
    {
        if (_arranging) return;
        _arranging = true;
        try
        {
            var required = Controls.Cast<Control>().Where(c => c != _spacer).Sum(c => c.Width + c.Margin.Horizontal);
            _spacer.Visible = ClientSize.Width >= required + Space.Sm;
            _spacer.Width = Math.Max(0, ClientSize.Width - required - _spacer.Margin.Horizontal);
        }
        finally { _arranging = false; }
    }
}
