namespace DentalClinicSystem.Helpers.Design.Motion;

public sealed class Animator : IDisposable
{
    private sealed class Animation(Control owner, string key, float from, float to, long start, double duration,
        Easing easing, Action<float> update, Action? done, bool loop, bool visual = true)
    {
        public Control Owner { get; } = owner;
        public string Key { get; } = key;
        public float From { get; } = from;
        public float To { get; } = to;
        public long Start { get; } = start;
        public double Duration { get; } = duration;
        public Easing Easing { get; } = easing;
        public Action<float> Update { get; } = update;
        public Action? Done { get; } = done;
        public bool Loop { get; } = loop;
        public bool Visual { get; } = visual;
        public float Current { get; set; } = from;
        public Color? CurrentColor { get; set; }
    }

    private readonly Func<long> _clock;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = Motion.FrameInterval };
    private readonly Dictionary<(Control, string), Animation> _animations = [];
    private readonly HashSet<Control> _owners = [];
    private readonly Dictionary<Control, Control[]> _parentChains = [];
    private readonly Dictionary<Control, int> _ancestorOwners = [];
    public int ActiveCount => _animations.Count;
    public bool IsRunning => _timer.Enabled;
    public event EventHandler? ActivityChanged;

    public Animator(Func<long> clock)
    {
        _clock = clock;
        _timer.Tick += (_, _) => Step();
    }
    public void Run(Control owner, string key, float from, float to, TimeSpan duration, Easing easing, Action<float> onUpdate, Action? onDone = null)
    {
        if (_animations.TryGetValue((owner, key), out var previous)) from = previous.Current;
        Cancel(owner, key);
        if (owner.IsDisposed) return;
        if (!Motion.Enabled || !owner.Visible || duration <= TimeSpan.Zero)
        {
            onUpdate(to);
            onDone?.Invoke();
            return;
        }
        Add(new(owner, key, from, to, _clock(), Motion.Duration(duration), easing, onUpdate, onDone, false));
        onUpdate(from);
    }
    public void RunColor(Control owner, string key, Color from, Color to, TimeSpan duration, Action<Color> update)
    {
        if (_animations.TryGetValue((owner, key), out var previous) && previous.CurrentColor is Color current) from = current;
        Cancel(owner, key);
        Run(owner, key, 0, 1, duration, Easing.EaseOutCubic, t =>
        {
            var color = Theme.Lerp(from, to, t);
            if (_animations.TryGetValue((owner, key), out var animation)) animation.CurrentColor = color;
            update(color);
        });
    }
    public void Loop(Control owner, string key, TimeSpan period, Action<float> onPhase)
    {
        Cancel(owner, key);
        if (owner.IsDisposed) return;
        onPhase(0);
        if (!Motion.Enabled || !owner.Visible) return;
        Add(new(owner, key, 0, 1, _clock(), Motion.Duration(period), Easing.Linear, onPhase, null, true));
    }
    public void Schedule(Control owner, string key, TimeSpan delay, Action onDone, Action<float>? onProgress = null)
    {
        Cancel(owner, key);
        if (owner.IsDisposed || !owner.Visible) return;
        Add(new(owner, key, 0, 1, _clock(), Math.Max(1, delay.TotalMilliseconds), Easing.Linear, onProgress ?? (_ => { }), onDone, false, false));
    }
    private void Add(Animation animation)
    {
        _animations[(animation.Owner, animation.Key)] = animation;
        if (_owners.Add(animation.Owner))
        {
            animation.Owner.Disposed += OwnerUnavailable;
            animation.Owner.VisibleChanged += OwnerVisibilityChanged;
            animation.Owner.ParentChanged += OwnerHierarchyChanged;
            LinkAncestors(animation.Owner);
        }
        _timer.Start();
        ActivityChanged?.Invoke(this, EventArgs.Empty);
    }
    public void Step()
    {
        var now = _clock();
        foreach (var animation in _animations.Values.ToArray())
        {
            var key = (animation.Owner, animation.Key);
            if (!_animations.TryGetValue(key, out var current) || current != animation) continue;
            if (animation.Owner.IsDisposed) { Cancel(animation.Owner); continue; }
            if (!animation.Owner.Visible || animation.Owner.FindForm()?.WindowState == FormWindowState.Minimized) { Settle(animation.Owner); continue; }
            var progress = Math.Max(0, (now - animation.Start) / animation.Duration);
            if (animation.Visual && !Motion.Enabled) progress = 1;
            var phase = animation.Loop && Motion.Enabled ? (float)(progress % 1) : (float)Math.Min(1, progress);
            animation.Current = animation.From + (animation.To - animation.From) * Easings.Evaluate(animation.Easing, phase);
            var finished = (!animation.Loop && progress >= 1) || (animation.Visual && !Motion.Enabled);
            if (finished) Cancel(animation.Owner, animation.Key);
            animation.Update(animation.Current);
            if (finished) animation.Done?.Invoke();
        }
    }
    public void Cancel(Control owner, string? key = null)
    {
        var count = _animations.Count;
        foreach (var item in _animations.Keys.Where(k => k.Item1 == owner && (key is null || k.Item2 == key)).ToArray()) _animations.Remove(item);
        if (!_animations.Keys.Any(k => k.Item1 == owner) && _owners.Remove(owner))
        {
            owner.Disposed -= OwnerUnavailable;
            owner.VisibleChanged -= OwnerVisibilityChanged;
            owner.ParentChanged -= OwnerHierarchyChanged;
            UnlinkAncestors(owner);
        }
        if (_animations.Count == 0) _timer.Stop();
        if (count != _animations.Count) ActivityChanged?.Invoke(this, EventArgs.Empty);
    }
    private void OwnerUnavailable(object? sender, EventArgs e) { if (sender is Control owner) Cancel(owner); }
    private void OwnerVisibilityChanged(object? sender, EventArgs e) { if (sender is Control { Visible: false } owner) Settle(owner); }
    private void LinkAncestors(Control owner)
    {
        List<Control> parents = [];
        for (var parent = owner.Parent; parent is not null; parent = parent.Parent)
        {
            parents.Add(parent);
            var count = _ancestorOwners.GetValueOrDefault(parent);
            if (count == 0) parent.VisibleChanged += AncestorVisibilityChanged;
            _ancestorOwners[parent] = count + 1;
        }
        _parentChains[owner] = parents.ToArray();
    }
    private void UnlinkAncestors(Control owner)
    {
        if (!_parentChains.Remove(owner, out var parents)) return;
        foreach (var parent in parents)
        {
            var count = _ancestorOwners[parent] - 1;
            if (count == 0) { _ancestorOwners.Remove(parent); parent.VisibleChanged -= AncestorVisibilityChanged; }
            else _ancestorOwners[parent] = count;
        }
    }
    private void OwnerHierarchyChanged(object? sender, EventArgs e)
    {
        if (sender is not Control owner || !_owners.Contains(owner)) return;
        UnlinkAncestors(owner); LinkAncestors(owner);
        if (!owner.Visible) Settle(owner);
    }
    private void AncestorVisibilityChanged(object? sender, EventArgs e)
    {
        foreach (var owner in _owners.Where(owner => !owner.Visible).ToArray()) Settle(owner);
    }
    private void Settle(Control owner)
    {
        var animations = _animations.Values.Where(a => a.Owner == owner && a.Visual).ToArray();
        Cancel(owner);
        foreach (var animation in animations) if (!owner.IsDisposed) animation.Update(animation.Loop ? 0 : animation.To);
    }
    public void Dispose()
    {
        foreach (var owner in _owners.ToArray()) Cancel(owner);
        _timer.Dispose();
    }
}
