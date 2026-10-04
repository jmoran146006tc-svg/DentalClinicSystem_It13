using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class DentistWorklist : UserControl
{
    private readonly TableLayoutPanel _rows = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas };
    public event Action<int>? HistoryRequested;
    public event Action<int>? CompletionRequested;
    public DentistWorklist(IReadOnlyList<AppointmentDetails> appointments, User actor, DateTime now)
    {
        Theme.MarkPrimitive(this); DesignPaint.Enable(this);
        Name = "dentistWorklist"; Dock = DockStyle.Top; AutoSize = true; BackColor = Palette.Canvas;
        _rows.ColumnStyles.Add(new(SizeType.Percent, 100)); Controls.Add(_rows);
        var list = DashboardPresentation.Worklist(appointments, actor.DentistId ?? 0, now);
        var next = DashboardPresentation.NextAppointment(list, now);
        if (list.Count == 0)
        {
            var empty = new EmptyState("No patients today", "Your appointments for today will appear here.", IconKind.Patients) { Dock = DockStyle.Top };
            _rows.Controls.Add(empty); return;
        }
        foreach (var detail in list) _rows.Controls.Add(Row(detail, actor, detail.Appointment.AppointmentId == next));
    }
    private WorklistCard Row(AppointmentDetails details, User actor, bool next)
    {
        var a = details.Appointment;
        var card = new WorklistCard { Name = $"appointmentCard{a.AppointmentId}", Dock = DockStyle.Top, Height = Metrics.WorklistHeight, Margin = new Padding(0, 0, 0, Space.Sm) };
        var background = next ? Palette.BrandSoft : Palette.Surface;
        card.Content.BackColor = background;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = background, Margin = Padding.Empty };
        layout.ColumnStyles.Add(new(SizeType.Percent, 100));
        layout.RowStyles.Add(new(SizeType.AutoSize)); layout.RowStyles.Add(new(SizeType.Percent, 100)); layout.RowStyles.Add(new(SizeType.AutoSize));
        var heading = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, WrapContents = true, BackColor = background, Margin = Padding.Empty };
        heading.Controls.Add(new Label { Text = $"{DisplayFormat.Time(a.AppointmentDateTime)}  {details.Patient.FullName}", AutoSize = true, Font = Typography.Heading, ForeColor = Palette.Ink900, Margin = new Padding(0, Space.Xs, Space.Sm, Space.Xs) });
        heading.Controls.Add(new StatusBadge(a.Status));
        if (next) heading.Controls.Add(new Badge("Next up", Semantic.Info) { Style = new(Palette.BrandSoft, Palette.BrandSoftText) });
        var reason = new Label { Text = DisplayFormat.Optional(a.Reason), Dock = DockStyle.Fill, AutoEllipsis = true, Font = Typography.Body, ForeColor = Palette.Ink700, Margin = new Padding(0, Space.Xs, 0, Space.Sm) };
        Tooltips.Attach(reason, DisplayFormat.Optional(a.Reason));
        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = background, Margin = Padding.Empty };
        var history = UiFactory.Button("Patient history", ButtonVariant.Secondary, IconKind.Patients, ButtonSize.Compact);
        history.Click += (_, _) => HistoryRequested?.Invoke(a.AppointmentId); actions.Controls.Add(history);
        if (a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Completed))
        {
            var complete = UiFactory.Button("Mark completed", icon: IconKind.Check, size: ButtonSize.Compact);
            complete.Click += (_, _) => CompletionRequested?.Invoke(a.AppointmentId); actions.Controls.Add(complete);
        }
        layout.Controls.Add(heading, 0, 0); layout.Controls.Add(reason, 0, 1); layout.Controls.Add(actions, 0, 2);
        card.Content.Controls.Add(layout);
        layout.Layout += (_, _) => card.MinimumSize = new(0, Math.Max(Metrics.Scale(card, Metrics.WorklistHeight),
            layout.GetPreferredSize(new(Math.Max(1, card.Content.Width), 0)).Height + card.Padding.Vertical));
        card.AttachHover(); return card;
    }
}
