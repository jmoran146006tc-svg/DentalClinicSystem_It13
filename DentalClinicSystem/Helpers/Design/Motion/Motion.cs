namespace DentalClinicSystem.Helpers.Design.Motion;

public static class Motion
{
    private static bool? _enabled;
    public static bool Enabled { get => _enabled ?? SystemInformation.UIEffectsEnabled; set => _enabled = value; }
    public static void UseSystemPreference() => _enabled = null;
    public static Animator Animator { get; } = new(() => Environment.TickCount64);
    public static TimeSpan Instant => TimeSpan.FromMilliseconds(90);
    public static TimeSpan Fast => TimeSpan.FromMilliseconds(150);
    public static TimeSpan Base => TimeSpan.FromMilliseconds(220);
    public static TimeSpan Slow => TimeSpan.FromMilliseconds(320);
    public static TimeSpan Long => TimeSpan.FromMilliseconds(600);
    public static TimeSpan LoadingDelay => TimeSpan.FromMilliseconds(250);
    public static TimeSpan ToastLifetime => TimeSpan.FromMilliseconds(3500);
    public static TimeSpan ShimmerPeriod => TimeSpan.FromMilliseconds(1400);
    public static TimeSpan SpinnerPeriod => TimeSpan.FromMilliseconds(900);
    public static TimeSpan RowFlash => TimeSpan.FromMilliseconds(900);
    public const int FrameInterval = 15, StaggerStep = 30, MaxStagger = 10;
#if DEBUG
    public static float TimeScale { get; set; } = 1;
#endif
    internal static double Duration(TimeSpan duration)
    {
#if DEBUG
        return Math.Max(1, duration.TotalMilliseconds * Math.Clamp(TimeScale, 1, 4));
#else
        return Math.Max(1, duration.TotalMilliseconds);
#endif
    }
    public static void Shake(Control owner)
    {
        if (!Enabled || owner.Parent is null) return;
        var start = owner.Left;
        Animator.Run(owner, "shake", 0, 1, Slow, Easing.Linear,
            t => owner.Left = start + (int)(Metrics.Scale(owner, Metrics.Shake) * Math.Sin(t * Math.PI * 6) * (1 - t)),
            () => owner.Left = start);
    }
}
