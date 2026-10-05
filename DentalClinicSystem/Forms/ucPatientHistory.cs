using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;

namespace DentalClinicSystem.Forms;

public sealed class ucPatientHistory : BufferedPage
{
    private readonly TableLayoutPanel _content = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Canvas };
    private readonly IPatientHistoryService _history;
    private readonly User _actor;
    private readonly int _appointmentId;
    private readonly DateTime _now;
    private readonly InlineAlert _alert = new() { Visible = false };
    private readonly AppButton _retry = UiFactory.Button("Retry", ButtonVariant.Secondary);
    private readonly Skeleton _skeleton = new() { Dock = DockStyle.Top, Height = Metrics.HistoryRowHeight };
    public ucPatientHistory(IPatientHistoryService history, User actor, int appointmentId, DateTime? now = null)
    {
        _history = history; _actor = actor; _appointmentId = appointmentId; _now = now ?? DateTime.Now;
        Theme.MarkPrimitive(this); DesignPaint.EnableContainer(this);
        Name = "patientHistory"; Dock = DockStyle.Fill; AutoScroll = true; BackColor = Palette.Canvas; Padding = Space.Page;
        _content.ColumnStyles.Add(new(SizeType.Percent, 100)); Controls.Add(_content);
        _content.Controls.Add(_alert); _content.Controls.Add(_retry);
        _content.Controls.Add(_skeleton);
        _retry.Visible = false; _retry.Click += async (_, _) => await LoadHistoryAsync();
        UiMessages.RegisterAlertHost(this, _alert);
        Load += async (_, _) => await LoadHistoryAsync();
    }
    private Task LoadHistoryAsync() => UiAction.RunAsync(this, async () =>
    {
        var loaded = false;
        try
        {
            var result = await _history.GetHistoryAsync(_actor, _appointmentId);
            if (IsDisposed) return;
            if (!result.Success || result.Data is null) { UiMessages.ShowError(result); return; }
            ClearHistory(); Render(result.Data); _alert.Visible = false; _retry.Visible = false; loaded = true;
        }
        finally
        {
            if (!IsDisposed)
            {
                if (!_skeleton.IsDisposed) _skeleton.Visible = false;
                if (!loaded) { _alert.ShowMessage("Patient history could not be loaded. Try again."); _retry.Visible = true; }
            }
        }
    });
    private void ClearHistory()
    {
        foreach (var control in _content.Controls.Cast<Control>().Where(c => c != _alert && c != _retry).ToArray()) control.Dispose();
    }
    private void Render(PatientHistory history)
    {
        _content.SuspendLayout();
        try
        {
            var p = history.Selected.Patient; var a = history.Selected.Appointment;
            _content.Controls.Add(Identity(p));
            _content.Controls.Add(Entry("Selected appointment", $"{DisplayFormat.DateTime(a.AppointmentDateTime)} · {history.Selected.Dentist.FullName}",
                $"{AppointmentStatus.Display(a.Status)} · {DisplayFormat.Optional(a.Reason)}\nNotes: {DisplayFormat.Optional(a.Notes)}"));
            _content.Controls.Add(Heading("Treatment history"));
            var types = history.TreatmentTypes.ToDictionary(t => t.TreatmentTypeId, t => t.Name);
            foreach (var treatment in history.Treatments.OrderByDescending(t => t.DatePerformed).ThenByDescending(t => t.TreatmentId))
                _content.Controls.Add(Entry(DisplayFormat.Date(treatment.DatePerformed), types.GetValueOrDefault(treatment.TreatmentTypeId, "Treatment"),
                    $"Tooth: {DisplayFormat.Optional(treatment.ToothNumber)} · Gross: {DisplayFormat.Currency(treatment.Cost)} · Billed: {DisplayFormat.Currency(treatment.Net)}\nNotes: {DisplayFormat.Optional(treatment.Notes)}"));
            if (history.Treatments.Count == 0) _content.Controls.Add(NoHistory("No treatments recorded", "Recorded treatments will appear here."));
            _content.Controls.Add(Heading("Past appointments"));
            var past = history.Appointments.Where(item => item.AppointmentId != a.AppointmentId && item.AppointmentDateTime < _now)
                .OrderByDescending(item => item.AppointmentDateTime).ThenByDescending(item => item.AppointmentId).ToArray();
            foreach (var appointment in past)
                _content.Controls.Add(Entry(DisplayFormat.DateTime(appointment.AppointmentDateTime), AppointmentStatus.Display(appointment.Status),
                    $"Reason: {DisplayFormat.Optional(appointment.Reason)}\nNotes: {DisplayFormat.Optional(appointment.Notes)}"));
            if (past.Length == 0) _content.Controls.Add(NoHistory("No past appointments", "Past appointments will appear here."));
        }
        finally { _content.ResumeLayout(true); }
    }
    private static Control Identity(Patient patient)
    {
        var card = UiFactory.Card(); card.Dock = DockStyle.Top; card.Height = Metrics.HistoryRowHeight;
        var avatar = new Avatar(patient.FullName) { Anchor = AnchorStyles.Top | AnchorStyles.Left, Margin = Padding.Empty };
        var details = new Label { Text = $"{patient.FullName}\nContact: {DisplayFormat.Phone(patient.ContactNumber)}\nDate of birth: {DisplayFormat.Date(patient.DateOfBirth)}",
            Dock = DockStyle.Fill, Font = Typography.Body, ForeColor = Palette.Ink700, Padding = new Padding(Space.Lg, 0, 0, 0) };
        var identity = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Palette.Surface };
        identity.ColumnStyles.Add(new(SizeType.Absolute, Metrics.NavHeight)); identity.ColumnStyles.Add(new(SizeType.Percent, 100));
        identity.Controls.Add(avatar, 0, 0); identity.Controls.Add(details, 1, 0);
        card.Content.Controls.Add(identity); return card;
    }
    private static Control Entry(string title, string subtitle, string details)
    {
        var card = UiFactory.Card(); card.Dock = DockStyle.Top; card.AutoSize = true; card.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        card.MinimumSize = new(0, Metrics.HistoryRowHeight);
        var column = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Surface };
        column.ColumnStyles.Add(new(SizeType.Percent, 100));
        foreach (var (text, font) in new[] { (title, Typography.Label), (subtitle, Typography.Body), (details, Typography.Caption) })
        {
            var copy = new Label { Text = text, Font = font, AutoSize = true, ForeColor = Palette.Ink700, Margin = new Padding(0, Space.Xs, 0, Space.Xs) };
            column.Controls.Add(copy); column.SizeChanged += (_, _) => copy.MaximumSize = new(Math.Max(1, column.Width - Space.Sm), 0);
        }
        card.Content.Controls.Add(column);
        column.Layout += (_, _) =>
        {
            var required = column.GetPreferredSize(new(Math.Max(1, card.Content.Width), 0)).Height + card.Padding.Vertical;
            card.MinimumSize = new(0, Math.Max(Metrics.HistoryRowHeight, required));
        };
        return card;
    }
    private static Control Heading(string title) => new Label { Text = title, AutoSize = true, Font = Typography.Heading, ForeColor = Palette.Ink900, Margin = new Padding(Space.Sm, Space.Lg, 0, Space.Sm) };
    private static Control NoHistory(string title, string message) => new EmptyState(title, message, IconKind.Treatments) { Dock = DockStyle.Top };
}
