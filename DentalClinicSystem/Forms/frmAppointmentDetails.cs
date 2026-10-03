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
    private readonly InlineAlert _alert = new() { Visible = false };
    public frmAppointmentDetails(AppointmentDetails details, IAppointmentService appointments, User actor, Func<int, Task> refresh) : base("Appointment details", "Mark completed")
    {
        _details = details; _appointments = appointments; _actor = actor; _refresh = refresh;
        Size = new(Metrics.FormWidth * 2, Metrics.LoginHeight); Body.AutoScroll = true;
        var content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, BackColor = Palette.Surface };
        content.ColumnStyles.Add(new(SizeType.Percent, 50)); content.ColumnStyles.Add(new(SizeType.Percent, 50));
        var a = details.Appointment; var p = details.Patient;
        content.Controls.Add(new StatusBadge(a.Status), 0, 0);
        content.Controls.Add(Copy(DisplayFormat.DateTime(a.AppointmentDateTime), Typography.Heading), 1, 0);
        var appointment = Column(("Patient", p.FullName), ("Dentist", details.Dentist.FullName), ("Reason", DisplayFormat.Optional(a.Reason)),
            ("Notes", DisplayFormat.Optional(a.Notes)), ("Cancellation reason", DisplayFormat.Optional(a.CancellationReason)));
        var patient = Column(("Patient details", p.FullName), ("Contact", DisplayFormat.Phone(p.ContactNumber)), ("Date of birth", DisplayFormat.Date(p.DateOfBirth)), ("Email", DisplayFormat.Optional(p.Email)));
        content.Controls.Add(appointment, 0, 1); content.Controls.Add(patient, 1, 1);
        Body.Controls.Add(content); Body.Controls.Add(_alert); _alert.BringToFront(); UiMessages.RegisterAlertHost(this, _alert);
        ConfirmButton.Visible = a.Status == AppointmentStatus.Scheduled && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Completed);
        _cancel.Visible = a.Status == AppointmentStatus.Scheduled && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Cancelled);
        DismissButton.Text = "Close"; ButtonStyler.Attach(DismissButton, ButtonVariant.Ghost);
        Footer.Height = Metrics.FieldHeight; Footer.WrapContents = true; Footer.Controls.Add(_cancel);
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
        if (a.Status != AppointmentStatus.Scheduled || !RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Completed)) return;
        var result = await _appointments.UpdateAppointmentStatusAsync(_actor, a.AppointmentId, AppointmentStatus.Completed);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment completed."); CloseAnimated(DialogResult.OK);
    }
    private async Task CancelAsync()
    {
        var a = _details.Appointment;
        if (a.Status != AppointmentStatus.Scheduled || !RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Cancelled)) return;
        if (!UiMessages.Confirm("Are you sure you want to cancel this appointment?", "Cancel appointment")) return;
        using var reason = UiFactory.Reason(); if (reason.ShowDialog(this) != DialogResult.OK) return;
        var result = await _appointments.CancelAppointmentAsync(_actor, a.AppointmentId, reason.Reason);
        if (!result.Success) { UiMessages.ShowError(result); return; }
        await _refresh(a.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess("Appointment cancelled."); CloseAnimated(DialogResult.OK);
    }
}
