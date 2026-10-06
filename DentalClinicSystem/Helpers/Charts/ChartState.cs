using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Charts;

// Shared data, accessibility and motion; each control retains its own geometry and renderer.
internal sealed class ChartState : IMessageFilter
{
    private readonly Control _owner;
    private double[] _previous = [], _current = [];
    private float[] _opacity = [];
    private bool _stagger;
    private bool _tabPending, _keyboardFocus;
    public IReadOnlyList<ChartDatum> Data { get; private set; } = [];
    public IReadOnlyList<double> Values => _current;
    public Func<double, string> Format { get; private set; } = value => value.ToString("N0");
    public string EmptyMessage { get; private set; } = "No data in this range";
    public float Progress { get; private set; } = 1;
    public bool First { get; private set; }
    public bool Empty => !Data.Any(row => row.Value > 0);
    public int Hovered { get; private set; } = -1;
    public event Action? Frame;

    public ChartState(Control owner, string title)
    {
        _owner = owner; owner.AccessibleName = title; owner.AccessibleRole = AccessibleRole.Graphic; owner.TabStop = true;
        Application.AddMessageFilter(this);
        owner.Disposed += (_, _) => Application.RemoveMessageFilter(this);
        owner.GotFocus += (_, _) => { _keyboardFocus = _tabPending; _tabPending = false; owner.Invalidate(); };
        owner.LostFocus += (_, _) => { _keyboardFocus = false; owner.Invalidate(); };
        owner.MouseLeave += (_, _) => Hover(-1);
        owner.KeyDown += (_, e) =>
        {
            _keyboardFocus = true; owner.Invalidate();
            if (Data.Count == 0) return;
            if (e.KeyCode is Keys.Left or Keys.Up or Keys.Right or Keys.Down)
            { Hover(Math.Clamp(Hovered + (e.KeyCode is Keys.Left or Keys.Up ? -1 : 1), 0, Data.Count - 1)); e.Handled = true; }
        };
        owner.MouseDown += (_, _) => { _tabPending = _keyboardFocus = false; owner.Focus(); owner.Invalidate(); };
    }
    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == 0x0100 && (Keys)m.WParam.ToInt32() == Keys.Tab && _owner.IsHandleCreated)
        {
            _tabPending = true;
            _owner.BeginInvoke(() => _tabPending = false);
        }
        return false;
    }

    public void SetData(IReadOnlyList<ChartDatum> data, Func<double, string> format, string emptyMessage, bool stagger = false)
    {
        var old = Data.Select((row, i) => (row.Label, Value: _current[i])).GroupBy(row => row.Label)
            .ToDictionary(group => group.Key, group => group.First().Value);
        First = Empty; _stagger = stagger && First;
        MotionSystem.Animator.Cancel(_owner, "chart-data"); MotionSystem.Animator.Cancel(_owner, "chart-hover");
        Data = data.Select(row => row with { Value = ChartMath.NonNegative(row.Value) }).ToArray();
        Format = format; EmptyMessage = emptyMessage; Hovered = -1;
        _previous = Data.Select(row => old.GetValueOrDefault(row.Label)).ToArray(); _current = _previous.ToArray();
        _opacity = Enumerable.Repeat(1f, Data.Count).ToArray();
        _owner.AccessibleDescription = Empty ? emptyMessage : string.Join(", ", Data.Select(row => $"{row.Label} {format(row.Value)}"));
        var duration = MotionSystem.Long + TimeSpan.FromMilliseconds(_stagger ? Math.Min(Math.Max(0, Data.Count - 1), MotionSystem.MaxStagger - 1) * MotionSystem.StaggerStep : 0);
        MotionSystem.Animator.Run(_owner, "chart-data", 0, 1, duration, Easing.EaseOutCubic, ApplyProgress);
    }

    internal void ApplyProgress(float progress)
    {
        Progress = Math.Clamp(progress, 0, 1);
        for (var i = 0; i < Data.Count; i++)
        {
            var delay = _stagger ? Math.Min(i, MotionSystem.MaxStagger - 1) * MotionSystem.StaggerStep / (float)MotionSystem.Long.TotalMilliseconds : 0;
            var local = Math.Clamp(Progress * (1 + (_stagger ? Math.Min(Math.Max(0, Data.Count - 1), MotionSystem.MaxStagger - 1) * MotionSystem.StaggerStep / (float)MotionSystem.Long.TotalMilliseconds : 0)) - delay, 0, 1);
            _current[i] = _previous[i] * (1 - local) + Data[i].Value * local;
        }
        Frame?.Invoke(); _owner.Invalidate();
    }

    public void Hover(int index)
    {
        if (Hovered == index) return;
        Hovered = index;
        Tooltips.Attach(_owner, index >= 0 ? $"{Data[index].Label}: {Format(Data[index].Value)}" : string.Empty);
        var from = _opacity.ToArray(); MotionSystem.Animator.Cancel(_owner, "chart-hover");
        MotionSystem.Animator.Run(_owner, "chart-hover", 0, 1, MotionSystem.Fast, Easing.EaseOutCubic, progress =>
        {
            for (var i = 0; i < _opacity.Length; i++)
            {
                var destination = index < 0 || i == index ? 1 : Metrics.ChartDimOpacity;
                _opacity[i] = from[i] + (destination - from[i]) * Math.Clamp(progress, 0, 1);
            }
            _owner.Invalidate();
        });
    }
    public Color Color(int index) => Theme.Lerp(Palette.Surface, Data[index].Color, _opacity[index]);
    public bool Begin(Graphics graphics)
    {
        if (_owner.Width <= 0 || _owner.Height <= 0) return false;
        DesignPaint.Prepare(graphics); graphics.Clear(Palette.Surface);
        if (_owner.Focused && _keyboardFocus) DesignPaint.Surface(graphics, _owner.ClientRectangle, Metrics.ControlRadius, Palette.Surface, Palette.Brand);
        if (!Empty) return true;
        TextRenderer.DrawText(graphics, EmptyMessage, Typography.Caption, _owner.ClientRectangle, Palette.Ink500, DesignPaint.TextFlags | TextFormatFlags.HorizontalCenter);
        return false;
    }
}
