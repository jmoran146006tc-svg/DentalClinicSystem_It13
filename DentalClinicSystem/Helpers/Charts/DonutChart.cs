using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
namespace DentalClinicSystem.Helpers.Charts;

public sealed class DonutChart : DesignControl
{
    private readonly ChartState _state;
    private Rectangle _ring;
    private IReadOnlyList<ChartSweep> _sweeps = [];
    public event Action<IReadOnlyList<double>>? ValuesChanged;
    public DonutChart()
    {
        Size = new(Metrics.FormWidth, Metrics.ChartHeight);
        _state = new(this, "Appointments by status"); _state.Frame += Rebuild; SizeChanged += (_, _) => Rebuild();
    }
    public void SetData(IReadOnlyList<ChartDatum> data, Func<double, string> format, string emptyMessage) => _state.SetData(data, format, emptyMessage);
    internal void SetAnimationProgress(float progress) => _state.ApplyProgress(progress);
    private void Rebuild()
    {
        _ring = ChartGeometry.Donut(ClientSize); _sweeps = ChartMath.SweepAngles(_state.Values); ValuesChanged?.Invoke(_state.Values);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_state.Begin(e.Graphics) || _ring.Width <= 0) return;
        var displayed = _state.First ? _state.Progress * 360 : 360;
        for (var i = 0; i < _sweeps.Count; i++)
        {
            var segment = _sweeps[i]; var sweep = (float)Math.Clamp(displayed - segment.Start, 0, segment.Sweep);
            if (sweep <= 0) continue;
            using var brush = new SolidBrush(_state.Color(i)); e.Graphics.FillPie(brush, _ring, (float)segment.Start - 90, sweep);
        }
        DrawSeparators(e.Graphics, displayed);
        var hole = Rectangle.Inflate(_ring, -(int)(_ring.Width * (1 - Metrics.DonutHoleRatio) / 2), -(int)(_ring.Height * (1 - Metrics.DonutHoleRatio) / 2));
        using var surface = new SolidBrush(Palette.Surface); e.Graphics.FillEllipse(surface, hole);
        var total = _state.Values.Sum();
        TextRenderer.DrawText(e.Graphics, double.IsFinite(total) ? _state.Format(total) : "Total exceeds chart scale", Typography.Heading, hole, Palette.Ink900, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
    }
    private void DrawSeparators(Graphics graphics, float displayed)
    {
        if (_sweeps.Count(segment => segment.Sweep > 0) < 2) return;
        using var gap = new Pen(Palette.Surface, Metrics.Scale(this, Metrics.ChartSegmentGap));
        var center = new PointF(_ring.Left + _ring.Width / 2f, _ring.Top + _ring.Height / 2f);
        foreach (var segment in _sweeps.Where(segment => segment.Sweep > 0 && segment.Start < displayed))
        {
            var angle = (segment.Start - 90) * Math.PI / 180;
            graphics.DrawLine(gap, center, new PointF(center.X + (float)Math.Cos(angle) * _ring.Width / 2,
                center.Y + (float)Math.Sin(angle) * _ring.Height / 2));
        }
    }
    protected override void OnMouseMove(MouseEventArgs e) { base.OnMouseMove(e); _state.Hover(ChartGeometry.Segment(e.Location, _ring, _sweeps)); }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
}
