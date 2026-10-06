using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class DentistWorklist : UserControl
{
    private readonly TableLayoutPanel _rows = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas, Margin = Padding.Empty };
    private readonly List<WorklistCard> _cards = [];
    public event Action<int>? HistoryRequested;
    public event Action<int>? CompletionRequested;
    public DentistWorklist(IReadOnlyList<AppointmentDetails> appointments, User actor, DateTime now)
    {
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this);
        Name = "dentistWorklist"; Dock = DockStyle.Top; AutoSize = true; BackColor = Palette.Canvas;
        _rows.ColumnStyles.Add(new(SizeType.Percent, 100)); Controls.Add(_rows);
        var list = DashboardPresentation.Worklist(appointments, actor.DentistId ?? 0, now);
        var next = DashboardPresentation.NextAppointment(list, now);
        if (list.Count == 0)
        {
            _rows.Controls.Add(new EmptyState("No patients today", "Your appointments for today will appear here.", IconKind.Patients) { Dock = DockStyle.Top });
            return;
        }
        foreach (var detail in list) _cards.Add(Row(detail, actor, detail.Appointment.AppointmentId == next));
        SizeChanged += (_, _) => Arrange(); DpiChangedAfterParent += (_, _) => Arrange(); Arrange();
    }
    private void Arrange()
    {
        var columns = Width >= Metrics.Scale(this, Metrics.WorklistColumnsWidth) ? 2 : 1;
        if (_rows.ColumnCount == columns && _rows.Controls.Count == _cards.Count) return;
        _rows.SuspendLayout();
        try
        {
            _rows.Controls.Clear(); _rows.ColumnCount = columns; _rows.ColumnStyles.Clear(); _rows.RowStyles.Clear();
            _rows.RowCount = (_cards.Count + columns - 1) / columns;
            for (var i = 0; i < columns; i++) _rows.ColumnStyles.Add(new(SizeType.Percent, 100f / columns));
            for (var i = 0; i < _rows.RowCount; i++) _rows.RowStyles.Add(new(SizeType.AutoSize));
            for (var i = 0; i < _cards.Count; i++)
            {
                _cards[i].Margin = new(0, 0, i % columns < columns - 1 ? Metrics.Scale(this, Space.Sm) : 0, Metrics.Scale(this, Space.Sm));
                _rows.Controls.Add(_cards[i], i % columns, i / columns);
            }
        }
        finally { _rows.ResumeLayout(true); }
    }
    private WorklistCard Row(AppointmentDetails details, User actor, bool next)
    {
        var a = details.Appointment;
        var background = next ? Palette.BrandSoft : Palette.Surface;
        var card = new WorklistCard { Name = $"appointmentCard{a.AppointmentId}", Dock = DockStyle.Top, Height = Metrics.WorklistHeight, FaceColor = background };
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, BackColor = background, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new(SizeType.Absolute, Metrics.Scale(card, Metrics.WorklistTimeWidth)));
        layout.ColumnStyles.Add(new(SizeType.Percent, 100)); layout.ColumnStyles.Add(new(SizeType.AutoSize));
        var time = new Label { Text = $"{DisplayFormat.Time(a.AppointmentDateTime)}\n{a.DurationMinutes} min", Dock = DockStyle.Fill,
            Font = Typography.Label, ForeColor = Palette.Ink700, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty };
        var identity = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = background, Margin = new(0, 0, Space.Sm, 0) };
        identity.ColumnStyles.Add(new(SizeType.Percent, 100));
        identity.RowStyles.Add(new(SizeType.Percent, 50)); identity.RowStyles.Add(new(SizeType.Percent, 50));
        var heading = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty, BackColor = background };
        heading.ColumnStyles.Add(new(SizeType.Percent, 100)); heading.ColumnStyles.Add(new(SizeType.AutoSize));
        heading.Controls.Add(new Label { Text = details.Patient.FullName, Anchor = AnchorStyles.Left | AnchorStyles.Right, AutoEllipsis = true,
            Height = TextRenderer.MeasureText("Ag", Typography.Label).Height,
            Font = Typography.Label, ForeColor = Palette.Ink900, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty }, 0, 0);
        var badge = new StatusBadge(a.Status) { Anchor = AnchorStyles.Right, Margin = Padding.Empty };
        badge.Width = badge.MinimumSize.Width; heading.Controls.Add(badge, 1, 0);
        var reason = new Label { Text = (next ? "Next up · " : "") + DisplayFormat.Optional(a.Reason), Dock = DockStyle.Fill,
            AutoEllipsis = true, Font = Typography.Caption, ForeColor = Palette.Ink700, Margin = Padding.Empty, TextAlign = ContentAlignment.MiddleLeft };
        Tooltips.Attach(reason, DisplayFormat.Optional(a.Reason)); identity.Controls.Add(heading, 0, 0); identity.Controls.Add(reason, 0, 1);
        var actions = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Right, BackColor = background, Margin = Padding.Empty };
        var history = UiFactory.Button("Patient history", ButtonVariant.Secondary, IconKind.Patients, ButtonSize.Compact);
        history.Width = history.MinimumSize.Width; history.Margin = new(0, 0, Space.Sm, 0);
        history.Click += (_, _) => HistoryRequested?.Invoke(a.AppointmentId); actions.Controls.Add(history);
        if (a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Completed))
        {
            var complete = UiFactory.Button("Mark completed", icon: IconKind.Check, size: ButtonSize.Compact);
            complete.Width = complete.MinimumSize.Width; complete.Margin = Padding.Empty;
            complete.Click += (_, _) => CompletionRequested?.Invoke(a.AppointmentId); actions.Controls.Add(complete);
        }
        layout.Controls.Add(time, 0, 0); layout.Controls.Add(identity, 1, 0); layout.Controls.Add(actions, 2, 0); card.Content.Controls.Add(layout);
        var arranging = false;
        void Fit()
        {
            if (arranging) return; arranging = true;
            try
            {
                var wrap = card.Width < Metrics.Scale(card, Metrics.WorklistWrapWidth);
                layout.RowCount = wrap ? 2 : 1; layout.RowStyles.Clear();
                layout.RowStyles.Add(new(SizeType.Percent, 100));
                if (wrap) layout.RowStyles.Add(new(SizeType.AutoSize));
                layout.SetColumnSpan(identity, wrap ? 2 : 1); layout.SetColumn(actions, wrap ? 0 : 2); layout.SetRow(actions, wrap ? 1 : 0);
                layout.SetColumnSpan(actions, wrap ? 3 : 1);
                var textHeight = TextRenderer.MeasureText("Ag", Typography.Label).Height + TextRenderer.MeasureText("Ag", Typography.Caption).Height;
                var height = Math.Max(Metrics.Scale(card, Metrics.WorklistHeight), card.Padding.Vertical + textHeight +
                    (wrap ? actions.PreferredSize.Height + Metrics.Scale(card, Space.Sm) : 0));
                card.MinimumSize = new(0, height); card.Height = height;
            }
            finally { arranging = false; }
        }
        card.SizeChanged += (_, _) => Fit(); card.DpiChangedAfterParent += (_, _) => Fit(); Fit(); card.AttachHover(); return card;
    }
}
