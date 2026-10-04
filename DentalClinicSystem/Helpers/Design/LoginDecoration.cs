using System.Drawing.Drawing2D;

namespace DentalClinicSystem.Helpers.Design;

public enum LoginDecorKind { GlassSquare, GlassCircle, GlassPill, Ring, Arc, Tooth, Sparkle, Cross, Dot, DotGrid }
public sealed record LoginDecorSpec(LoginDecorKind Kind, float X, float Y, int Size, float Rotation = 0);
public sealed record LoginDecorPlacement(LoginDecorSpec Spec, RectangleF Bounds, RectangleF VisibleBounds, RectangleF ShapeBounds);

// Bitmap artwork only: no controls, focus targets or accessibility nodes.
public static class LoginDecoration
{
    public static IReadOnlyList<LoginDecorPlacement> Place(Control owner)
    {
        int S(int value) => Metrics.Scale(owner, value);
        // A stable envelope includes the tallest card (Caps Lock + inline error).
        // This keeps the size/DPI-only cache valid when feedback changes layout.
        var clear = new RectangleF((owner.Width - S(Metrics.LoginCardWidth)) / 2f,
            (owner.Height - S(Metrics.LoginDecorClearHeight)) / 2f, S(Metrics.LoginCardWidth), S(Metrics.LoginDecorClearHeight));
        clear.Inflate(S(Space.Xl), S(Space.Xl));
        var placements = new List<LoginDecorPlacement>();
        foreach (var spec in Metrics.LoginDecorations)
        {
            var width = S(spec.Size);
            var height = spec.Kind == LoginDecorKind.GlassPill ? width * Metrics.LoginDecorPillAspect
                : spec.Kind == LoginDecorKind.DotGrid ? S(Space.Xl + Space.Lg) : width;
            var shape = new RectangleF(-width / 2f, -height / 2f, width, height);
            using var matrix = new Matrix(); matrix.Translate(owner.Width * spec.X, owner.Height * spec.Y); matrix.Rotate(spec.Rotation);
            var points = new[] { new PointF(shape.Left, shape.Top), new PointF(shape.Right, shape.Top), new PointF(shape.Right, shape.Bottom), new PointF(shape.Left, shape.Bottom) };
            matrix.TransformPoints(points);
            var bounds = RectangleF.FromLTRB(points.Min(p => p.X), points.Min(p => p.Y), points.Max(p => p.X), points.Max(p => p.Y));
            // Include stroke and the soft glass shadow in the exclusion/visibility bounds.
            bounds.Inflate(S(Space.Xs), S(Space.Xs));
            var visible = RectangleF.Intersect(bounds, owner.ClientRectangle);
            var minimumSide = Math.Min(S(spec.Size), S(Space.Md));
            if (bounds.IntersectsWith(clear) || visible.Width < minimumSide || visible.Height < minimumSide
                || visible.Width * visible.Height < bounds.Width * bounds.Height * Metrics.LoginDecorMinimumVisibleArea) continue;
            placements.Add(new(spec, bounds, visible, shape));
        }
        return placements.AsReadOnly();
    }
    public static void DrawSoft(Graphics graphics, Control owner)
    {
        DesignPaint.Prepare(graphics);
        Orb(.02f, .05f, Metrics.LoginOrbAquaSize, Palette.DecorOrbAqua);
        Orb(1, 1, Metrics.LoginOrbLilacSize, Palette.DecorOrbLilac);
        Orb(1, .45f, Metrics.LoginOrbAccentSize, Palette.DecorOrbAccent);
        using var wash = new LinearGradientBrush(owner.ClientRectangle, Palette.DecorWashSky, Palette.DecorWashAccent, LinearGradientMode.ForwardDiagonal);
        graphics.FillRectangle(wash, owner.ClientRectangle);
        void Orb(float x, float y, int diameter, Color ink)
        {
            var size = Metrics.Scale(owner, diameter); var center = new PointF(owner.Width * x, owner.Height * y);
            using var path = new GraphicsPath(); path.AddEllipse(center.X - size / 2f, center.Y - size / 2f, size, size);
            using var gradient = new PathGradientBrush(path)
            {
                CenterPoint = center, CenterColor = ink, SurroundColors = [Palette.WithAlpha(ink, 0)],
                Blend = new Blend { Positions = [0, .25f, .5f, .75f, 1], Factors = [0, .10f, .35f, .70f, 1] }
            };
            graphics.FillPath(gradient, path);
        }
    }
    public static void DrawCrisp(Graphics graphics, Control owner, IReadOnlyList<LoginDecorPlacement> placements)
    {
        DesignPaint.Prepare(graphics);
        var scale = owner.DeviceDpi / 96f;
        foreach (var placement in placements)
        {
            var spec = placement.Spec; var rect = placement.ShapeBounds;
            var state = graphics.Save();
            try
            {
                graphics.TranslateTransform(owner.Width * spec.X, owner.Height * spec.Y); graphics.RotateTransform(spec.Rotation);
                using var path = new GraphicsPath();
                switch (spec.Kind)
                {
                    case LoginDecorKind.GlassSquare: using (var rounded = DesignPaint.RoundedRect(rect, Metrics.HeroRadius * scale)) path.AddPath(rounded, false); break;
                    case LoginDecorKind.GlassCircle: case LoginDecorKind.Ring: case LoginDecorKind.Dot: path.AddEllipse(rect); break;
                    case LoginDecorKind.GlassPill: using (var rounded = DesignPaint.RoundedRect(rect, rect.Height / 2)) path.AddPath(rounded, false); break;
                    case LoginDecorKind.Arc: path.AddArc(rect, 220, 220); break;
                    case LoginDecorKind.Tooth:
                        using (var tooth = Icons.Build(IconKind.Dentist))
                        using (var matrix = new Matrix()) { matrix.Translate(rect.Left, rect.Top); matrix.Scale(rect.Width / Metrics.IconGrid, rect.Height / Metrics.IconGrid); tooth.Transform(matrix); path.AddPath(tooth, false); }
                        break;
                    case LoginDecorKind.Sparkle:
                        var inset = rect.Width / 8;
                        path.AddPolygon(new PointF[] { new(0, rect.Top), new(inset, -inset), new(rect.Right, 0), new(inset, inset), new(0, rect.Bottom), new(-inset, inset), new(rect.Left, 0), new(-inset, -inset) }); break;
                    case LoginDecorKind.Cross: path.AddLine(0, rect.Top, 0, rect.Bottom); path.StartFigure(); path.AddLine(rect.Left, 0, rect.Right, 0); break;
                    case LoginDecorKind.DotGrid:
                        var dot = Metrics.Scale(owner, Space.Xs); var gap = Metrics.Scale(owner, Space.Md);
                        for (var y = 0; y < Metrics.LoginDecorGridRows; y++) for (var x = 0; x < Metrics.LoginDecorGridColumns; x++)
                            path.AddEllipse(rect.Left + x * gap, rect.Top + y * gap, dot, dot);
                        break;
                }
                if (spec.Kind is LoginDecorKind.GlassSquare or LoginDecorKind.GlassCircle or LoginDecorKind.GlassPill)
                {
                    // Three translucent silhouettes give a soft, bounded shadow.
                    using var shadow = new SolidBrush(Palette.DecorShadow);
                    for (var offset = Metrics.Border; offset <= Space.Xs; offset += Metrics.Border)
                    { var shadowState = graphics.Save(); graphics.TranslateTransform(0, offset * scale); graphics.FillPath(shadow, path); graphics.Restore(shadowState); }
                    using var fill = new LinearGradientBrush(rect, Palette.DecorGlassWhite, Palette.DecorGlassAccent, LinearGradientMode.ForwardDiagonal);
                    graphics.FillPath(fill, path); using var edge = new Pen(Palette.DecorGlassEdge, Metrics.Border * scale); graphics.DrawPath(edge, path);
                }
                else if (spec.Kind is LoginDecorKind.Sparkle or LoginDecorKind.Dot or LoginDecorKind.DotGrid)
                { using var fill = new SolidBrush(spec.Kind == LoginDecorKind.Sparkle ? (spec.X < .5f ? Palette.DecorSparkle : Palette.DecorSparkleAccent) : Palette.DecorAccent); graphics.FillPath(fill, path); }
                else
                {
                    using var gradient = new LinearGradientBrush(rect, spec.Kind == LoginDecorKind.Tooth ? Palette.DecorTooth : Palette.DecorRing,
                        spec.Kind == LoginDecorKind.Tooth ? Palette.DecorToothWhite : Palette.DecorRingWhite, LinearGradientMode.ForwardDiagonal);
                    using var stroke = new Pen(gradient, Metrics.LoginDecorStroke * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                    graphics.DrawPath(stroke, path);
                }
            }
            finally { graphics.Restore(state); }
        }
    }
}
