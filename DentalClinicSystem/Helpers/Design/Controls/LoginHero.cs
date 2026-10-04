using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using DentalClinicSystem.Helpers.Design.Motion;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed record LoginTextRegion(string Text, Rectangle Bounds, Color Ink, double MinimumRatio);

// A single geometry model drives the glass, painted copy and real input HWNDs.
// The full-client canvas also lets E3 shadows extend beyond the card edge.
public sealed class LoginHero : DesignControl
{
    private readonly ClinicLoginBackdrop _backdrop;
    private readonly FieldBox _username, _password;
    private readonly Button _submit;
    private readonly InlineAlert _alert;
    private readonly List<LoginTextRegion> _text = [];
    private Bitmap? _glass, _face, _badge, _pill, _pillBackground, _shadow;
    private Rectangle _shadowBounds;
    private bool _layoutBusy, _capsLock;
    private (Size Field, int Dpi) _inputClipKey;
    private EntranceOverlay? _entrance;
    private readonly string _eyebrow = "GOOD " + DashboardPresentation.GreetingPeriod(TimeProvider.System.GetLocalNow().DateTime).ToUpperInvariant();
    public LoginRememberToggle Remember { get; }
    public Rectangle CardBounds { get; private set; }
    public Rectangle BadgeBounds { get; private set; }
    public Rectangle PillBounds { get; private set; }
    public IReadOnlyList<LoginTextRegion> TextRegions => _text;
    public ElevationLevel CardElevation => ElevationLevel.E3;
    public bool IsEntering => _entrance is not null;

    public LoginHero(ClinicLoginBackdrop backdrop, FieldBox username, FieldBox password, Button submit, InlineAlert alert)
    {
        _backdrop = backdrop; _username = username; _password = password; _submit = submit; _alert = alert;
        Remember = new LoginRememberToggle(this) { TabIndex = 2 };
        Controls.AddRange([username, password, Remember, alert, submit]);
        _backdrop.CompositionChanged += BackdropChanged;
        _alert.VisibleChanged += (_, _) => Relayout();
        // Reserve one stable error slot. InlineAlert's height animation is
        // legacy 96-DPI geometry; its color reveal still animates, while this
        // local clamp avoids rebuilding the glass on every feedback frame.
        _alert.SizeChanged += (_, _) =>
        {
            if (!_layoutBusy && _alert.Height != S(Metrics.AlertHeight)) _alert.Height = S(Metrics.AlertHeight);
        };
        VisibleChanged += (_, _) => { if (!Visible) SettleEntrance(); };
        AccessibleName = "Welcome back. Sign in to your clinic account.";
    }
    private int S(int value) => Metrics.Scale(this, value);
    private Font PixelFont(Font font) => new(font.FontFamily, font.SizeInPoints * DeviceDpi / 72f, font.Style, GraphicsUnit.Pixel);
    private void BackdropChanged(object? sender, EventArgs e) => Relayout();
    public void SetCapsLock(bool enabled) { if (_capsLock == enabled) return; _capsLock = enabled; Relayout(); }
    protected override void OnSizeChanged(EventArgs e) { base.OnSizeChanged(e); if (_username is not null) Relayout(); }
    protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Relayout(); }
    protected override void OnLocationChanged(EventArgs e) { base.OnLocationChanged(e); Invalidate(); }

    public void Relayout()
    {
        if (_layoutBusy || Width <= 0 || Height <= 0) return;
        _layoutBusy = true;
        try
        {
            SettleEntrance();
            using var caption = PixelFont(Typography.Caption);
            using var title = PixelFont(Typography.Title);
            using var label = PixelFont(Typography.Label);
            var compact = Height < S(Metrics.LoginTargetHeight);
            var gap = S(compact ? Space.Md : Space.Lg);
            var inner = Math.Min(S(Metrics.LoginFormWidth), Width - S(Space.Xxxl * 2));
            var cardWidth = Math.Min(S(Metrics.LoginCardWidth), Width - S(Space.Xl * 2));
            var x = (Width - inner) / 2;
            var top = S(Metrics.LoginBadgeSize / 2 + Space.Lg);
            _text.Clear();
            void Copy(string text, int height, Color ink, int after, double ratio = 4.5)
            {
                _text.Add(new(text, new(x, top, inner, height), ink, ratio)); top += height + after;
            }
            Copy(_eyebrow, label.Height + S(Space.Xs), Palette.BrandSoftText, S(Space.Xs));
            Copy("Welcome back", title.Height + S(Space.Xs), Palette.Ink900, S(Space.Xs), 3);
            Copy("Sign in to your clinic account", caption.Height + S(Space.Xs), Palette.LoginBodyInk, gap);
            Copy("Username", label.Height + S(Space.Xs), Palette.LoginBodyInk, S(Space.Xs));
            _username.SetBounds(x, top, inner, Math.Max(S(Metrics.LoginFieldHeight), _username.MinimumSize.Height)); top += _username.Height + gap;
            Copy("Password", label.Height + S(Space.Xs), Palette.LoginBodyInk, S(Space.Xs));
            _password.Controls.OfType<Button>().Single().MinimumSize = new(S(Metrics.ControlHeight), S(Metrics.ControlHeight));
            _password.SetBounds(x, top, inner, Math.Max(S(Metrics.LoginFieldHeight), _password.MinimumSize.Height)); top += _password.Height;
            Remember.SetBounds(x, top, inner, S(Metrics.ControlHeight)); top += Remember.Height + S(Space.Xs);
            if (_capsLock) Copy("Caps Lock is on", S(Space.Xl), Palette.Warning.Text, S(Space.Xs));
            if (_alert.Visible)
            {
                _alert.SetBounds(x, top, inner, Math.Max(_alert.Height, S(Metrics.AlertHeight))); top += _alert.Height + S(Space.Xs);
            }
            else _alert.Width = inner;
            _submit.SetBounds(x, top, inner, S(Metrics.LoginFieldHeight)); top += _submit.Height + gap;
            Copy("Trouble signing in? Ask your clinic administrator.", caption.Height + S(Space.Xs), Palette.LoginBodyInk, S(Space.Xl));
            CardBounds = new((Width - cardWidth) / 2, (Height - top) / 2, cardWidth, top);
            BadgeBounds = new((Width - S(Metrics.LoginBadgeSize)) / 2, CardBounds.Top - S(Metrics.LoginBadgeSize) / 2, S(Metrics.LoginBadgeSize), S(Metrics.LoginBadgeSize));
            PillBounds = new((Width - S(Metrics.LoginPillWidth)) / 2, CardBounds.Bottom + S(Space.Md), S(Metrics.LoginPillWidth), S(Metrics.CompactHeight));
            for (var index = 0; index < _text.Count; index++) _text[index] = _text[index] with { Bounds = Offset(_text[index].Bounds, CardBounds.Top) };
            foreach (var control in new Control[] { _username, _password, Remember, _submit, _alert }) control.Top += CardBounds.Top;
            if (_inputClipKey != (_username.Size, DeviceDpi))
            {
                _inputClipKey = (_username.Size, DeviceDpi);
                foreach (var control in new Control[] { _username, _password, _submit })
                {
                    using var path = DesignPaint.RoundedRect(control.ClientRectangle, S(Metrics.ControlRadius));
                    var previous = control.Region; control.Region = new Region(path); previous?.Dispose();
                }
            }
            RebuildLayers();
        }
        finally { _layoutBusy = false; }
    }
    private static Rectangle Offset(Rectangle bounds, int y) { bounds.Offset(0, y); return bounds; }

    private void RebuildLayers()
    {
        _glass?.Dispose(); _face?.Dispose(); _badge?.Dispose(); _pill?.Dispose(); _pillBackground?.Dispose(); _shadow?.Dispose();
        _glass = _backdrop.CreateWashedCrop(CardBounds, Palette.GlassWash, true);
        var toneBounds = _alert.Visible ? _alert.Bounds : CardBounds;
        BackColor = SampleGlass(new(toneBounds.Left + toneBounds.Width / 2, toneBounds.Top + toneBounds.Height / 2));
        _alert.BackColor = BackColor;
        // GDI text does not preserve alpha. RGB makes cached glyphs fully opaque
        // before the rounded face is composited into the ARGB shadow layer.
        _face = new Bitmap(_glass.Width, _glass.Height, PixelFormat.Format32bppRgb);
        using (var graphics = Graphics.FromImage(_face))
        {
            Blit(graphics, _glass, Point.Empty);
            DesignPaint.Prepare(graphics);
            foreach (var region in _text)
            {
                var bounds = region.Bounds; bounds.Offset(-CardBounds.Left, -CardBounds.Top);
                var isLabel = region.Text is "Username" or "Password";
                var role = region.Text == "Welcome back" ? Typography.Title : isLabel || region.Text == _eyebrow ? Typography.Label : Typography.Caption;
                using var font = PixelFont(role);
                var flags = DesignPaint.TextFlags | (isLabel || region.Text == "Caps Lock is on" ? TextFormatFlags.Left : TextFormatFlags.HorizontalCenter);
                if (region.Text == "Caps Lock is on")
                {
                    Icons.Draw(graphics, IconKind.Warning, new(bounds.Left, bounds.Top + (bounds.Height - S(Metrics.IconSize)) / 2, S(Metrics.IconSize), S(Metrics.IconSize)), region.Ink);
                    bounds.X += S(Metrics.IconSize + Space.Sm); bounds.Width -= S(Metrics.IconSize + Space.Sm);
                }
                var copy = region.Text == _eyebrow ? string.Join(" ", region.Text.ToCharArray()) : region.Text;
                TextRenderer.DrawText(graphics, copy, font, bounds, region.Ink, flags);
            }
        }
        var inset = S(Elevation.Padding(ElevationLevel.E3));
        _shadowBounds = Rectangle.Inflate(CardBounds, inset, inset);
        _shadow = (Bitmap)ShadowCache.Get(_shadowBounds.Size, Metrics.HeroRadius, ElevationLevel.E3, DeviceDpi).Clone();
        // Bake the rounded face into its shadow layer once, including its edge.
        using (var graphics = Graphics.FromImage(_shadow))
        {
            var rect = new Rectangle(inset, inset, CardBounds.Width, CardBounds.Height);
            using var path = DesignPaint.RoundedRect(rect, S(Metrics.HeroRadius));
            graphics.SetClip(path); Blit(graphics, _face, rect.Location); graphics.ResetClip();
            using var pen = new Pen(Palette.GlassEdge, S(Metrics.Border)); DesignPaint.Prepare(graphics); graphics.DrawPath(pen, path);
        }
        BuildBadge(); BuildPill(); Invalidate(); Remember.Invalidate();
    }
    private void BuildBadge()
    {
        var padding = S(Elevation.Padding(ElevationLevel.E2));
        _badge = (Bitmap)ShadowCache.Get(new(BadgeBounds.Width + padding * 2, BadgeBounds.Height + padding * 2), Metrics.LoginBadgeSize / 2, ElevationLevel.E2, DeviceDpi).Clone();
        using var graphics = Graphics.FromImage(_badge); DesignPaint.Prepare(graphics);
        var circle = new Rectangle(padding, padding, BadgeBounds.Width, BadgeBounds.Height);
        using var gradient = new LinearGradientBrush(circle, Palette.Brand, Palette.BrandHover, LinearGradientMode.Vertical);
        graphics.FillEllipse(gradient, circle);
        using var ring = new Pen(Palette.GlassEdge, S(Metrics.Border)); graphics.DrawEllipse(ring, circle);
        var mark = LoginArtwork.Mark; var scale = Math.Min((float)S(Metrics.LoginMarkSize) / mark.Width, (float)S(Metrics.LoginMarkSize) / mark.Height);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(mark, new RectangleF(circle.Left + (circle.Width - mark.Width * scale) / 2, circle.Top + (circle.Height - mark.Height * scale) / 2, mark.Width * scale, mark.Height * scale));
    }
    private void BuildPill()
    {
        _pillBackground = _backdrop.CreateWashedCrop(PillBounds, Palette.LoginPillWash, false);
        using var copy = new Bitmap(PillBounds.Width, PillBounds.Height, PixelFormat.Format32bppRgb);
        using (var graphics = Graphics.FromImage(copy))
        {
            using var font = PixelFont(Typography.Caption);
            Blit(graphics, _pillBackground, Point.Empty);
            Icons.Draw(graphics, IconKind.Shield, new(S(Space.Lg), (PillBounds.Height - S(Metrics.IconSize)) / 2, S(Metrics.IconSize), S(Metrics.IconSize)), Palette.LoginBodyInk);
            TextRenderer.DrawText(graphics, "Staff access only · Dental Care", font,
                new Rectangle(S(Space.Lg + Metrics.IconSize + Space.Sm), 0, PillBounds.Width - S(Space.Lg * 2 + Metrics.IconSize + Space.Sm), PillBounds.Height), Palette.LoginBodyInk, DesignPaint.TextFlags);
        }
        _pill = new Bitmap(PillBounds.Width, PillBounds.Height);
        using var rounded = Graphics.FromImage(_pill);
        using var path = DesignPaint.RoundedRect(new Rectangle(Point.Empty, PillBounds.Size), PillBounds.Height / 2f);
        rounded.SetClip(path); Blit(rounded, copy, Point.Empty);
    }
    public Color SampleGlass(Point point) => _glass?.GetPixel(Math.Clamp(point.X - CardBounds.Left, 0, _glass.Width - 1), Math.Clamp(point.Y - CardBounds.Top, 0, _glass.Height - 1)) ?? Palette.Surface;
    public Color SamplePill(Point point) => _pillBackground?.GetPixel(Math.Clamp(point.X - PillBounds.Left, 0, _pillBackground.Width - 1), Math.Clamp(point.Y - PillBounds.Top, 0, _pillBackground.Height - 1)) ?? Palette.Surface;
    public void PaintGlass(Graphics graphics, Rectangle destination, Point origin)
    {
        if (_glass is not null) graphics.DrawImage(_glass, destination, new Rectangle(origin.X - CardBounds.Left, origin.Y - CardBounds.Top, destination.Width, destination.Height), GraphicsUnit.Pixel);
        else graphics.Clear(Palette.Surface);
    }
    internal void PaintBase(Graphics graphics)
    {
        _backdrop.PaintCrop(graphics, ClientRectangle, Location);
        if (_pill is not null) Blit(graphics, _pill, PillBounds.Location);
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        PaintBase(e.Graphics);
        if (_shadow is not null) Blit(e.Graphics, _shadow, _shadowBounds.Location);
        if (_badge is not null) Blit(e.Graphics, _badge, new(BadgeBounds.Left - S(Space.Xl), BadgeBounds.Top - S(Space.Xl)));
    }
    private static void Blit(Graphics graphics, Bitmap bitmap, Point origin) => graphics.DrawImage(bitmap,
        new Rectangle(origin, bitmap.Size), 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel);
    public void StartEntrance()
    {
        SettleEntrance();
        if (!MotionSystem.Enabled || !Visible || _shadow is null || _badge is null) return;
        _entrance = new EntranceOverlay(this, _shadow, _shadowBounds, _badge, Rectangle.Inflate(BadgeBounds, S(Space.Xl), S(Space.Xl)), [_username, _password, Remember, _submit]);
        Controls.Add(_entrance); _entrance.BringToFront(); _entrance.Start();
    }
    public void SettleEntrance()
    {
        var overlay = _entrance; _entrance = null;
        if (overlay is not null) { Controls.Remove(overlay); overlay.Dispose(); }
        Invalidate();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _backdrop.CompositionChanged -= BackdropChanged; SettleEntrance();
            _glass?.Dispose(); _face?.Dispose(); _badge?.Dispose(); _pill?.Dispose(); _pillBackground?.Dispose(); _shadow?.Dispose();
        }
        base.Dispose(disposing);
    }

    private sealed class EntranceOverlay : DesignControl
    {
        private sealed class Layer(Bitmap image, Rectangle bounds, bool owned) : IDisposable
        {
            public Bitmap Image { get; } = image;
            public Rectangle Bounds { get; } = bounds;
            public float Progress { get; set; }
            private readonly ImageAttributes _attributes = new();
            private readonly ColorMatrix _matrix = new();
            public void Paint(Graphics graphics, Rectangle destination, float opacity)
            {
                _matrix.Matrix33 = Math.Clamp(opacity, 0, 1); _attributes.SetColorMatrix(_matrix);
                graphics.DrawImage(Image, destination, 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, _attributes);
            }
            public void Dispose() { _attributes.Dispose(); if (owned) Image.Dispose(); }
        }
        private readonly LoginHero _hero;
        private readonly Layer _card, _badge;
        private readonly Layer[] _rows;
        public EntranceOverlay(LoginHero hero, Bitmap card, Rectangle cardBounds, Bitmap badge, Rectangle badgeBounds, Control[] rows)
        {
            _hero = hero; Bounds = hero.ClientRectangle; Enabled = false;
            _card = new(card, cardBounds, false); _badge = new(badge, badgeBounds, false);
            _rows = rows.Select(row =>
            {
                var bitmap = new Bitmap(row.Width, row.Height); row.DrawToBitmap(bitmap, row.ClientRectangle); return new Layer(bitmap, row.Bounds, true);
            }).ToArray();
        }
        public void Start()
        {
            MotionSystem.Animator.Run(this, "card", 0, 1, MotionSystem.Slow, Easing.EaseOutCubic, t => { _card.Progress = t; Invalidate(); }, () => _hero.SettleEntrance());
            MotionSystem.Animator.Schedule(this, "badge-delay", TimeSpan.FromMilliseconds(MotionSystem.StaggerStep), () =>
                MotionSystem.Animator.Run(this, "badge", 0, 1, MotionSystem.Base, Easing.EaseOutBack, t => { _badge.Progress = t; Invalidate(); }));
            for (var index = 0; index < _rows.Length; index++)
            {
                var row = _rows[index]; var key = "row-" + index;
                MotionSystem.Animator.Schedule(this, key + "-delay", TimeSpan.FromMilliseconds(MotionSystem.StaggerStep * index), () =>
                    MotionSystem.Animator.Run(this, key, 0, 1, MotionSystem.Fast, Easing.EaseOutCubic, t => { row.Progress = t; Invalidate(); }));
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e) { }
        protected override void OnPaint(PaintEventArgs e)
        {
            _hero.PaintBase(e.Graphics);
            var dy = (int)(Metrics.Scale(this, Metrics.Slide) * (1 - _card.Progress));
            _card.Paint(e.Graphics, Offset(_card.Bounds, dy), _card.Progress);
            var badge = _badge.Bounds; var scale = _badge.Progress;
            var width = (int)(badge.Width * scale); var height = (int)(badge.Height * scale);
            badge = new(badge.Left + (badge.Width - width) / 2, badge.Top + (badge.Height - height) / 2 + dy, width, height);
            if (width > 0 && height > 0) _badge.Paint(e.Graphics, badge, scale);
            foreach (var row in _rows) row.Paint(e.Graphics, Offset(row.Bounds, dy), row.Progress * _card.Progress);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _card.Dispose(); _badge.Dispose(); foreach (var row in _rows) row.Dispose(); }
            base.Dispose(disposing);
        }
    }
}

// Login's toggle paints the exact cached glass beneath its hit target. It uses
// the shared toggle's keyboard semantics and tokens without changing other pages.
public sealed class LoginRememberToggle : CheckBox
{
    private readonly LoginHero _hero;
    private float _position;
    public LoginRememberToggle(LoginHero hero)
    {
        _hero = hero; Theme.MarkPrimitive(this); DesignPaint.Enable(this);
        Text = "Remember username"; AccessibleName = Text; Font = Typography.Body; TabStop = true; Cursor = Cursors.Hand;
        CheckedChanged += (_, _) => MotionSystem.Animator.Run(this, "remember", _position, Checked ? 1 : 0, MotionSystem.Base, Easing.EaseOutCubic, t => { _position = t; Invalidate(); });
        GotFocus += (_, _) => Invalidate(); LostFocus += (_, _) => Invalidate();
    }
    protected override void OnPaintBackground(PaintEventArgs e) { }
    protected override void OnPaint(PaintEventArgs e)
    {
        _hero.PaintGlass(e.Graphics, ClientRectangle, Location);
        int S(int value) => Metrics.Scale(this, value);
        var track = new Rectangle(S(Space.Xs), (Height - S(Metrics.IconSize)) / 2, S(Metrics.ControlHeight), S(Metrics.IconSize));
        DesignPaint.Surface(e.Graphics, track, track.Height / 2f, Theme.Lerp(Palette.LineStrong, Palette.Brand, _position), Palette.LoginFieldBorder);
        var diameter = track.Height - S(Space.Xs);
        using var knob = new SolidBrush(Palette.Surface);
        e.Graphics.FillEllipse(knob, track.Left + S(Metrics.FocusRing) + (track.Width - diameter - S(Space.Xs)) * _position, track.Top + S(Metrics.FocusRing), diameter, diameter);
        TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(track.Right + S(Space.Sm), 0, Width - track.Right - S(Space.Sm), Height), Palette.LoginBodyInk, DesignPaint.TextFlags);
        if (Focused)
        {
            using var path = DesignPaint.RoundedRect(Rectangle.Inflate(ClientRectangle, -S(Metrics.FocusRing), -S(Metrics.FocusRing)), S(Metrics.ControlRadius));
            using var pen = new Pen(Palette.Brand, S(Metrics.FocusRing)); e.Graphics.DrawPath(pen, path);
        }
    }
    protected override void Dispose(bool disposing) { if (disposing) MotionSystem.Animator.Cancel(this); base.Dispose(disposing); }
}
