using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.UiTests;

public sealed class MotionTests
{
    [Fact]
    public void EasingsHaveEndpointsMonotonicCubicAndSettledOvershoot()
    {
        foreach (var easing in Enum.GetValues<Easing>()) { Assert.Equal(0, Easings.Evaluate(easing, 0)); Assert.Equal(1, Easings.Evaluate(easing, 1)); }
        var values = Enumerable.Range(0, 101).Select(i => Easings.Evaluate(Easing.EaseOutCubic, i / 100f)).ToArray();
        Assert.Equal(values.Order().ToArray(), values); Assert.True(Easings.Evaluate(Easing.EaseOutBack, .7f) > 1);
    }
    [Fact]
    public void AnimatorUsesElapsedTimeReplacesCurrentValueAndStopsWhenIdle() => UiThread.Run(() =>
    {
        MotionSystem.Enabled = true; long now = 0; float value = 0; var done = 0;
        using var animator = new Animator(() => now); using var form = new Form(); using var owner = new Panel { Dock = DockStyle.Fill }; form.Controls.Add(owner); UiThread.Show(form);
        animator.Run(owner, "value", 0, 100, MotionSystem.Long, Easing.Linear, current => value = current, () => done++);
        now = 300; animator.Step(); Assert.Equal(50, value);
        animator.Run(owner, "value", 0, 200, MotionSystem.Long, Easing.Linear, current => value = current, () => done++); Assert.Equal(50, value);
        now = 600; animator.Step(); Assert.Equal(125, value);
        now = 900; animator.Step(); animator.Step(); Assert.Equal(200, value); Assert.Equal(1, done); Assert.False(animator.IsRunning); Assert.Equal(0, animator.ActiveCount);
        animator.Run(owner, "dispose", 0, 1, MotionSystem.Long, Easing.Linear, _ => { }); owner.Dispose(); Assert.False(animator.IsRunning);
    });
    [Fact]
    public void ReducedMotionAndAncestorHidingSettleImmediately() => UiThread.Run(() =>
    {
        using var form = new Form(); using var parent = new Panel { Dock = DockStyle.Fill }; using var owner = new Panel { Dock = DockStyle.Fill }; form.Controls.Add(parent); parent.Controls.Add(owner); UiThread.Show(form);
        using var animator = new Animator(() => 0); float value = 0;
        animator.Run(owner, "reduced", 0, 1, MotionSystem.Long, Easing.Linear, number => value = number); Assert.Equal(1, value); Assert.False(animator.IsRunning);
        MotionSystem.Enabled = true; animator.Run(owner, "hidden", 0, 10, MotionSystem.Long, Easing.Linear, number => value = number);
        parent.Hide(); Assert.Equal(10, value); Assert.Equal(0, animator.ActiveCount); Assert.False(animator.IsRunning);
        Assert.Equal(Palette.Surface, Theme.Lerp(Palette.Surface, Palette.Brand, 0)); Assert.Equal(Palette.Brand, Theme.Lerp(Palette.Surface, Palette.Brand, 1));
        var middle = Theme.Lerp(Palette.Surface, Palette.Brand, .5f); Assert.InRange(middle.R, Math.Min(Palette.Surface.R, Palette.Brand.R), Math.Max(Palette.Surface.R, Palette.Brand.R));
        Assert.Same(ShadowCache.Get(new(300, 200), Metrics.CardRadius, ElevationLevel.E1, 96), ShadowCache.Get(new(300, 200), Metrics.CardRadius, ElevationLevel.E1, 96));
    });
}
