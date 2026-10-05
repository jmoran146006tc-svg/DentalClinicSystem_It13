using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
namespace DentalClinicSystem.Helpers.Charts;

public sealed class BarChart : DesignControl
{
    private readonly ChartState _state;
    private Rectangle _plot;
    private Rectangle[] _bars = [];
    private ChartTicks _ticks = new([], 0);
    public BarChart()
    {
        Size = new(Metrics.FormWidth, Metrics.ChartHeight);
        _state = new(this, "Top treatment types"); _state.Frame += Rebuild; SizeChanged += (_, _) => Rebuild();
    }
    public void SetData(IReadOnlyList<ChartDatum> data, Func<double, string> format, string emptyMessage) => _state.SetData(data, format, emptyMessage, true);
    internal void SetAnimationProgress(float progress) => _state.ApplyProgress(progress);
    private void Rebuild()
    {
        _plot = ChartGeometry.Plot(ClientSize, Metrics.Scale(this, Metrics.ChartLabelWidth), Space.Xl);
        _plot.Width = Math.Max(0, _plot.Width - Metrics.Scale(this, Metrics.ChartAxisWidth));
        _ticks = ChartMath.NiceTicks(0, _state.Data.Select(row => row.Value).DefaultIfEmpty().Max());
        var lengths = ChartMath.BarLengths(_state.Values, _ticks.Max, _plot.Width);
        _bars = lengths.Select((length, i) => ChartGeometry.Bar(_plot, i, lengths.Count, length)).ToArray();
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_state.Begin(e.Graphics) || _plot.Width <= 0 || _plot.Height <= 0) return;
        using var grid = new Pen(Palette.Line, Metrics.Border);
        foreach (var tick in _ticks.Values)
        {
            var x = (float)ChartMath.Scale(tick, (0, _ticks.Max), (_plot.Left, _plot.Right));
            e.Graphics.DrawLine(grid, x, _plot.Top, x, _plot.Bottom);
        }
        for (var i = 0; i < _bars.Length; i++) DrawBar(e.Graphics, i);
    }
    private void DrawBar(Graphics graphics, int index)
    {
        var bar = _bars[index]; var datum = _state.Data[index];
        DesignPaint.Surface(graphics, bar, Metrics.ControlRadius, _state.Color(index));
        TextRenderer.DrawText(graphics, datum.Label, Typography.Caption,
            new Rectangle(Space.Xs, bar.Top, Math.Max(0, _plot.Left - Space.Sm), bar.Height), Palette.Ink500, DesignPaint.TextFlags);
        TextRenderer.DrawText(graphics, _state.Format(_state.Values[index]), Typography.Caption,
            new Rectangle(bar.Right + Space.Xs, bar.Top, Math.Max(0, Width - bar.Right - Space.Sm), bar.Height), Palette.Ink500, DesignPaint.TextFlags);
    }
    protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); _state.Hover(Array.FindIndex(_bars, bar => bar.Contains(e.Location))); }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
}
