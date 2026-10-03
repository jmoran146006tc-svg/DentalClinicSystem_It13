namespace DentalClinicSystem.Helpers.Design;

public enum ElevationLevel { E0, E1, E2, E3 }
public sealed record ShadowLayer(int Offset, int Spread, float Opacity);

public static class Elevation
{
    public static IReadOnlyList<ShadowLayer> Layers(ElevationLevel level) => level switch
    {
        ElevationLevel.E1 => [new(1, 2, .04f), new(4, 6, .06f)],
        ElevationLevel.E2 => [new(2, 4, .06f), new(8, 10, .10f)],
        ElevationLevel.E3 => [new(4, 6, .08f), new(16, 18, .14f)],
        _ => []
    };
    public static int Padding(ElevationLevel level) => level switch { ElevationLevel.E1 => Space.Md, ElevationLevel.E2 => Space.Xl, ElevationLevel.E3 => Space.Xxxl, _ => 0 };
}

public static class ShadowCache
{
    private static readonly Dictionary<(Size Size, int Radius, ElevationLevel Level, int Dpi), Bitmap> Cache = [];
    public static Bitmap Get(Size size, int radius, ElevationLevel level, int dpi)
    {
        var key = (size, radius, level, dpi);
        if (Cache.TryGetValue(key, out var cached)) return cached;
        var bitmap = new Bitmap(Math.Max(1, size.Width), Math.Max(1, size.Height));
        using var graphics = Graphics.FromImage(bitmap);
        DesignPaint.Prepare(graphics);
        var scale = dpi / 96f;
        var inset = Elevation.Padding(level) * scale;
        var bounds = new RectangleF(inset, inset, size.Width - 2 * inset, size.Height - 2 * inset);
        foreach (var layer in Elevation.Layers(level))
        {
            for (var spread = layer.Spread; spread > 0; spread--)
            {
                var rect = bounds;
                rect.Offset(0, layer.Offset * scale);
                rect.Inflate(spread * scale, spread * scale);
                DesignPaint.Surface(graphics, rect, radius * scale + spread * scale, Palette.WithAlpha(Palette.Ink900, layer.Opacity / layer.Spread));
            }
        }
        if (Cache.Count >= Metrics.CacheLimit)
        {
            var oldest = Cache.First(); Cache.Remove(oldest.Key); oldest.Value.Dispose();
        }
        Cache.Add(key, bitmap);
        return bitmap;
    }
    public static void Clear()
    {
        foreach (var bitmap in Cache.Values) bitmap.Dispose();
        Cache.Clear();
    }
}
