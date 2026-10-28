using System.Collections;
using System.Drawing.Drawing2D;
using System.Reflection;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public sealed class ucReports : UserControl
{
    // ---------- palette ----------
    private static readonly Color Bg = Color.FromArgb(243, 248, 249);
    private static readonly Color Ink = Color.FromArgb(28, 40, 51);
    private static readonly Color Muted = Color.FromArgb(107, 122, 133);
    private static readonly Color Border = Color.FromArgb(226, 236, 238);
    private static readonly Color Teal = Color.FromArgb(69, 200, 200);
    private static readonly Color TealDark = Color.FromArgb(14, 124, 134);
    private static readonly Color Track = Color.FromArgb(229, 240, 241);

    // ---------- fonts (created once, reused by every paint) ----------
    private static readonly Font TitleFont = new("Segoe UI Semibold", 14f);
    private static readonly Font SubFont = new("Segoe UI", 10f);
    private static readonly Font LabelFont = new("Segoe UI", 11f);
    private static readonly Font ValueFont = new("Segoe UI Semibold", 11f);
    private static readonly Font PillFont = new("Segoe UI Semibold", 9f);
    private static readonly Font BigFont = new("Segoe UI Semibold", 24f);

    private const TextFormatFlags LeftFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                                              TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
    private const TextFormatFlags RightFlags = TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
                                               TextFormatFlags.NoPadding;

    private const int RowGap = 20;
    private const int ColGap = 20;

    private readonly IReportService _reports;
    private readonly User _user;
    private readonly DateTimePicker _from = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    private readonly DateTimePicker _to = new() { Format = DateTimePickerFormat.Short, Width = 110 };

    private readonly BarList _workload = new();
    private readonly BarList _revenue = new();
    private readonly BarList _types = new();
    private readonly StatusTiles _status = new();

    private readonly TableLayoutPanel _grid;
    private readonly Card _workloadCard;
    private readonly Card _statusCard;
    private readonly Card _revenueCard;
    private readonly Card _typesCard;

    public ucReports(IReportService reports, User user)
    {
        _reports = reports;
        _user = user;

        BackColor = Bg;
        AutoScroll = true;
        Padding = new Padding(24, 8, 24, 24);

        _from.Value = new(DateTime.Today.Year, DateTime.Today.Month, 1);
        _to.Value = DateTime.Today;

        // ----- header: subtitle on the left, date filters on the right -----
        var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Bg };

        var subtitle = new Label
        {
            Text = "A simple overview of clinic activity",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Muted,
            Font = LabelFont,
            BackColor = Bg
        };

        var filters = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 12, 0, 0),
            BackColor = Bg
        };

        var refresh = new Button
        {
            Text = "Refresh",
            Width = 96,
            Height = 32,
            FlatStyle = FlatStyle.Flat,
            BackColor = TealDark,
            ForeColor = Color.White,
            Font = ValueFont,
            Cursor = Cursors.Hand,
            Margin = new Padding(0)
        };
        refresh.FlatAppearance.BorderSize = 0;
        refresh.Click += async (_, _) => await LoadReports();

        _from.Margin = new Padding(0, 4, 14, 0);
        _to.Margin = new Padding(0, 4, 14, 0);

        filters.Controls.Add(FilterLabel("From"));
        filters.Controls.Add(_from);
        filters.Controls.Add(FilterLabel("To"));
        filters.Controls.Add(_to);
        filters.Controls.Add(refresh);

        header.Controls.Add(subtitle);
        header.Controls.Add(filters);
        subtitle.BringToFront(); // Fill control must be docked last

        // ----- 2 x 2 grid of cards -----
        _grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Bg,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        _grid.ColumnStyles.Add(new(SizeType.Percent, 50));
        _grid.ColumnStyles.Add(new(SizeType.Percent, 50));
        _grid.RowStyles.Add(new(SizeType.Absolute, 400));
        _grid.RowStyles.Add(new(SizeType.Absolute, 300));

        _workloadCard = new Card("Dentist workload", "Appointments per dentist", _workload);
        _statusCard = new Card("Appointments by status", "All appointments in the selected range", _status);
        _revenueCard = new Card("Billed by day", "Total billed per day", _revenue);
        _typesCard = new Card("Top treatment types", "Most frequently recorded treatments", _types);

        Place(_workloadCard, 0, 0);
        Place(_statusCard, 1, 0);
        Place(_revenueCard, 0, 1);
        Place(_typesCard, 1, 1);

        // Docking: the control added LAST is docked FIRST, so header goes in after grid.
        Controls.Add(_grid);
        Controls.Add(header);

        ApplyLayout();
        Load += async (_, _) => await LoadReports();
    }

    private static Label FilterLabel(string text) => new()
    {
        Text = text,
        AutoSize = false,
        Width = 44,
        Height = 32,
        TextAlign = ContentAlignment.MiddleRight,
        ForeColor = Muted,
        Font = SubFont,
        Margin = new Padding(0, 0, 6, 0)
    };

    private void Place(Card card, int col, int row)
    {
        card.Margin = new Padding(col == 0 ? 0 : ColGap / 2, 0, col == 0 ? ColGap / 2 : 0, RowGap);
        _grid.Controls.Add(card, col, row);
    }

    /// <summary>Sizes each grid row to fit the taller card in it, so nothing is clipped or left as a huge blank area.</summary>
    private void ApplyLayout()
    {
        int row0 = Math.Max(_workloadCard.PreferredCardHeight, _statusCard.PreferredCardHeight) + RowGap;
        int row1 = Math.Max(_revenueCard.PreferredCardHeight, _typesCard.PreferredCardHeight) + RowGap;

        _grid.RowStyles[0].Height = row0;
        _grid.RowStyles[1].Height = row1;
        _grid.Height = row0 + row1;
    }

    private async Task LoadReports()
    {
        var from = _from.Value.Date;
        var to = _to.Value.Date;

        if (from > to)
        {
            MessageBox.Show("'From' date must not be later than 'To' date.");
            return;
        }

        await UiAction.RunAsync(this, async () =>
        {
            var s = _reports.GetAppointmentStatusCountsAsync(_user, from, to);
            var r = _reports.GetRevenueByDayAsync(_user, from, to);
            var t = _reports.GetTopTreatmentTypesAsync(_user, from, to);
            var w = _reports.GetDentistWorkloadAsync(_user, from, to);

            await Task.WhenAll(s, r, t, w);
            if (IsDisposed) return;

            if (!s.Result.Success)
            {
                UiMessages.ShowError(s.Result);
                return;
            }

            _status.SetData(Pairs(UiMessages.Items(s.Result)));
            _revenue.SetData(Pairs(UiMessages.Items(r.Result)), v => $"₱{v:N2}");
            _types.SetData(Pairs(UiMessages.Items(t.Result), true));
            _workload.SetData(Pairs(UiMessages.Items(w.Result), true));

            ApplyLayout();
        });
    }

    private static List<(string Label, double Value)> Pairs(
        IEnumerable? items, bool sort = false)
    {
        var result = new List<(string, double)>();
        if (items == null) return result;

        foreach (var item in items)
        {
            if (item == null) continue;

            var props = item.GetType().GetProperties()
                .Where(p => !p.Name.EndsWith("Id") &&
                            p.GetIndexParameters().Length == 0).ToArray();

            var value = props.FirstOrDefault(p =>
                IsNumber(p.PropertyType));
            var label = props.FirstOrDefault(p =>
                !IsNumber(p.PropertyType));

            if (value == null || label == null) continue;

            var name = label.GetValue(item);
            result.Add((
                name is DateTime d ? d.ToString("MMM d") :
                name is DateOnly date ? date.ToString("MMM d") :
                name?.ToString() ?? "-",
                Convert.ToDouble(value.GetValue(item) ?? 0)));
        }

        return sort ? result.OrderByDescending(x => x.Item2).ToList() : result;
    }

    private static bool IsNumber(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return Type.GetTypeCode(t) is TypeCode.Byte or TypeCode.Int16 or
            TypeCode.Int32 or TypeCode.Int64 or TypeCode.Single or
            TypeCode.Double or TypeCode.Decimal or TypeCode.UInt16 or
            TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.SByte;
    }

    /// <summary>"Checked-in", "CheckedIn", "checked in" -> "checkedin" so status names match however they are spelled.</summary>
    private static string Normalize(string s) =>
        new string(s.Where(char.IsLetterOrDigit).ToArray())
            .ToLowerInvariant()
            .Replace("canceled", "cancelled");

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    // =====================================================================
    //  Card: white rounded panel with the title + subtitle ON TOP and a divider
    // =====================================================================
    private sealed class Card : Panel
    {
        private const int HeaderHeight = 78;
        private const int ContentGap = 12;
        private const int SidePad = 24;
        private const int BottomPad = 24;

        private readonly string _title;
        private readonly string _subtitle;
        private readonly ChartBase _content;

        public Card(string title, string subtitle, ChartBase content)
        {
            _title = title;
            _subtitle = subtitle;
            _content = content;

            BackColor = Bg;
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw, true);
            Padding = new Padding(SidePad, HeaderHeight + ContentGap, SidePad, BottomPad);

            content.Dock = DockStyle.Fill;
            Controls.Add(content);
        }

        public int PreferredCardHeight =>
            HeaderHeight + ContentGap + _content.ContentHeight + BottomPad;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var path = RoundedRect(rect, 16))
            using (var fill = new SolidBrush(Color.White))
            using (var pen = new Pen(Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(pen, path);
            }

            int textW = Math.Max(0, Width - SidePad * 2);
            TextRenderer.DrawText(g, _title, TitleFont,
                new Rectangle(SidePad, 18, textW, 28), Ink, LeftFlags);
            TextRenderer.DrawText(g, _subtitle, SubFont,
                new Rectangle(SidePad, 46, textW, 20), Muted, LeftFlags);

            using var divider = new Pen(Border);
            g.DrawLine(divider, 1, HeaderHeight, Width - 2, HeaderHeight);
        }
    }

    // =====================================================================
    //  Chart controls
    // =====================================================================
    private abstract class ChartBase : Control
    {
        protected ChartBase()
        {
            BackColor = Color.White;
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
        }

        /// <summary>Height this chart needs to show all of its data.</summary>
        public abstract int ContentHeight { get; }
    }

    private sealed class BarList : ChartBase
    {
        private const int RowPitch = 64;
        private const int BarHeight = 10;

        private List<(string Label, double Value)> _data = new();
        private Func<double, string> _format = v => v.ToString("N0");

        public override int ContentHeight => Math.Max(60, _data.Count * RowPitch);

        public void SetData(List<(string Label, double Value)> data,
            Func<double, string>? format = null)
        {
            _data = data;
            _format = format ?? (v => v.ToString("N0"));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            if (_data.Count == 0)
            {
                TextRenderer.DrawText(g, "No data", LabelFont,
                    new Rectangle(0, 4, Width, 24), Muted, LeftFlags);
                return;
            }

            if (Width <= BarHeight) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            double max = _data.Max(x => x.Value);
            const int valueWidth = 120;

            using var trackBrush = new SolidBrush(Track);
            using var barBrush = new SolidBrush(Teal);

            for (int i = 0; i < _data.Count; i++)
            {
                var (label, value) = _data[i];
                int y = i * RowPitch;

                TextRenderer.DrawText(g, label, LabelFont,
                    new Rectangle(0, y, Math.Max(0, Width - valueWidth - 8), 24), Ink, LeftFlags);
                TextRenderer.DrawText(g, _format(value), ValueFont,
                    new Rectangle(Width - valueWidth, y, valueWidth, 24), TealDark, RightFlags);

                var track = new Rectangle(0, y + 32, Width - 1, BarHeight);
                using (var trackPath = RoundedRect(track, BarHeight / 2))
                    g.FillPath(trackBrush, trackPath);

                if (max > 0 && value > 0)
                {
                    int w = Math.Max(BarHeight, (int)(track.Width * value / max));
                    var bar = new Rectangle(0, track.Y, w, BarHeight);
                    using var barPath = RoundedRect(bar, BarHeight / 2);
                    g.FillPath(barBrush, barPath);
                }
            }
        }
    }

    private sealed class StatusTiles : ChartBase
    {
        private const int TileHeight = 132;
        private const int Gap = 12;

        private static readonly (string Name, Color Fg, Color Tint)[] Tiles =
        {
            ("Scheduled",    Color.FromArgb(37, 99, 235),  Color.FromArgb(228, 238, 253)),
            ("Checked-in",   Color.FromArgb(202, 117, 8),  Color.FromArgb(254, 243, 224)),
            ("In treatment", Color.FromArgb(14, 124, 134), Color.FromArgb(222, 246, 244)),
            ("Completed",    Color.FromArgb(22, 128, 61),  Color.FromArgb(223, 245, 230)),
            ("Cancelled",    Color.FromArgb(220, 38, 38),  Color.FromArgb(254, 229, 229))
        };

        private Dictionary<string, int> _counts = new();

        public override int ContentHeight
        {
            get
            {
                int rows = (Tiles.Length + 1) / 2;
                return rows * TileHeight + (rows - 1) * Gap;
            }
        }

        public void SetData(List<(string Label, double Value)> data)
        {
            _counts = data
                .GroupBy(x => Normalize(x.Label))
                .ToDictionary(grp => grp.Key, grp => (int)grp.Sum(x => x.Value));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int tileWidth = (Width - Gap) / 2;
            if (tileWidth < 60) return;

            for (int i = 0; i < Tiles.Length; i++)
            {
                var (name, fg, tint) = Tiles[i];
                _counts.TryGetValue(Normalize(name), out int count);

                var rect = new Rectangle((i % 2) * (tileWidth + Gap),
                    (i / 2) * (TileHeight + Gap), tileWidth - 1, TileHeight - 1);

                using (var tilePath = RoundedRect(rect, 12))
                using (var tileFill = new SolidBrush(Color.White))
                using (var tilePen = new Pen(Border))
                {
                    g.FillPath(tileFill, tilePath);
                    g.DrawPath(tilePen, tilePath);
                }

                // status pill: colored dot + name
                var textSize = TextRenderer.MeasureText(g, name, PillFont,
                    Size.Empty, TextFormatFlags.NoPadding);
                var pill = new Rectangle(rect.X + 16, rect.Y + 16,
                    12 + 8 + 6 + textSize.Width + 12, 26);

                using (var pillPath = RoundedRect(pill, 13))
                using (var pillFill = new SolidBrush(tint))
                using (var dotBrush = new SolidBrush(fg))
                {
                    g.FillPath(pillFill, pillPath);
                    g.FillEllipse(dotBrush, pill.X + 12, pill.Y + 9, 8, 8);
                }

                TextRenderer.DrawText(g, name, PillFont,
                    new Rectangle(pill.X + 12 + 8 + 6, pill.Y, textSize.Width + 6, pill.Height),
                    fg, LeftFlags);

                // big number + caption
                TextRenderer.DrawText(g, count.ToString("N0"), BigFont,
                    new Point(rect.X + 14, rect.Y + 50), Ink, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "appointments", SubFont,
                    new Point(rect.X + 16, rect.Y + 100), Muted, TextFormatFlags.NoPadding);
            }
        }
    }
}