using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class frmRescheduleAppointment : DialogShell
{
    private readonly ComboBox _dentist = new();
    private readonly DateTimePicker _when = new() { Format = DateTimePickerFormat.Custom, CustomFormat = DisplayFormat.DateTimePattern };
    private readonly Appointment _appointment;
    private readonly IAppointmentService _appointments;
    private readonly User _actor;
    private IReadOnlyList<Dentist> _dentists = [];
    private int _version;
    public frmRescheduleAppointment(Appointment appointment, IAppointmentService appointments, IDentistService dentists, User actor)
        : base("Reschedule appointment", "Reschedule")
    {
        _appointment = appointment; _appointments = appointments; _actor = actor;
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
        fields.ColumnStyles.Add(new(SizeType.Percent, 100));
        foreach (var field in new[] { UiFactory.Field(_dentist, Roles.Dentist, FieldKind.Choice), UiFactory.Field(_when, "Date and time", FieldKind.Date) })
        { field.Dock = DockStyle.Top; fields.Controls.Add(field); }
        var alert = new InlineAlert { Dock = DockStyle.Top, Visible = false };
        Body.Controls.Add(fields); Body.Controls.Add(alert); UiMessages.RegisterAlertHost(this, alert);
        _when.Value = appointment.AppointmentDateTime;
        _dentist.DisplayMember = nameof(DisplayOption.Display); _dentist.ValueMember = nameof(DisplayOption.Id);
        ConfirmButton.Enabled = false;
        Shown += async (_, _) => await UiAction.RunAsync(this, async () =>
        {
            var result = await dentists.GetAllDentistsAsync(actor);
            if (!result.Success) { UiMessages.ShowError(result); return; }
            _dentists = result.Data ?? []; await RefreshAvailabilityAsync();
            if (!IsDisposed) { _dentist.SelectedValue = appointment.DentistId; ConfirmButton.Enabled = true; }
        });
        _when.ValueChanged += async (_, _) => await UiAction.RunAsync(this, RefreshAvailabilityAsync);
    }
    private async Task RefreshAvailabilityAsync()
    {
        var version = ++_version; var when = _when.Value; var selected = _dentist.SelectedValue as int?;
        var options = await Task.WhenAll(_dentists.Where(d => d.IsActive).Select(async d =>
        {
            var result = await _appointments.ValidateSlotAsync(d.DentistId, when, _appointment.DurationMinutes, _appointment.AppointmentId);
            var label = result.Success ? "" : result.ErrorMessage.EndsWith(ClinicRules.TimeOffSuffix, StringComparison.Ordinal) ? " (off)" : " (busy)";
            return new DisplayOption(d.DentistId, d.FullName + label);
        }));
        if (IsDisposed || version != _version) return;
        _dentist.DataSource = options; if (selected is int id) _dentist.SelectedValue = id;
    }
    protected override async Task<bool> ConfirmAsync()
    {
        using var owner = UiMessages.UseOwner(this);
        if (_dentist.SelectedValue is not int id) { UiMessages.ShowError(ServiceResult.Fail("Pick a dentist first.")); return false; }
        var result = await _appointments.RescheduleAppointmentAsync(_actor, _appointment.AppointmentId, _when.Value, id);
        if (!result.Success) UiMessages.ShowError(result);
        return result.Success;
    }
}
