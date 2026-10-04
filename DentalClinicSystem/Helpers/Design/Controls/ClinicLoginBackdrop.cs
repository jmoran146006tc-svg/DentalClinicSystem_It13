using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DentalClinicSystem.Helpers.Design.Controls;

public static class LoginArtwork
{
    private static readonly Lazy<Image> PhotoResource = new(() => Load("dentist.jpg"));
    private static readonly Lazy<Image> MarkResource = new(() => Load("LoginMark.png"));
    public static Image Photo => PhotoResource.Value;
    public static Image Mark => MarkResource.Value;
    public static double PhotoAspect => (double)Photo.Width / Photo.Height;
    private static Image Load(string name)
    {
        using var stream = typeof(LoginArtwork).Assembly.GetManifestResourceStream($"DentalClinicSystem.Resources.{name}")
            ?? throw new InvalidOperationException($"Missing embedded login image: {name}");
        using var source = Image.FromStream(stream);
        return (Image)source.Clone();
    }
}

// Repainting only blits. Photo composition and the small blur belong to this
// size/DPI cache; neither runs during entrance, focus or feedback animation.
public sealed class ClinicLoginBackdrop : DesignControl
{
    private Bitmap? _composite, _blurred;
    private (Size Size, int Dpi) _cacheKey;
    public RectangleF PhotoBounds { get; private set; }
    public event EventHandler? CompositionChanged;
    public ClinicLoginBackdrop() { Dock = DockStyle.Fill; AccessibleName = "Dental Care clinic"; }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Rebuild(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Rebuild(); }
    private void Rebuild()
    {
        if (_composite is not null && _cacheKey == (Size, DeviceDpi)) return;
        _composite?.Dispose(); _blurred?.Dispose(); _composite = _blurred = null;
        if (Width <= 0 || Height <= 0) return;
        _cacheKey = (Size, DeviceDpi);
        _composite = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(_composite))
        {
            DesignPaint.Prepare(graphics);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var photo = LoginArtwork.Photo;
            var scale = Math.Max((float)Width / photo.Width, (float)Height / photo.Height);
            PhotoBounds = new((Width - photo.Width * scale) / 2, (Height - photo.Height * scale) / 2, photo.Width * scale, photo.Height * scale);
            graphics.DrawImage(photo, PhotoBounds);
            using var tint = new SolidBrush(Palette.LoginTint);
            graphics.FillRectangle(tint, ClientRectangle);
            using var ellipse = new GraphicsPath();
            var surround = ClientRectangle; surround.Inflate(Width / 4, Height / 4);
            ellipse.AddEllipse(surround);
            using var vignette = new PathGradientBrush(ellipse)
            {
                CenterPoint = new(Width / 2f, Height / 2f), CenterColor = Palette.WithAlpha(Palette.Ink900, 0),
                SurroundColors = [Palette.LoginVignette]
            };
            graphics.FillRectangle(vignette, ClientRectangle);
        }
        _blurred = new Bitmap(Math.Max(1, Width / Metrics.LoginBlurScale), Math.Max(1, Height / Metrics.LoginBlurScale), PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(_blurred))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(_composite, new Rectangle(Point.Empty, _blurred.Size));
        }
        Blur(_blurred);
        Invalidate(); CompositionChanged?.Invoke(this, EventArgs.Empty);
    }
    private static void Blur(Bitmap image)
    {
        var data = image.LockBits(new(Point.Empty, image.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            var input = new byte[data.Stride * data.Height]; var output = new byte[input.Length];
            Marshal.Copy(data.Scan0, input, 0, input.Length);
            for (var pass = 0; pass < Metrics.LoginBlurPasses; pass++)
            {
                Filter(input, output, true); Filter(output, input, false);
            }
            Marshal.Copy(input, 0, data.Scan0, input.Length);
            void Filter(byte[] source, byte[] target, bool horizontal)
            {
                var radius = Metrics.LoginBlurRadius;
                for (var y = 0; y < image.Height; y++) for (var x = 0; x < image.Width; x++)
                    for (var channel = 0; channel < 4; channel++)
                    {
                        var sum = 0;
                        for (var offset = -radius; offset <= radius; offset++)
                        {
                            var sx = horizontal ? Math.Clamp(x + offset, 0, image.Width - 1) : x;
                            var sy = horizontal ? y : Math.Clamp(y + offset, 0, image.Height - 1);
                            sum += source[sy * data.Stride + sx * 4 + channel];
                        }
                        target[y * data.Stride + x * 4 + channel] = (byte)(sum / (radius * 2 + 1));
                    }
            }
        }
        finally { image.UnlockBits(data); }
    }
    public Bitmap CreateWashedCrop(Rectangle bounds, Color wash, bool blurred)
    {
        Rebuild();
        var result = new Bitmap(Math.Max(1, bounds.Width), Math.Max(1, bounds.Height), PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(result);
        var source = blurred ? _blurred : _composite;
        if (source is not null)
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var sx = (float)source.Width / Width; var sy = (float)source.Height / Height;
            graphics.DrawImage(source, new Rectangle(Point.Empty, result.Size), new RectangleF(bounds.X * sx, bounds.Y * sy, bounds.Width * sx, bounds.Height * sy), GraphicsUnit.Pixel);
        }
        else graphics.Clear(Palette.Surface);
        using var brush = new SolidBrush(wash); graphics.FillRectangle(brush, new Rectangle(Point.Empty, result.Size));
        return result;
    }
    public void PaintCrop(Graphics graphics, Rectangle destination, Point origin)
    {
        if (_composite is null) { graphics.Clear(Palette.Canvas); return; }
        graphics.DrawImage(_composite, destination, new Rectangle(origin, destination.Size), GraphicsUnit.Pixel);
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (_composite is not null) e.Graphics.DrawImage(_composite, ClientRectangle, 0, 0, _composite.Width, _composite.Height, GraphicsUnit.Pixel);
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { _composite?.Dispose(); _blurred?.Dispose(); }
        base.Dispose(disposing);
    }
}
