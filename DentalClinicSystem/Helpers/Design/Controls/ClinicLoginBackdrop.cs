using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

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
    private Bitmap? _photo, _softDecor, _crispDecor, _composite, _blurred;
    private readonly ImageAttributes _decorAttributes = new();
    private readonly ColorMatrix _decorAlpha = new();
    public float DecorationProgress { get; private set; } = 1;
    public Size DecorationCacheSize => _crispDecor?.Size ?? Size.Empty;
    public IReadOnlyList<LoginDecorPlacement> Decorations { get; private set; } = Array.Empty<LoginDecorPlacement>();
    private (Size Size, int Dpi) _cacheKey;
    public RectangleF PhotoBounds { get; private set; }
    public event EventHandler? CompositionChanged;
    public ClinicLoginBackdrop() { Dock = DockStyle.Fill; AccessibleName = "Dental Care clinic"; VisibleChanged += (_, _) => { if (!Visible) SettleEntrance(); }; }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Rebuild(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Rebuild(); }
    private void Rebuild()
    {
        if (_composite is not null && _cacheKey == (Size, DeviceDpi)) return;
        SettleEntrance(); DisposeCache();
        if (Width <= 0 || Height <= 0) return;
        _cacheKey = (Size, DeviceDpi);
        var photo = RebuildPhoto();
        _softDecor = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(_softDecor)) LoginDecoration.DrawSoft(graphics, this);
        _composite = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(_composite))
        {
            Blit(graphics, photo); Blit(graphics, _softDecor);
        }
        // Capture only photo + luminous gradients. Crisp motifs never enter the frost.
        _blurred = new Bitmap(Math.Max(1, Width / Metrics.LoginBlurScale), Math.Max(1, Height / Metrics.LoginBlurScale), PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(_blurred))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(_composite, new Rectangle(Point.Empty, _blurred.Size));
        }
        Blur(_blurred);
        _crispDecor = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        Decorations = LoginDecoration.Place(this);
        using (var graphics = Graphics.FromImage(_crispDecor)) LoginDecoration.DrawCrisp(graphics, this, Decorations);
        using (var graphics = Graphics.FromImage(_composite)) Blit(graphics, _crispDecor);
        Invalidate(); CompositionChanged?.Invoke(this, EventArgs.Empty);
    }
    private Bitmap RebuildPhoto()
    {
        _photo = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(_photo);
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
        return _photo;
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
    private static void Blit(Graphics graphics, Bitmap bitmap) => graphics.DrawImage(bitmap,
        new Rectangle(Point.Empty, bitmap.Size), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
    public void PaintCrop(Graphics graphics, Rectangle destination, Point origin)
    {
        var source = new Rectangle(origin, destination.Size);
        if (_composite is null) { graphics.Clear(Palette.Canvas); return; }
        if (DecorationProgress >= 1 || _photo is not { } photo || _softDecor is not { } softDecor || _crispDecor is not { } crispDecor)
        {
            graphics.DrawImage(_composite, destination, source, GraphicsUnit.Pixel); return;
        }
        graphics.DrawImage(photo, destination, source, GraphicsUnit.Pixel);
        if (DecorationProgress <= 0) return;
        _decorAlpha.Matrix33 = DecorationProgress; _decorAttributes.SetColorMatrix(_decorAlpha);
        graphics.DrawImage(softDecor, destination, source.X, source.Y, source.Width, source.Height, GraphicsUnit.Pixel, _decorAttributes);
        graphics.DrawImage(crispDecor, destination, source.X, source.Y, source.Width, source.Height, GraphicsUnit.Pixel, _decorAttributes);
    }
    public void StartEntrance()
    {
        SettleEntrance();
        if (!MotionSystem.Enabled || !Visible) return;
        SetProgress(0);
        MotionSystem.Animator.Schedule(this, "decor-delay", TimeSpan.FromMilliseconds(MotionSystem.StaggerStep), () =>
            MotionSystem.Animator.Run(this, "decor", 0, 1, MotionSystem.Slow, Easing.EaseOutCubic, SetProgress));
    }
    private void SetProgress(float value) { DecorationProgress = Math.Clamp(value, 0, 1); Invalidate(true); }
    public void SettleEntrance() { MotionSystem.Animator.Cancel(this); SetProgress(1); }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e) => PaintCrop(e.Graphics, ClientRectangle, Point.Empty);
    private void DisposeCache()
    {
        _photo?.Dispose(); _softDecor?.Dispose(); _crispDecor?.Dispose(); _composite?.Dispose(); _blurred?.Dispose();
        _photo = _softDecor = _crispDecor = _composite = _blurred = null;
        Decorations = Array.Empty<LoginDecorPlacement>();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) { MotionSystem.Animator.Cancel(this); DisposeCache(); _decorAttributes.Dispose(); }
        base.Dispose(disposing);
    }
}
