using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed class frmDentistTimeOff : DialogShell
{
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, MultiSelect = false, AllowUserToAddRows = false };
    private readonly DateTimePicker _from = new(), _to = new();
    private readonly TextBox _reason = new() { MaxLength = FieldLimits.TimeOffReason };
    private readonly IDentistService _service;
    private readonly User _actor;
    private readonly int _dentistId;
    private readonly AppButton _remove = UiFactory.Button("Remove", ButtonVariant.Danger);
    private sealed record Row(int TimeOffId, DateTime From, DateTime To, string? Reason);
    public frmDentistTimeOff(Dentist dentist, IDentistService service, User actor) : base($"Time off: {dentist.FullName}", "Add", new Size(720, 720))
    {
        _service = service; _actor = actor; _dentistId = dentist.DentistId;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.ColumnStyles.Add(new(SizeType.Percent, 100)); root.RowStyles.Add(new(SizeType.AutoSize)); root.RowStyles.Add(new(SizeType.Percent, 100)); root.RowStyles.Add(new(SizeType.AutoSize));
        var alert = new InlineAlert { Dock = DockStyle.Top, Visible = false }; UiMessages.RegisterAlertHost(this, alert);
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1 };
        fields.ColumnStyles.Add(new(SizeType.Percent, 100));
        foreach (var field in new[] { UiFactory.Field(_from, "From", FieldKind.Date), UiFactory.Field(_to, "To", FieldKind.Date), UiFactory.Field(_reason, "Reason (optional)") })
        { field.Dock = DockStyle.Top; fields.Controls.Add(field); }
        root.Controls.Add(alert, 0, 0); root.Controls.Add(_grid, 0, 1); root.Controls.Add(fields, 0, 2); Body.Controls.Add(root);
        foreach (var picker in new[] { _from, _to }) { picker.Format = DateTimePickerFormat.Custom; picker.CustomFormat = DisplayFormat.DatePattern; }
        GridTheme.Apply(_grid); DismissButton.Text = "Close"; Footer.Controls.Add(_remove); _remove.Enabled = false;
        _grid.SelectionChanged += (_, _) => _remove.Enabled = _grid.SelectedRows.Count > 0;
        _remove.Click += async (_, _) => await UiAction.RunAsync(this, async () =>
        {
            if (_grid.CurrentRow?.DataBoundItem is not Row row || !UiMessages.Confirm("Remove this time off?", "Remove time off")) return;
            var result = await _service.RemoveTimeOffAsync(_actor, _dentistId, row.TimeOffId);
            if (!result.Success) { UiMessages.ShowError(result); return; }
            await LoadRowsAsync(); UiMessages.ShowSuccess("Time off removed.");
        }, _remove);
        Shown += async (_, _) => await UiAction.RunAsync(this, LoadRowsAsync);
    }
    private async Task LoadRowsAsync()
    {
        var result = await _service.GetTimeOffAsync(_actor, _dentistId);
        if (IsDisposed) return;
        if (!result.Success) { UiMessages.ShowError(result); return; }
        GridHelper.Bind(_grid, (result.Data ?? []).Select(t => new Row(t.TimeOffId, t.StartDate, t.EndDate, t.Reason)), row => row.TimeOffId, "No time off recorded.", "TimeOffId");
        _grid.ClearSelection(); _remove.Enabled = false;
    }
    protected override async Task<bool> ConfirmAsync()
    {
        using var owner = UiMessages.UseOwner(this);
        var result = await _service.AddTimeOffAsync(_actor, new() { DentistId = _dentistId, StartDate = _from.Value.Date, EndDate = _to.Value.Date, Reason = _reason.Text });
        if (!result.Success) { UiMessages.ShowError(result); return false; }
        await LoadRowsAsync(); _reason.Clear(); UiMessages.ShowSuccess("Time off added."); return false;
    }
}
