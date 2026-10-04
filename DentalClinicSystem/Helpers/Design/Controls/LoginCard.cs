using System.Drawing.Imaging;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class LoginCard : RoundedPanel
{
    private readonly LoginBrand _backdrop;
    private Bitmap? _entrance;
    private EntranceLayer? _entranceLayer;
    private readonly ImageAttributes _imageAlpha = new();
    private readonly ColorMatrix _alpha = new();
    private float _progress = 1;
    protected override int ContentInset => Space.Xl;
    public float EntranceProgress => _progress;

    public LoginCard(LoginBrand backdrop) : base(elevation: ElevationLevel.E3)
    {
        _backdrop = backdrop;
        _backdrop.CompositionChanged += BackdropChanged;
        _imageAlpha.SetColorMatrix(_alpha);
        LocationChanged += (_, _) => { Invalidate(); _entranceLayer?.Invalidate(); };
    }

    private void BackdropChanged(object? sender, EventArgs e) => Invalidate();

    public void CaptureEntrance()
    {
        _entrance?.Dispose();
        _entrance = new Bitmap(Width, Height);
        using (var graphics = Graphics.FromImage(_entrance)) PaintSurface(graphics);
        // WinForms child windows cannot have opacity. Fade one cached surface
        // above the live controls, keeping username focus and keyboard input.
        Content.DrawToBitmap(_entrance, Content.Bounds);
        _entranceLayer = new EntranceLayer(this) { Bounds = ClientRectangle };
        Controls.Add(_entranceLayer); _entranceLayer.BringToFront();
        SetEntranceProgress(0);
    }

    public void SetEntranceProgress(float progress)
    {
        _progress = Math.Clamp(progress, 0, 1);
        _alpha.Matrix33 = _progress;
        _imageAlpha.SetColorMatrix(_alpha);
        if (_progress >= 1)
        {
            _entrance?.Dispose(); _entrance = null;
            _entranceLayer?.Dispose(); _entranceLayer = null;
        }
        _entranceLayer?.Invalidate();
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        // A captured child surface becomes stale after a size or DPI change.
        if (_entrance is not null) SetEntranceProgress(1);
        base.OnSizeChanged(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e) => _backdrop?.PaintCrop(e.Graphics, ClientRectangle, Location);
    private void PaintEntrance(Graphics graphics)
    {
        _backdrop.PaintCrop(graphics, ClientRectangle, Location);
        if (_entrance is not null) graphics.DrawImage(_entrance, ClientRectangle, 0, 0, Width, Height, GraphicsUnit.Pixel, _imageAlpha);
    }

    private sealed class EntranceLayer(LoginCard card) : DesignControl
    {
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e) => card.PaintEntrance(e.Graphics);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _backdrop.CompositionChanged -= BackdropChanged; _entranceLayer?.Dispose(); _entrance?.Dispose(); _imageAlpha.Dispose(); }
        base.Dispose(disposing);
    }
}
