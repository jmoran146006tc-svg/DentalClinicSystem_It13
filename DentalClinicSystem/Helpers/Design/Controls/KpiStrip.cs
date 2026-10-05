namespace DentalClinicSystem.Helpers.Design.Controls;

// Align painted card faces to the page edges. Each cell clips its own outer
// shadow gutter so sibling windows cannot paint over a neighboring card face.
public sealed class KpiStrip : Panel
{
    private readonly IReadOnlyList<KpiCard> _cards;
    private bool _arranging;
    public KpiStrip(params KpiCard[] cards)
    {
        _cards = cards; Dock = DockStyle.Top; Margin = Padding.Empty; BackColor = Palette.Canvas;
        foreach (var card in cards)
        {
            var cell = new Panel { BackColor = Palette.Canvas }; cell.Controls.Add(card); Controls.Add(cell);
        }
        SizeChanged += (_, _) => Arrange(); DpiChangedAfterParent += (_, _) => Arrange();
        Arrange();
    }
    private void Arrange()
    {
        if (_arranging || _cards.Count == 0) return;
        _arranging = true;
        try
        {
            var columns = Width >= Metrics.Scale(this, Metrics.FormWidth * 3 - Space.Page.Horizontal) ? _cards.Count : Math.Min(2, _cards.Count);
            var inset = Metrics.Scale(this, Elevation.Padding(ElevationLevel.E1));
            var gap = Metrics.Scale(this, Space.Lg);
            var height = Metrics.Scale(this, Metrics.KpiHeight);
            var faceHeight = height - inset * 2;
            var width = Math.Max(0, Width - (columns - 1) * gap) / columns;
            Height = ((_cards.Count + columns - 1) / columns) * (faceHeight + gap);
            for (var i = 0; i < _cards.Count; i++)
            {
                var left = i % columns * (width + gap);
                var faceWidth = i % columns == columns - 1 ? Width - left : width;
                Controls[i].Bounds = new(left, i / columns * (faceHeight + gap), faceWidth, faceHeight + gap);
                _cards[i].Bounds = new(-inset, -inset, faceWidth + inset * 2, height);
            }
        }
        finally { _arranging = false; }
    }
}
