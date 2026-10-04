using System.Drawing.Drawing2D;

namespace DentalClinicSystem.Helpers.Design;

public enum IconKind
{
    Dashboard, Patients, Dentist, Appointments, Treatments, Users, Reports, Logout,
    Search, Plus, Close, Check, Warning, Info, ChevronLeft, ChevronRight, ChevronDown,
    Eye, EyeOff, Clock, Edit, Phone, Mail, User, Lock
}

public static class Icons
{
    public static GraphicsPath Build(IconKind kind)
    {
        var path = new GraphicsPath();
        switch (kind)
        {
            case IconKind.Dashboard:
                foreach (var x in new[] { 3, 14 }) foreach (var y in new[] { 3, 14 }) path.AddRectangle(new(x, y, 7, 7));
                break;
            case IconKind.Patients:
            case IconKind.Users:
            case IconKind.User:
                path.AddEllipse(8, 3, 8, 8); path.StartFigure(); path.AddArc(4, 13, 16, 14, 180, 180); break;
            case IconKind.Dentist:
                path.AddBezier(12, 5, 1, -2, 3, 16, 6, 21); path.AddBezier(6, 21, 10, 23, 8, 12, 12, 13);
                path.AddBezier(12, 13, 16, 12, 14, 23, 18, 21); path.AddBezier(18, 21, 21, 16, 23, -2, 12, 5); break;
            case IconKind.Appointments:
                path.AddRectangle(new(3, 5, 18, 16)); Line(path, 3, 10, 21, 10); Line(path, 7, 2, 7, 7); Line(path, 17, 2, 17, 7); break;
            case IconKind.Treatments:
                path.AddRectangle(new(5, 5, 14, 16)); path.AddRectangle(new(9, 2, 6, 5)); Line(path, 8, 12, 16, 12); Line(path, 8, 16, 14, 16); break;
            case IconKind.Reports:
                Line(path, 3, 3, 3, 21); Line(path, 3, 21, 22, 21); path.AddRectangle(new(7, 12, 3, 9)); path.AddRectangle(new(13, 7, 3, 14)); path.AddRectangle(new(19, 3, 3, 18)); break;
            case IconKind.Logout:
                Lines(path, new(10, 3), new(4, 3), new(4, 21), new(10, 21)); Line(path, 9, 12, 21, 12); Lines(path, new(17, 8), new(21, 12), new(17, 16)); break;
            case IconKind.Search:
                path.AddEllipse(3, 3, 13, 13); Line(path, 15, 15, 21, 21); break;
            case IconKind.Plus: Line(path, 12, 4, 12, 20); Line(path, 4, 12, 20, 12); break;
            case IconKind.Close: Line(path, 5, 5, 19, 19); Line(path, 19, 5, 5, 19); break;
            case IconKind.Check: Lines(path, new(4, 12), new(10, 18), new(21, 5)); break;
            case IconKind.Warning:
                Lines(path, new(12, 2), new(22, 21), new(2, 21), new(12, 2)); Line(path, 12, 8, 12, 14); Line(path, 12, 17, 12, 18); break;
            case IconKind.Info:
            case IconKind.Clock:
                path.AddEllipse(2, 2, 20, 20);
                if (kind == IconKind.Clock) Lines(path, new(12, 6), new(12, 12), new(16, 14));
                else { Line(path, 12, 10, 12, 17); Line(path, 12, 6, 12, 7); }
                break;
            case IconKind.ChevronLeft: Lines(path, new(15, 5), new(8, 12), new(15, 19)); break;
            case IconKind.ChevronRight: Lines(path, new(9, 5), new(16, 12), new(9, 19)); break;
            case IconKind.ChevronDown: Lines(path, new(5, 9), new(12, 16), new(19, 9)); break;
            case IconKind.Eye:
            case IconKind.EyeOff:
                path.AddBezier(2, 12, 7, 3, 17, 3, 22, 12); path.AddBezier(22, 12, 17, 21, 7, 21, 2, 12); path.AddEllipse(9, 9, 6, 6);
                if (kind == IconKind.EyeOff) Line(path, 3, 3, 21, 21);
                break;
            case IconKind.Edit:
                Lines(path, new(4, 16), new(16, 4), new(20, 8), new(8, 20), new(3, 21), new(4, 16)); Line(path, 14, 6, 18, 10); break;
            case IconKind.Phone:
                Lines(path, new(4, 3), new(8, 3), new(10, 8), new(7, 10), new(14, 17), new(16, 14), new(21, 16), new(21, 20));
                path.AddBezier(21, 20, 12, 25, -1, 12, 4, 3); break;
            case IconKind.Mail:
                path.AddRectangle(new(2, 5, 20, 14)); Lines(path, new(2, 5), new(12, 13), new(22, 5)); break;
            case IconKind.Lock:
                path.AddRectangle(new(5, 10, 14, 11)); path.StartFigure(); path.AddArc(8, 3, 8, 14, 180, 180); Line(path, 12, 14, 12, 17); break;
        }
        return path;
    }
    public static void Draw(Graphics graphics, IconKind kind, RectangleF bounds, Color color)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        using var path = Build(kind);
        using var matrix = new Matrix();
        var scale = Math.Min(bounds.Width, bounds.Height) / Metrics.IconGrid;
        matrix.Translate(bounds.X, bounds.Y); matrix.Scale(scale, scale); path.Transform(matrix);
        using var pen = new Pen(color, Metrics.IconStroke * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
        DesignPaint.Prepare(graphics); graphics.DrawPath(pen, path);
    }
    private static void Line(GraphicsPath path, float x1, float y1, float x2, float y2) { path.StartFigure(); path.AddLine(x1, y1, x2, y2); }
    private static void Lines(GraphicsPath path, params PointF[] points) { path.StartFigure(); path.AddLines(points); }
}
