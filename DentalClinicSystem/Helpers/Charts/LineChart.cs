using System.Drawing.Drawing2D;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
namespace DentalClinicSystem.Helpers.Charts;

public sealed class LineChart : DesignControl
{
    private readonly ChartState _state;
    private Rectangle _plot;
    private PointF[] _points = [];
    private ChartTicks _ticks = new([], 0);
    public LineChart()
    {
        Size = new(Metrics.FormWidth, Metrics.ChartHeight);
        _state = new(this, "Billed by day"); _state.Frame += Rebuild; SizeChanged += (_, _) => Rebuild();
    }
    public void SetData(IReadOnlyList<ChartDatum> data, Func<double, string> format, string emptyMessage) => _state.SetData(data, format, emptyMessage);
    internal void SetAnimationProgress(float progress) => _state.ApplyProgress(progress);
    private void Rebuild()
    {
        _plot = ChartGeometry.Plot(ClientSize, Metrics.Scale(this, Metrics.ChartAxisWidth), Space.Xl);
        _ticks = ChartMath.NiceTicks(0, _state.Data.Select(row => row.Value).DefaultIfEmpty().Max());
        _points = ChartGeometry.Points(_plot, _state.Values, _ticks.Max);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_state.Begin(e.Graphics) || _plot.Width <= 0 || _plot.Height <= 0) return;
        DrawAxes(e.Graphics); var saved = e.Graphics.Save();
        try
        {
            var width = _plot.Width * (_state.First ? _state.Progress : 1);
            e.Graphics.SetClip(new RectangleF(_plot.Left - Space.Xs, _plot.Top - Space.Xs, width + Space.Sm, _plot.Height + Space.Sm));
            DrawSeries(e.Graphics);
        }
        finally { e.Graphics.Restore(saved); }
    }
    private void DrawAxes(Graphics graphics)
    {
        using var grid = new Pen(Palette.Line, Metrics.Border);
        foreach (var tick in _ticks.Values)
        {
            var y = (int)ChartMath.Scale(tick, (0, _ticks.Max), (_plot.Bottom, _plot.Top));
            graphics.DrawLine(grid, _plot.Left, y, _plot.Right, y);
            TextRenderer.DrawText(graphics, _state.Format(tick), Typography.Caption,
                new Rectangle(0, y - Typography.Caption.Height / 2, _plot.Left - Space.Sm, Typography.Caption.Height), Palette.Ink500, DesignPaint.TextFlags | TextFormatFlags.Right);
        }
        foreach (var (index, bounds) in ChartGeometry.AxisLabels(_plot, _points.Length, Metrics.Scale(this, Metrics.ChartLabelWidth), Typography.Caption.Height))
            TextRenderer.DrawText(graphics, _state.Data[index].Label, Typography.Caption,
                bounds, Palette.Ink500, DesignPaint.TextFlags);
    }
    private void DrawSeries(Graphics graphics)
    {
        if (_points.Length == 0) return;
        if (_points.Length > 1)
        {
            using var area = new GraphicsPath(); area.AddLines(_points); area.AddLine(_points[^1], new(_points[^1].X, _plot.Bottom));
            area.AddLine(new PointF(_points[^1].X, _plot.Bottom), new PointF(_points[0].X, _plot.Bottom)); area.CloseFigure();
            var opacity = _state.First ? Math.Clamp((_state.Progress - Metrics.ChartAreaStart) / (1 - Metrics.ChartAreaStart), 0, 1) : 1;
            using var fill = new SolidBrush(Palette.WithAlpha(Palette.Brand, Metrics.ChartAreaOpacity * opacity)); graphics.FillPath(fill, area);
            for (var i = 1; i < _points.Length; i++) { using var line = new Pen(_state.Color(i), Metrics.FocusRing); graphics.DrawLine(line, _points[i - 1], _points[i]); }
        }
        for (var i = 0; i < _points.Length; i++)
        {
            using var marker = new SolidBrush(_state.Color(i)); var point = _points[i]; graphics.FillEllipse(marker, point.X - Space.Xs, point.Y - Space.Xs, Space.Sm, Space.Sm);
        }
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); _state.Hover(Array.FindIndex(_points, point => Math.Abs(point.X - e.X) <= Space.Sm && Math.Abs(point.Y - e.Y) <= Space.Sm));
    }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
}
