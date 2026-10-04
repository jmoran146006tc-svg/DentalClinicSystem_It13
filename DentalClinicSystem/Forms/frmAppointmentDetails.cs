using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class frmAppointmentDetails : DialogShell
{
    private readonly AppointmentDetails _details;
    private readonly IAppointmentService _appointments;
    private readonly User _actor;
    private readonly Func<int, Task> _refresh;
    private readonly AppButton _cancel = UiFactory.Button("Cancel appointment", ButtonVariant.Danger);
    private readonly AppButton _checkIn = UiFactory.Button("Check in", ButtonVariant.Secondary);
    private readonly AppButton _reschedule = UiFactory.Button("Reschedule", ButtonVariant.Ghost);
    private readonly InlineAlert _alert = new() { Visible = false };
    public frmAppointmentDetails(AppointmentDetails details, IAppointmentService appointments, User actor, Func<int, Task> refresh, IDentistService? dentists = null) : base("Appointment details", "Mark completed")
    {
        _details = details; _appointments = appointments; _actor = actor; _refresh = refresh;
        Size = new(Metrics.FormWidth * 2, Metrics.LoginHeight); Body.AutoScroll = true;
        var content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = Palette.Surface };
        content.ColumnStyles.Add(new(SizeType.Percent, 50)); content.ColumnStyles.Add(new(SizeType.Percent, 50));
        var a = details.Appointment; var p = details.Patient;
        content.Controls.Add(new StatusBadge(a.Status), 0, 0);
        content.Controls.Add(Copy($"{DisplayFormat.Date(a.AppointmentDateTime)}\n{DisplayFormat.Time(a.AppointmentDateTime)} – {DisplayFormat.Time(a.AppointmentDateTime.AddMinutes(a.DurationMinutes))}", Typography.Heading), 1, 0);
        var appointment = Column(("Patient", p.FullName), ("Dentist", details.Dentist.FullName), ("Reason", DisplayFormat.Optional(a.Reason)),
            ("Notes", DisplayFormat.Optional(a.Notes)), ("Cancellation reason", DisplayFormat.Optional(a.CancellationReason)));
        var patient = Column(("Patient details", p.FullName), ("Contact", DisplayFormat.Phone(p.ContactNumber)), ("Date of birth", DisplayFormat.Date(p.DateOfBirth)), ("Email", DisplayFormat.Optional(p.Email)));
        if (RoleAccess.IsAdmin(actor) || RoleAccess.IsDentist(actor))
        {
            var allergyCaption = Copy("Allergies", Typography.Label); allergyCaption.ForeColor = Palette.Danger.Text;
            var allergies = Copy(DisplayFormat.Optional(p.Allergies), Typography.Label); allergies.ForeColor = Palette.Danger.Text;
            patient.Controls.Add(allergyCaption); patient.Controls.Add(allergies);
            patient.Controls.Add(Copy("Medical notes", Typography.Label)); patient.Controls.Add(Copy(DisplayFormat.Optional(p.MedicalNotes), Typography.Body));
        }
        content.Controls.Add(appointment, 0, 1); content.Controls.Add(patient, 1, 1);
        Body.Controls.Add(content); Body.Controls.Add(_alert); _alert.BringToFront(); UiMessages.RegisterAlertHost(this, _alert);
        ConfirmButton.Visible = a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Completed);
        _checkIn.Visible = a.Status == AppointmentStatus.Scheduled && a.AppointmentDateTime.Date == DateTime.Today && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.CheckedIn);
        _cancel.Visible = a.Status == AppointmentStatus.Scheduled && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Cancelled);
        DismissButton.Text = "Close"; ButtonStyler.Attach(DismissButton, ButtonVariant.Ghost);
        Footer.Height = Metrics.FieldHeight; Footer.WrapContents = true; Footer.Controls.Add(_cancel); Footer.Controls.Add(_checkIn); Footer.Controls.Add(_reschedule);
        _reschedule.Visible = dentists is not null && a.Status == AppointmentStatus.Scheduled && RoleAccess.Can(actor, Permission.ManageAppointments);
        _reschedule.Click += async (_, _) => await UiAction.RunAsync(this, async () =>
        {
            if (dentists is null) return;
            using var dialog = new frmRescheduleAppointment(a, _appointments, dentists, _actor);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment rescheduled."); CloseAnimated(DialogResult.OK);
        }, _reschedule);
        _checkIn.Click += async (_, _) => await UiAction.RunAsync(this, CheckInAsync, _checkIn);
        ConfirmButton.Click += async (_, _) => await UiAction.RunAsync(this, CompleteAsync, ConfirmButton);
        _cancel.Click += async (_, _) => await UiAction.RunAsync(this, CancelAsync, _cancel);
    }
    private static Label Copy(string text, Font font) => new() { Text = text, Font = font, ForeColor = Palette.Ink700, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, Space.Xs, Space.Sm, Space.Sm) };
    private static Control Column(params (string Caption, string Value)[] entries)
    {
        var column = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, BackColor = Palette.Surface, Padding = new Padding(0, Space.Lg, Space.Lg, 0) };
        column.ColumnStyles.Add(new(SizeType.Percent, 100));
        foreach (var (caption, value) in entries)
        {
            column.Controls.Add(Copy(caption, Typography.Label)); var copy = Copy(value, Typography.Body); column.Controls.Add(copy);
            column.SizeChanged += (_, _) => copy.MaximumSize = new(Math.Max(1, column.Width - column.Padding.Horizontal - Space.Lg), 0);
        }
        return column;
    }
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
        using var reason = UiFactory.Reason(); if (reason.ShowDialog(this) != DialogResult.OK) return;
        var result = await _appointments.CancelAppointmentAsync(_actor, a.AppointmentId, reason.IsNoShow, reason.Reason);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment cancelled."); CloseAnimated(DialogResult.OK);
    }
}
