using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace DentalClinicSystem.Helpers.Design.Controls;

// The photograph is decoded once. Each control owns its size/DPI composition.
public sealed class LoginBrand : DesignControl
{
    private static readonly Lazy<Image> Mark = new(() => LoadImage("LoginMark.png"));
    private static readonly Lazy<Image> Photo = new(() => LoadImage("dentist.jpg"));
    private static readonly (IconKind Icon, string Text)[] Features =
    [
        (IconKind.Appointments, "Smart scheduling"),
        (IconKind.Patients, "Patient records"),
        (IconKind.Treatments, "Treatment history")
    ];
    private Bitmap? _background;
    private Bitmap? _story;
    private readonly ImageAttributes _storyAlpha = new();
    private readonly ColorMatrix _alpha = new();
    private int _storyRight;
    private float _progress = 1;

    private static Image LoadImage(string name)
    {
        using var stream = typeof(LoginBrand).Assembly.GetManifestResourceStream($"DentalClinicSystem.Resources.{name}")
            ?? throw new InvalidOperationException($"Missing embedded login image: {name}");
        using var source = Image.FromStream(stream);
        return (Image)source.Clone();
    }

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Image? LogoImage { get; init; } = Mark.Value;
    public static Image PhotoImage => Photo.Value;
    public static double PhotoAspect => (double)PhotoImage.Width / PhotoImage.Height;
    public Rectangle StoryBounds { get; private set; }
    public RectangleF PhotoBounds { get; private set; }
    public float EntranceProgress => _progress;
    public event EventHandler? CompositionChanged;

    public LoginBrand()
    {
        Dock = DockStyle.Fill;
        AccessibleName = "Dental Care. Appointments and treatments, organized. Smart scheduling, patient records, treatment history.";
        _storyAlpha.SetColorMatrix(_alpha);
    }

    public void SetStoryRight(int right)
    {
        if (_storyRight == right) return;
        _storyRight = right;
        Rebuild();
    }

    public void SetEntranceProgress(float progress)
    {
        _progress = Math.Clamp(progress, 0, 1);
        _alpha.Matrix33 = _progress;
        _storyAlpha.SetColorMatrix(_alpha);
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); Rebuild(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Rebuild(); }

    private void Rebuild()
    {
        _background?.Dispose(); _background = null;
        _story?.Dispose(); _story = null;
        if (Width <= 0 || Height <= 0) return;
        _background = new Bitmap(Width, Height);
        using (var graphics = Graphics.FromImage(_background))
        {
            DesignPaint.Begin(graphics, this);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            var photo = PhotoImage;
            var cover = Math.Max((float)Width / photo.Width, (float)Height / photo.Height);
            PhotoBounds = new((Width - photo.Width * cover) / 2, (Height - photo.Height * cover) / 2, photo.Width * cover, photo.Height * cover);
            graphics.DrawImage(photo, PhotoBounds);
            using var wash = new SolidBrush(Palette.LoginPhotoWash);
            graphics.FillRectangle(wash, ClientRectangle);
            using var scrim = new LinearGradientBrush(ClientRectangle, Palette.LoginScrimLeft, Palette.LoginScrimRight, LinearGradientMode.Horizontal);
            scrim.InterpolationColors = new ColorBlend
            {
                Colors = [Palette.LoginScrimLeft, Palette.LoginScrimBrandEdge, Palette.LoginScrimRight, Palette.LoginScrimRight],
                Positions = [0, Metrics.LoginBrandZone, Metrics.LoginScrimFadeEnd, 1]
            };
            graphics.FillRectangle(scrim, ClientRectangle);
        }
        // GDI text on a transparent bitmap does not preserve glyph alpha.
        // An opaque copy gives clean antialiasing and the same photo pixels
        // beneath the story at every opacity during its brief entrance.
        _story = (Bitmap)_background.Clone();
        using (var graphics = Graphics.FromImage(_story)) DrawStory(graphics);
        Invalidate();
        CompositionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void DrawStory(Graphics graphics)
    {
        DesignPaint.Prepare(graphics);
        int S(int value) => Metrics.Scale(this, value);
        using var display = ScaledFont(Typography.Display);
        using var body = ScaledFont(Typography.Body);
        using var caption = ScaledFont(Typography.Caption);
        var left = S(Space.Xxl);
        // Keep every glyph inside the guaranteed scrim and clear of the card.
        var right = Math.Min((int)(Width * Metrics.LoginBrandZone), _storyRight > 0 ? _storyRight : Width);
        var width = Math.Max(S(Metrics.IconSize), right - left);
        var badge = new Rectangle(left, S(Space.Xxl), S(Metrics.LoginBadgeSize), S(Metrics.LoginBadgeSize));
        using var fill = new SolidBrush(Palette.LoginBadgeFill);
        using var ring = new Pen(Palette.LoginChipBorder, S(Metrics.Border));
        graphics.FillEllipse(fill, badge); graphics.DrawEllipse(ring, badge);
        if (LogoImage is { } mark)
        {
            var size = S(Metrics.LoginMarkSize);
            var scale = Math.Min((float)size / mark.Width, (float)size / mark.Height);
            graphics.DrawImage(mark, new RectangleF(badge.Left + (badge.Width - mark.Width * scale) / 2,
                badge.Top + (badge.Height - mark.Height * scale) / 2, mark.Width * scale, mark.Height * scale));
        }
        var top = Math.Max(badge.Bottom + S(Space.Xxl), (Height - S(Metrics.LoginTargetHeight / 4)) / 2);
        var titleHeight = TextRenderer.MeasureText(graphics, "Dental Care", display, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Height + S(Space.Sm);
        TextRenderer.DrawText(graphics, "Dental Care", display, new Rectangle(left, top, width, titleHeight), Palette.Surface, DesignPaint.TextFlags);
        var taglineTop = top + titleHeight + S(Space.Sm);
        var wrapped = TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;
        var taglineHeight = TextRenderer.MeasureText(graphics, "Appointments and treatments, organized.", body, new Size(width, int.MaxValue), wrapped).Height;
        TextRenderer.DrawText(graphics, "Appointments and treatments, organized.", body, new Rectangle(left, taglineTop, width, taglineHeight),
            Contrast.Composite(Palette.LoginCaption, Palette.BrandPressed), wrapped);
        var chipTop = taglineTop + taglineHeight + S(Space.Xl);
        var x = left;
        var chipHeight = S(Metrics.CompactHeight);
        foreach (var (icon, text) in Features)
        {
            var textWidth = TextRenderer.MeasureText(graphics, text, caption, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width;
            var chipWidth = textWidth + S(Metrics.IconSize + Space.Sm + Space.Md * 2);
            if (x > left && x + chipWidth > right) { x = left; chipTop += chipHeight + S(Space.Sm); }
            var bounds = new Rectangle(x, chipTop, Math.Min(width, chipWidth), chipHeight);
            DesignPaint.Surface(graphics, bounds, chipHeight / 2f, Palette.LoginChipFill, Palette.LoginChipBorder);
            Icons.Draw(graphics, icon, new(bounds.Left + S(Space.Md), bounds.Top + (chipHeight - S(Metrics.IconSize)) / 2, S(Metrics.IconSize), S(Metrics.IconSize)), Palette.Surface);
            TextRenderer.DrawText(graphics, text, caption, new Rectangle(bounds.Left + S(Space.Md + Metrics.IconSize + Space.Sm), bounds.Top,
                Math.Max(0, bounds.Width - S(Space.Md * 2 + Metrics.IconSize + Space.Sm)), chipHeight), Palette.Surface, DesignPaint.TextFlags);
            x = bounds.Right + S(Space.Sm);
        }
        StoryBounds = new(left, top, width, chipTop + chipHeight - top);
        TextRenderer.DrawText(graphics, "Dental Clinic Management System", caption,
            new Rectangle(left, Height - S(Space.Xxl + Space.Xl), width, S(Space.Xl)),
            Contrast.Composite(Palette.LoginFooter, Palette.BrandPressed), DesignPaint.TextFlags);
    }

    // Pixel units prevent GDI's screen DPI from scaling the offscreen bitmap
    // font a second time. Typography remains the source of point sizes.
    private Font ScaledFont(Font font) => new(font.FontFamily, font.SizeInPoints * DeviceDpi / 72f, font.Style, GraphicsUnit.Pixel);

    // The card uses the matching crop without parent Paint recursion or capture.
    public void PaintCrop(Graphics graphics, Rectangle destination, Point origin)
    {
        if (_background is null) { graphics.Clear(Palette.BrandPressed); return; }
        graphics.DrawImage(_background, destination, new Rectangle(origin, destination.Size), GraphicsUnit.Pixel);
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        if (_background is null || _story is null) return;
        if (_progress >= 1) e.Graphics.DrawImageUnscaled(_story, Point.Empty);
        else
        {
            e.Graphics.DrawImageUnscaled(_background, Point.Empty);
            e.Graphics.DrawImage(_story, ClientRectangle, 0, 0, Width, Height, GraphicsUnit.Pixel, _storyAlpha);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _background?.Dispose(); _story?.Dispose(); _storyAlpha.Dispose(); }
        base.Dispose(disposing);
    }
}
