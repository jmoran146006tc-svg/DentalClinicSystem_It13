using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class PageHeader : TableLayoutPanel
{
    public PageHeader(string title, string? subtitle = null, params Control[] actions)
    {
        Theme.MarkPrimitive(this);
        ColumnCount = 2; RowCount = 1; Dock = DockStyle.Top; AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink; MinimumSize = new(0, Typography.Title.Height + Typography.Caption.Height + Space.Lg); BackColor = Palette.Canvas; Margin = new Padding(0, 0, 0, Space.Lg);
        ColumnStyles.Add(new(SizeType.Percent, 100)); ColumnStyles.Add(new(SizeType.AutoSize));
        var copy = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Palette.Canvas, Margin = Padding.Empty };
        copy.Controls.Add(new Label { Text = title, AutoSize = true, Font = Typography.Title, ForeColor = Palette.Ink900, Margin = Padding.Empty });
        if (subtitle is not null)
        {
            var caption = new Label { Text = subtitle, AutoSize = true, Font = Typography.Caption, ForeColor = Palette.Ink500, Margin = new Padding(0, Space.Xs, 0, 0) };
            copy.Controls.Add(caption);
            copy.SizeChanged += (_, _) => caption.MaximumSize = new(Math.Max(1, copy.Width), 0);
        }
        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Palette.Canvas };
        buttons.Controls.AddRange(actions); Controls.Add(copy, 0, 0); Controls.Add(buttons, 1, 0);
    }
}

public sealed class EmptyState : TableLayoutPanel
{
    public EmptyState(string title = "No records yet", string message = "Records will appear here when they are added.", IconKind icon = IconKind.Info, Control? action = null)
    {
        Theme.MarkPrimitive(this);
        ColumnCount = 1; RowCount = 4; Height = Metrics.EmptyHeight; Dock = DockStyle.Fill; BackColor = Palette.Surface; Padding = new Padding(Space.Xl);
        ColumnStyles.Add(new(SizeType.Percent, 100));
        var illustration = new IconTile(icon) { Anchor = AnchorStyles.None, Size = new(Metrics.NavHeight, Metrics.NavHeight) };
        Controls.Add(illustration, 0, 0);
        Controls.Add(new Label { Text = title, AutoSize = true, Anchor = AnchorStyles.None, Font = Typography.Heading, ForeColor = Palette.Ink900 }, 0, 1);
        var caption = new Label { Text = message, AutoSize = true, Anchor = AnchorStyles.None, Font = Typography.Caption, ForeColor = Palette.Ink500, TextAlign = ContentAlignment.MiddleCenter };
        SizeChanged += (_, _) => caption.MaximumSize = new(Math.Max(1, ClientSize.Width - Padding.Horizontal - Space.Sm), 0);
        Controls.Add(caption, 0, 2);
        if (action is not null) { action.Anchor = AnchorStyles.None; Controls.Add(action, 0, 3); }
    }
}

public sealed class IconTile : DesignControl
{
    public IconKind Icon { get; }
    public IconTile(IconKind icon) { Icon = icon; Size = new(Metrics.NavHeight, Metrics.NavHeight); AccessibleName = icon.ToString(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 0 || Height <= 0) return;
        DesignPaint.Begin(e.Graphics, this);
        DesignPaint.Surface(e.Graphics, ClientRectangle, Metrics.ControlRadius, Palette.BrandSoft);
        Icons.Draw(e.Graphics, Icon, Rectangle.Inflate(ClientRectangle, -Space.Sm, -Space.Sm), Palette.BrandSoftText);
    }
}

public sealed class KpiCard : RoundedPanel
{
    private readonly Label _value = new() { Dock = DockStyle.Fill, Font = Typography.KpiNumber, ForeColor = Palette.Ink900, Text = "0" };
    private double _current;
    public KpiCard(string title, string subtitle = "", IconKind icon = IconKind.Reports) : base(null, ElevationLevel.E1)
    {
        Height = Metrics.KpiHeight; Width = Metrics.FormWidth;
        var copy = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Palette.Surface };
        copy.RowStyles.Add(new(SizeType.AutoSize)); copy.RowStyles.Add(new(SizeType.Percent, 100)); copy.RowStyles.Add(new(SizeType.AutoSize));
        _value.MinimumSize = new(0, Typography.KpiNumber.Height + Space.Sm);
        copy.Controls.Add(new Label { Text = title, AutoSize = true, Font = Typography.Caption, ForeColor = Palette.Ink500 }, 0, 0);
        copy.Controls.Add(_value, 0, 1); copy.Controls.Add(new Label { Text = subtitle, AutoSize = true, Font = Typography.Caption, ForeColor = Palette.Ink500 }, 0, 2);
        var tile = new IconTile(icon) { Anchor = AnchorStyles.Top | AnchorStyles.Left };
        copy.Padding = new Padding(0, 0, Metrics.NavHeight + Space.Sm, 0);
        Content.SizeChanged += (_, _) => tile.Location = new(Math.Max(0, Content.ClientSize.Width - tile.Width), 0);
        Content.Controls.Add(copy); Content.Controls.Add(tile);
        tile.Location = new(Math.Max(0, Content.ClientSize.Width - tile.Width), 0); tile.BringToFront();
        MouseEnter += (_, _) => Elevation = ElevationLevel.E2;
        MouseLeave += (_, _) => Elevation = ElevationLevel.E1;
    }
    public void SetValue(double value, Func<double, string>? format = null, bool animate = true)
    {
        format ??= number => number.ToString("N0");
        if (!animate) { MotionSystem.Animator.Cancel(this, "kpi-value"); _current = value; _value.Text = format(value); return; }
        MotionSystem.Animator.Run(this, "kpi-value", (float)_current, (float)value, MotionSystem.Long, Easing.EaseOutCubic,
            number => { _current = number; _value.Text = format(number); });
    }
}
