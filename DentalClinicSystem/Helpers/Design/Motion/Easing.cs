namespace DentalClinicSystem.Helpers.Design.Motion;

public enum Easing { Linear, EaseOutCubic, EaseInOutCubic, EaseOutBack }

public static class Easings
{
    public static float Evaluate(Easing easing, float value)
    {
        var t = Math.Clamp(value, 0, 1);
        return easing switch
        {
            Easing.EaseOutCubic => 1 - MathF.Pow(1 - t, 3),
            Easing.EaseInOutCubic => t < .5f ? 4 * t * t * t : 1 - MathF.Pow(-2 * t + 2, 3) / 2,
            Easing.EaseOutBack => 1 + 2.5f * MathF.Pow(t - 1, 3) + 1.5f * MathF.Pow(t - 1, 2),
            _ => t
        };
    }
}
