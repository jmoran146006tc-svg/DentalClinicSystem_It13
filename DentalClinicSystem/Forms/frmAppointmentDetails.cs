using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class frmAppointmentDetails : DialogShell
{
    private static readonly Color Soft = Color.FromArgb(234, 249, 250);
    private static readonly Color TealDark = Color.FromArgb(31, 111, 115);
    private static readonly Color Muted = Color.FromArgb(107, 124, 128);
    private static readonly string[] CancelReasons =
        ["Patient asked to cancel", "Patient did not show up (no-show)", "Dentist unavailable", "Emergency or illness", "Rescheduled by clinic"];
    private const int NoShowIndex = 1;

    private AppointmentDetails _details;
    private readonly IAppointmentService _appointments;
    private readonly IDentistService? _dentists;
    private readonly User _actor;
    private readonly Func<int, Task> _refresh;
    private readonly AppButton _cancel = UiFactory.Button("Cancel appointment", ButtonVariant.Danger);
    private readonly AppButton _checkIn = UiFactory.Button("Check in", ButtonVariant.Secondary);
    private readonly AppButton _reschedule = UiFactory.Button("Reschedule", ButtonVariant.Ghost);
    private readonly InlineAlert _alert = new() { Visible = false };
    private readonly ComboBox _cancelReason = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Panel _badgeHost = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Anchor = AnchorStyles.Top | AnchorStyles.Right };
    private readonly Label _title = Copy("", Typography.Heading);
    private readonly Label _subtitle = Copy("", Typography.Body);
    private readonly Label _extra = Copy("", Typography.Body);
    private readonly TableLayoutPanel _cancelRow = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = Color.Transparent };
    private readonly DataGridView _history = new() { Dock = DockStyle.Top };
    private sealed record HistoryRow(int AppointmentId, DateTime Date, string Dentist, string Reason, string Status);

    public frmAppointmentDetails(AppointmentDetails details, IAppointmentService appointments, User actor, Func<int, Task> refresh, IDentistService? dentists = null) : base("Appointment details", "Mark completed")
    {
        _details = details; _appointments = appointments; _actor = actor; _refresh = refresh; _dentists = dentists;
        Size = new(Metrics.FormWidth * 3, Metrics.LoginHeight + 140); Body.AutoScroll = true;

        var root = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Surface, Padding = new Padding(0, 0, Space.Lg, Space.Lg) };
        root.ColumnStyles.Add(new(SizeType.Percent, 100));
        root.Controls.Add(BuildPatientStrip());
        root.Controls.Add(BuildSelectedCard());
        root.Controls.Add(Section("Appointments")); root.Controls.Add(_history);
        root.Controls.Add(Section("Treatment history")); root.Controls.Add(BuildTreatmentEmptyState());
        Body.Controls.Add(root); Body.Controls.Add(_alert); _alert.BringToFront(); UiMessages.RegisterAlertHost(this, _alert);

        DismissButton.Text = "Close"; ButtonStyler.Attach(DismissButton, ButtonVariant.Ghost);
        Footer.Height = Metrics.FieldHeight; Footer.WrapContents = true; Footer.Controls.Add(_checkIn); Footer.Controls.Add(_reschedule);
        ApplyDetails(details);

        _reschedule.Click += async (_, _) => await UiAction.RunAsync(this, async () =>
        {
            if (dentists is null) return;
            var a = _details.Appointment;
            using var dialog = new frmRescheduleAppointment(a, _appointments, dentists, _actor);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment rescheduled."); CloseAnimated(DialogResult.OK);
        }, _reschedule);
        _checkIn.Click += async (_, _) => await UiAction.RunAsync(this, CheckInAsync, _checkIn);
        ConfirmButton.Click += async (_, _) => await UiAction.RunAsync(this, CompleteAsync, ConfirmButton);
        _cancel.Click += async (_, _) => await UiAction.RunAsync(this, CancelAsync, _cancel);
       
        Shown += async (_, _) => await UiAction.RunAsync(this, LoadHistoryAsync);
    }

    // ---------- layout ----------
    private Control BuildPatientStrip()
    {
        var p = _details.Patient;
        var strip = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4, BackColor = Palette.Surface, Margin = Padding.Empty };
        for (var i = 0; i < 4; i++) strip.ColumnStyles.Add(new(SizeType.Percent, 25));
        strip.Controls.Add(Fact("Patient", p.FullName)); strip.Controls.Add(Fact("Contact", DisplayFormat.Phone(p.ContactNumber)));
        strip.Controls.Add(Fact("Date of birth", DisplayFormat.Date(p.DateOfBirth))); strip.Controls.Add(Fact("Email", DisplayFormat.Optional(p.Email)));
        if (RoleAccess.IsAdmin(_actor) || RoleAccess.IsDentist(_actor))
        {
            strip.Controls.Add(Fact("Allergies", DisplayFormat.Optional(p.Allergies), Palette.Danger.Text));
            var notes = Fact("Medical notes", DisplayFormat.Optional(p.MedicalNotes)); strip.Controls.Add(notes); strip.SetColumnSpan(notes, 3);
        }
        return strip;
    }

    private Control BuildSelectedCard()
    {
        var card = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = Soft, Padding = new Padding(Space.Lg), Margin = new Padding(0, Space.Lg, 0, Space.Sm) };
        card.ColumnStyles.Add(new(SizeType.Percent, 100)); card.ColumnStyles.Add(new(SizeType.AutoSize));
        var caption = Copy("SELECTED APPOINTMENT", Typography.Label); caption.ForeColor = TealDark;
        _subtitle.ForeColor = Muted; _extra.ForeColor = Muted;
        card.Controls.Add(caption, 0, 0); card.Controls.Add(_badgeHost, 1, 0);
        card.Controls.Add(_title, 0, 1); card.SetColumnSpan(_title, 2);
        card.Controls.Add(_subtitle, 0, 2); card.SetColumnSpan(_subtitle, 2);
        card.Controls.Add(_extra, 0, 3); card.SetColumnSpan(_extra, 2);

        _cancelReason.Items.AddRange(CancelReasons); _cancelReason.SelectedIndex = 0;
        var field = UiFactory.Field(_cancelReason, "Reason for cancelling", FieldKind.Choice); field.Dock = DockStyle.Fill;
        _cancelRow.ColumnStyles.Add(new(SizeType.Percent, 100)); _cancelRow.ColumnStyles.Add(new(SizeType.AutoSize));
        _cancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        _cancelRow.Controls.Add(field, 0, 0); _cancelRow.Controls.Add(_cancel, 1, 0);
        _cancelRow.Margin = new Padding(0, Space.Lg, 0, 0);
        card.Controls.Add(_cancelRow, 0, 4); card.SetColumnSpan(_cancelRow, 2);
        return card;
    }

    private static Control BuildTreatmentEmptyState()
    {
        var box = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Surface, Padding = new Padding(0, Space.Lg, 0, Space.Lg) };
        box.ColumnStyles.Add(new(SizeType.Percent, 100));
        var head = Copy("No treatment history", Typography.Label); head.AutoSize = false; head.Height = 28; head.TextAlign = ContentAlignment.MiddleCenter;
        var sub = Copy("Completed treatment records will appear here.", Typography.Body); sub.ForeColor = Muted; sub.AutoSize = false; sub.Height = 28; sub.TextAlign = ContentAlignment.MiddleCenter;
        box.Controls.Add(head); box.Controls.Add(sub);
        return box;
    }

    private static Label Section(string text) { var l = Copy(text, Typography.Heading); l.Margin = new Padding(0, Space.Lg, 0, Space.Sm); return l; }

    private static Control Fact(string caption, string value, Color? color = null)
    {
        var box = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Surface, Margin = Padding.Empty };
        box.ColumnStyles.Add(new(SizeType.Percent, 100));
        var c = Copy(caption, Typography.Label); var v = Copy(value, Typography.Body);
        if (color is { } col) { c.ForeColor = col; v.ForeColor = col; }
        box.Controls.Add(c); box.Controls.Add(v);
        return box;
    }

    private static Label Copy(string text, Font font) => new() { Text = text, Font = font, ForeColor = Palette.Ink700, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, Space.Xs, Space.Sm, Space.Sm) };

    private static string RelativeDay(DateTime when) => (when.Date - DateTime.Today).Days switch
    { 0 => "Today", 1 => "Tomorrow", -1 => "Yesterday", _ => DisplayFormat.Date(when) };

    // ---------- state ----------
    private void ApplyDetails(AppointmentDetails d)
    {
        _details = d; var a = d.Appointment;
        _title.Text = $"Appointment on {RelativeDay(a.AppointmentDateTime)} at {DisplayFormat.Time(a.AppointmentDateTime)}";
        _subtitle.Text = $"{d.Dentist.FullName} · {DisplayFormat.Optional(a.Reason)} · {DisplayFormat.Time(a.AppointmentDateTime)} – {DisplayFormat.Time(a.AppointmentDateTime.AddMinutes(a.DurationMinutes))}";
        var extra = new List<string>();
        if (!string.IsNullOrWhiteSpace(a.Notes)) extra.Add($"Notes: {a.Notes}");
        if (!string.IsNullOrWhiteSpace(a.CancellationReason)) extra.Add($"Cancellation reason: {a.CancellationReason}");
        _extra.Text = string.Join("\n", extra); _extra.Visible = extra.Count > 0;
        _badgeHost.Controls.Clear(); _badgeHost.Controls.Add(new StatusBadge(a.Status));

        ConfirmButton.Visible = a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn && RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Completed);
        _checkIn.Visible = a.Status == AppointmentStatus.Scheduled && a.AppointmentDateTime.Date == DateTime.Today && RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.CheckedIn);
        _cancelRow.Visible = a.Status == AppointmentStatus.Scheduled && RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Cancelled);
        _reschedule.Visible = _dentists is not null && a.Status == AppointmentStatus.Scheduled && RoleAccess.Can(_actor, Permission.ManageAppointments);
        foreach (DataGridViewRow r in _history.Rows) r.Selected = r.DataBoundItem is HistoryRow h && h.AppointmentId == a.AppointmentId;
    }

    private async Task LoadHistoryAsync()
    {
        var all = UiMessages.Items(await _appointments.GetAllAppointmentsAsync(_actor));
        var names = new Dictionary<int, string> { [_details.Dentist.DentistId] = _details.Dentist.FullName };
        if (_dentists is not null) foreach (var d in UiMessages.Items(await _dentists.GetAllDentistsAsync(_actor))) names[d.DentistId] = d.FullName;
        if (IsDisposed) return;
        var rows = all.Where(x => x.PatientId == _details.Patient.PatientId).OrderByDescending(x => x.AppointmentDateTime)
            .Select(x => new HistoryRow(x.AppointmentId, x.AppointmentDateTime, names.GetValueOrDefault(x.DentistId, $"Dentist #{x.DentistId}"), x.Reason ?? "Appointment", x.Status)).ToList();
        GridHelper.Bind(_history, rows, r => r.AppointmentId, "No appointments for this patient.", "AppointmentId");
       
        _history.Height = 48 + Math.Clamp(rows.Count, 1, 5) * 70;
        ApplyDetails(_details);   // re-highlight the selected row
    }

    // ---------- actions ----------
    protected override bool CanConfirm() => false; // The async service result controls closing.
    private async Task CompleteAsync()
    {
        var a = _details.Appointment;
        if (a.Status is not (AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn) || !RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Completed)) return;
        var result = await _appointments.UpdateAppointmentStatusAsync(_actor, a.AppointmentId, AppointmentStatus.Completed);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment completed."); CloseAnimated(DialogResult.OK);
    }
    private async Task CheckInAsync()
    {
        var a = _details.Appointment;
        var result = await _appointments.UpdateAppointmentStatusAsync(_actor, a.AppointmentId, AppointmentStatus.CheckedIn);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Patient checked in."); CloseAnimated(DialogResult.OK);
    }
    private async Task CancelAsync()
    {
        var a = _details.Appointment;
        if (a.Status != AppointmentStatus.Scheduled || !RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Cancelled)) return;
        if (!UiMessages.Confirm("Are you sure you want to cancel this appointment?", "Cancel appointment")) return;
        var result = await _appointments.CancelAppointmentAsync(_actor, a.AppointmentId, _cancelReason.SelectedIndex == NoShowIndex, _cancelReason.Text);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment cancelled."); CloseAnimated(DialogResult.OK);
    }
}