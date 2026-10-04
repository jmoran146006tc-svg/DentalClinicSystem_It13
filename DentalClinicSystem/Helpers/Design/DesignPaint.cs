using System.Drawing.Drawing2D;
using System.Reflection;

namespace DentalClinicSystem.Helpers.Design;

public static class DesignPaint
{
    private static readonly Dictionary<(RectangleF, float), GraphicsPath> Paths = [];
    private static readonly MethodInfo? SetStyle = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
    public const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;

    public static void Enable(Control control) => SetStyle?.Invoke(control, [ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true]);
    public static Color ParentBackground(Control control)
    {
        for (var parent = control.Parent; parent is not null; parent = parent.Parent)
            if (parent.BackColor.A == 255) return parent.BackColor;
        return Palette.Canvas;
    }
    public static void Prepare(Graphics graphics)
    {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
    }
    public static void Begin(Graphics graphics, Control control)
    {
        graphics.Clear(ParentBackground(control));
        Prepare(graphics);
    }
    public static GraphicsPath RoundedRect(RectangleF rect, float radius)
    {
        radius = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
        var key = (rect, radius);
        if (!Paths.TryGetValue(key, out var path))
        {
            if (Paths.Count >= Metrics.CacheLimit) ClearPaths();
            path = Build(rect, radius);
            Paths.Add(key, path);
        }
        return (GraphicsPath)path.Clone();
    }
    private static GraphicsPath Build(RectangleF rect, float radius)
    {
        var path = new GraphicsPath();
        if (rect.Width <= 0 || rect.Height <= 0) return path;
        if (radius <= 0) { path.AddRectangle(rect); return path; }
        var diameter = radius * 2;
        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }
    public static void Surface(Graphics graphics, RectangleF rect, float radius, Color fill, Color? border = null)
    {
        if (rect.Width <= 0 || rect.Height <= 0) return;
        Prepare(graphics);
        rect.Inflate(-Metrics.Border / 2f, -Metrics.Border / 2f);
        using var path = RoundedRect(rect, radius);
        using var brush = new SolidBrush(fill);
        graphics.FillPath(brush, path);
        if (border is Color line) { using var pen = new Pen(line, Metrics.Border); graphics.DrawPath(pen, path); }
    }
    public static void ClearPaths()
    {
        foreach (var path in Paths.Values) path.Dispose();
        Paths.Clear();
    }
}
