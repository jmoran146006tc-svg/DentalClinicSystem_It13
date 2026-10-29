using System.ComponentModel;
using System.Drawing.Drawing2D;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design;
using DentalClinicSystem.Helpers.Design.Controls;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Forms;

public sealed record TreatmentRow(DateTime Date, string Dentist, string Procedure, string Notes);

public sealed class frmAppointmentDetails : DialogShell
{
    private static readonly Color TileFill = Color.FromArgb(232, 244, 246), CardFill = Color.FromArgb(240, 249, 248), CardLine = Color.FromArgb(205, 230, 228),
        HeadFill = Color.FromArgb(246, 250, 250), Muted = Color.FromArgb(112, 134, 140), Teal = Color.FromArgb(15, 118, 110);
    private static readonly Font Cap = new(Typography.Label.FontFamily, Math.Max(7f, Typography.Label.SizeInPoints - 1f), FontStyle.Bold), Bold = new(Typography.Body, FontStyle.Bold),
        Small = new(Typography.Body.FontFamily, Math.Max(7f, Typography.Body.SizeInPoints - 1f));
    private static readonly string[] Reasons = ["Patient asked to cancel", "No-show", "Dentist unavailable", "Rescheduled by clinic"];

    private readonly AppointmentDetails _details;
    private readonly IAppointmentService _appointments;
    private readonly IDentistService? _dentists;
    private readonly User _actor;
    private readonly Func<int, Task> _refresh;
    private readonly AppButton _cancel = UiFactory.Button("Cancel appointment", ButtonVariant.Danger);
    private readonly AppButton _checkIn = UiFactory.Button("Check in", ButtonVariant.Secondary);
    private readonly AppButton _reschedule = UiFactory.Button("Reschedule", ButtonVariant.Secondary);
    private readonly ComboBox _reason = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly InlineAlert _alert = new() { Visible = false };
    private readonly TableLayoutPanel _tables = Stack();
    private readonly bool _loadHistory, _loadTreatments;

    public frmAppointmentDetails(AppointmentDetails details, IAppointmentService appointments, User actor, Func<int, Task> refresh, IDentistService? dentists = null,
        IReadOnlyList<AppointmentDetails>? history = null, IReadOnlyList<TreatmentRow>? treatments = null) : base("Appointment details", "Mark completed")
    {
        _details = details; _appointments = appointments; _actor = actor; _refresh = refresh; _dentists = dentists;
        Size = new(Metrics.FormWidth * 2 + 160, Metrics.LoginHeight + 160); Body.AutoScroll = true;
        var a = details.Appointment; var p = details.Patient; var s = a.AppointmentDateTime;
        var clinical = RoleAccess.IsAdmin(actor) || RoleAccess.IsDentist(actor);
        var canCancel = a.Status == AppointmentStatus.Scheduled && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Cancelled);
        _reason.Items.AddRange(Reasons); _reason.SelectedIndex = 0;

        // header: name + subtitle (left), Close (right)
        var root = Stack(); var head = Grid(100, 0); head.Padding = new Padding(Space.Lg, Space.Sm, Space.Lg, Space.Sm); var names = Stack();
        names.Controls.Add(Txt(p.FullName, Typography.Heading, Palette.Ink700, Palette.Surface)); names.Controls.Add(Txt("Patient information", Typography.Body, Muted, Palette.Surface));
        var close = UiFactory.Button("Close", ButtonVariant.Secondary); close.Anchor = AnchorStyles.Top | AnchorStyles.Right; close.Click += (_, _) => CloseAnimated(DialogResult.Cancel);
        head.Controls.Add(names, 0, 0); head.Controls.Add(close, 1, 0); root.Controls.Add(head); root.Controls.Add(Rule());

        // info tiles (optional patient fields are read by name, so a missing property simply hides its tile)
        var top = Stack(); top.Padding = new Padding(Space.Lg, 0, Space.Lg, Space.Lg);
        var sex = Prop(p, "Sex", "Gender"); var emName = Prop(p, "EmergencyContactName", "EmergencyContact"); var emNo = Prop(p, "EmergencyContactNumber", "EmergencyContactPhone", "EmergencyPhone");
        var tiles = Grid(25, 25, 25, 25); tiles.Margin = new Padding(0, Space.Sm, 0, 0);
        tiles.Controls.Add(Tile("Age / Sex", $"{(int)((DateTime.Today - p.DateOfBirth).TotalDays / 365.25)} years{(sex is null ? "" : " · " + sex)}"), 0, 0);
        tiles.Controls.Add(Tile("Birthdate", DisplayFormat.Date(p.DateOfBirth)), 1, 0);
        tiles.Controls.Add(Tile("Phone", DisplayFormat.Phone(p.ContactNumber)), 2, 0);
        tiles.Controls.Add(Tile("Email", DisplayFormat.Optional(p.Email)), 3, 0);
        var extra = new List<Control>();
        void Opt(string caption, string? value, Color? fore = null) { if (!string.IsNullOrWhiteSpace(value)) extra.Add(Tile(caption, value, fore)); }
        Opt("Address", Prop(p, "Address"));
        Opt("Emergency contact", string.Join(" · ", new[] { emName, emNo }.Where(x => !string.IsNullOrWhiteSpace(x))));
        Opt("Registered date", Prop(p, "RegisteredDate", "DateRegistered", "CreatedAt", "CreatedDate", "DateCreated"));
        if (clinical) Opt("Allergies", DisplayFormat.Optional(p.Allergies), Palette.Danger.Text);
        for (var i = 0; i < extra.Count; i++) tiles.Controls.Add(extra[i], i % 4, 1 + i / 4);
        if (clinical) Span(tiles, Tile("Medical notes", DisplayFormat.Optional(p.MedicalNotes)), 0, 1 + (extra.Count + 3) / 4, 4);
        top.Controls.Add(tiles);

        // selected appointment card
        var card = new Round { Fill = CardFill, Stroke = CardLine, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(Space.Lg), Margin = new Padding(0, Space.Sm, 0, 0) };
        var c = Grid(100, 0); c.BackColor = CardFill; var r = 0;
        void Row(Control ctl) => Span(c, ctl, 0, r++, 2);
        c.Controls.Add(Txt("SELECTED APPOINTMENT", Cap, Teal, CardFill), 0, 0);
        c.Controls.Add(new StatusBadge(a.Status) { Anchor = AnchorStyles.Top | AnchorStyles.Right }, 1, r++);
        Row(Txt($"Appointment on {(s.Date == DateTime.Today ? "Today" : DisplayFormat.Date(s))} at {DisplayFormat.Time(s)}", Typography.Body, Palette.Ink700, CardFill));
        Row(Txt($"{details.Dentist.FullName}  ·  {DisplayFormat.Optional(a.Reason)}", Small, Muted, CardFill));
        if (!string.IsNullOrWhiteSpace(a.Notes)) Row(Txt($"Notes: {a.Notes}", Typography.Body, Palette.Ink700, CardFill));
        if (!string.IsNullOrWhiteSpace(a.CancellationReason)) Row(Txt($"Cancellation reason: {a.CancellationReason}", Typography.Body, Palette.Danger.Text, CardFill));
        if (canCancel)
        {
            var line = Rule(); line.Margin = new Padding(0, Space.Lg, 0, Space.Sm); Row(line);
            Row(Txt("Reason for cancelling", Bold, Palette.Ink700, CardFill));
            _reason.Anchor = AnchorStyles.Left | AnchorStyles.Right; _reason.Margin = new Padding(0, Space.Xs, Space.Sm, 0); _cancel.Anchor = AnchorStyles.Right;
            c.Controls.Add(_reason, 0, r); c.Controls.Add(_cancel, 1, r);
        }
        card.Controls.Add(c); top.Controls.Add(card); root.Controls.Add(top); root.Controls.Add(Rule());

        // full-width tables: filled now with what we have, then (if not supplied) loaded from the service in OnShown
        _loadHistory = history is null; _loadTreatments = treatments is null;
        RenderTables(history ?? new[] { details }, treatments ?? Treated(new[] { details }));
        root.Controls.Add(_tables);

        Body.Controls.Add(root); Body.Controls.Add(_alert); _alert.BringToFront(); UiMessages.RegisterAlertHost(this, _alert);
        ConfirmButton.Visible = a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.Completed);
        _checkIn.Visible = a.Status == AppointmentStatus.Scheduled && s.Date == DateTime.Today && RoleAccess.CanChangeStatus(actor, a, AppointmentStatus.CheckedIn);
        _cancel.Visible = canCancel;
        _reschedule.Visible = dentists is not null && a.Status == AppointmentStatus.Scheduled && RoleAccess.Can(actor, Permission.ManageAppointments);
        GroupFooter([_reschedule], [_checkIn, ConfirmButton]);

        _reschedule.Click += async (_, _) => await UiAction.RunAsync(this, async () =>
        {
            using var dialog = new frmRescheduleAppointment(a, _appointments, dentists!, _actor);
            if (dialog.ShowDialog(this) == DialogResult.OK) await Done("Appointment rescheduled.");
        }, _reschedule);
        _checkIn.Click += async (_, _) => await UiAction.RunAsync(this, CheckInAsync, _checkIn);
        ConfirmButton.Click += async (_, _) => await UiAction.RunAsync(this, CompleteAsync, ConfirmButton);
        _cancel.Click += async (_, _) => await UiAction.RunAsync(this, CancelAsync, _cancel);
    }

    // ---- patient history (loaded from IAppointmentService) ----
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!_loadHistory) return;
        try { await LoadHistoryAsync(); } catch { /* keep the rows already shown */ }
    }

    private async Task LoadHistoryAsync()
    {
        var current = _details.Appointment;
        if (!TryUnwrap(await _appointments.GetAllAppointmentsAsync(_actor), out IReadOnlyList<Appointment> all)) return;
        var rows = new List<AppointmentDetails>();
        foreach (var ap in all.Where(x => x.PatientId == current.PatientId).OrderByDescending(x => x.AppointmentDateTime))
        {
            if (ap.AppointmentId == current.AppointmentId) { rows.Add(_details); continue; }
            if (TryUnwrap(await _appointments.GetDetailsAsync(_actor, ap.AppointmentId), out AppointmentDetails d)) rows.Add(d);
        }
        if (IsDisposed || rows.Count == 0) return;
        RenderTables(rows, _loadTreatments ? Treated(rows) : new List<TreatmentRow>());
    }

    private void RenderTables(IReadOnlyList<AppointmentDetails> history, IReadOnlyList<TreatmentRow> treatments)
    {
        _tables.SuspendLayout();
        foreach (var old in _tables.Controls.Cast<Control>().ToList()) { _tables.Controls.Remove(old); old.Dispose(); }
        _tables.Controls.Add(Table("Appointments", [24, 22, 28, 18, 0], ["DATE", "DENTIST", "REASON", "STATUS", ""], history.OrderByDescending(h => h.Appointment.AppointmentDateTime).Select(HistoryRow)));
        _tables.Controls.Add(Table("Treatment history", [18, 18, 18, 46], ["DATE", "DENTIST", "PROCEDURE", "NOTES"], treatments.OrderByDescending(t => t.Date).Select(t => new object?[] { DisplayFormat.Date(t.Date), t.Dentist, t.Procedure, DisplayFormat.Optional(t.Notes) })));
        _tables.ResumeLayout(true);
    }

    /// <summary>Stopgap treatment history: each completed appointment (reason = procedure, notes = notes).</summary>
    private static List<TreatmentRow> Treated(IEnumerable<AppointmentDetails> rows) => rows.Where(r => r.Appointment.Status == AppointmentStatus.Completed)
        .Select(r => new TreatmentRow(r.Appointment.AppointmentDateTime, r.Dentist.FullName, r.Appointment.Reason ?? "", r.Appointment.Notes ?? "")).ToList();

    /// <summary>Reads the payload of a ServiceResult&lt;T&gt; (tries Value / Data / Result). Replace with e.g. result.Value if you prefer.</summary>
    private static bool TryUnwrap<T>(object result, out T value)
    {
        foreach (var name in new[] { "Value", "Data", "Result" })
            if (result.GetType().GetProperty(name)?.GetValue(result) is T v) { value = v; return true; }
        value = default!; return false;
    }

    private object?[] HistoryRow(AppointmentDetails h)
    {
        var ap = h.Appointment; var w = ap.AppointmentDateTime;
        var open = UiFactory.Button("Open", ButtonVariant.Secondary); open.Anchor = AnchorStyles.Right; open.Margin = new Padding(0, 0, Space.Lg, 0);
        open.Click += (_, _) => { using var d = new frmAppointmentDetails(h, _appointments, _actor, _refresh, _dentists); if (d.ShowDialog(this) == DialogResult.OK) CloseAnimated(DialogResult.OK); };
        return [TwoLine(w.Date == DateTime.Today ? "Today" : DisplayFormat.Date(w), DisplayFormat.Time(w)), h.Dentist.FullName, DisplayFormat.Optional(ap.Reason), new StatusBadge(ap.Status) { Anchor = AnchorStyles.Left }, open];
    }

    // ---- building blocks ----
    private static TableLayoutPanel Grid(params float[] w)
    {
        var g = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = w.Length, BackColor = Palette.Surface, Margin = new Padding(0) };
        foreach (var x in w) g.ColumnStyles.Add(x > 0 ? new ColumnStyle(SizeType.Percent, x) : new ColumnStyle(SizeType.AutoSize));
        return g;
    }
    private static TableLayoutPanel Stack() => Grid(100);
    private static Panel Rule() => new() { BackColor = CardLine, Height = 1, Dock = DockStyle.Fill, Margin = new Padding(0) };
    private static void Span(TableLayoutPanel t, Control ctl, int col, int row, int span) { t.Controls.Add(ctl, col, row); t.SetColumnSpan(ctl, span); }
    private static Control TwoLine(string top, string bottom)
    {
        var t = Stack(); t.Dock = DockStyle.Fill;
        t.Controls.Add(Txt(top, Typography.Body, Palette.Ink700, Palette.Surface)); t.Controls.Add(Txt(bottom, Small, Muted, Palette.Surface));
        return t;
    }
    private static Label Txt(string text, Font font, Color fore, Color back) =>
        new() { Text = text, Font = font, ForeColor = fore, BackColor = back, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, Space.Xs, Space.Sm, Space.Xs) };
    private static Label Cell(string t, bool head) =>
        new() { Text = t, Font = head ? Cap : Typography.Body, ForeColor = head ? Muted : Palette.Ink700, BackColor = head ? HeadFill : Palette.Surface, AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(Space.Sm), Margin = new Padding(0) };

    /// <summary>Reads an optional property by name (first match wins); returns null when the model doesn't have it.</summary>
    private static string? Prop(object o, params string[] names)
    {
        foreach (var n in names)
            if (o.GetType().GetProperty(n)?.GetValue(o) is { } v)
                return v switch { DateTime d => DisplayFormat.Date(d), DateOnly d => DisplayFormat.Date(d.ToDateTime(TimeOnly.MinValue)), _ => v.ToString() };
        return null;
    }

    private static Control Tile(string caption, string value, Color? fore = null)
    {
        var t = new Round { Fill = TileFill, Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(Space.Lg, Space.Sm, Space.Sm, Space.Sm), Margin = new Padding(0, 0, Space.Sm, Space.Sm) };
        var s = Stack(); s.BackColor = TileFill; var v = Txt(value, Bold, fore ?? Palette.Ink700, TileFill);
        s.Controls.Add(Txt(caption.ToUpperInvariant(), Cap, Muted, TileFill)); s.Controls.Add(v); t.Controls.Add(s);
        t.SizeChanged += (_, _) => v.MaximumSize = new(Math.Max(1, t.Width - t.Padding.Horizontal - Space.Sm), 0);
        return t;
    }

    /// <summary>Header row + data rows; each cell is a string (becomes a label), a Control, or null (empty).</summary>
    private static Control Table(string title, float[] widths, string[] heads, IEnumerable<object?[]> rows)
    {
        var wrap = Stack(); var g = Grid(widths);
        wrap.Controls.Add(new Label { Text = title, Font = Typography.Heading, ForeColor = Palette.Ink700, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(Space.Lg, Space.Lg, 0, Space.Sm) });
        void Add(IEnumerable<object?> cells, bool head)
        {
            var row = g.RowCount; g.RowCount += 2; g.RowStyles.Add(new RowStyle(SizeType.AutoSize)); g.RowStyles.Add(new RowStyle(SizeType.AutoSize)); var i = 0;
            foreach (var cell in cells)
            {
                if (cell is not null)
                {
                    var ctl = cell as Control ?? Cell(cell.ToString()!, head);
                    if (i == 0) ctl.Padding = new Padding(Space.Lg, Space.Sm, Space.Sm, Space.Sm);
                    g.Controls.Add(ctl, i, row);
                }
                i++;
            }
            Span(g, Rule(), 0, row + 1, g.ColumnCount);
        }
        Add(heads, true); var list = rows.ToList(); foreach (var r in list) Add(r, false);
        if (list.Count == 0) { g.RowCount++; g.RowStyles.Add(new RowStyle(SizeType.AutoSize)); var none = Cell("No records yet.", false); none.Padding = new Padding(Space.Lg, Space.Sm, Space.Sm, Space.Sm); Span(g, none, 0, g.RowCount - 1, g.ColumnCount); }
        wrap.Controls.Add(g); return wrap;
    }

    private sealed class Round : Panel
    {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color Fill { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] public Color Stroke { get; set; } = Color.Transparent;
        public Round() { DoubleBuffered = true; ResizeRedraw = true; BackColor = Palette.Surface; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1); const int d = 24; using var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right - d, r.Y, d, d, 270, 90); p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); p.AddArc(r.X, r.Bottom - d, d, d, 90, 90); p.CloseFigure();
            using var b = new SolidBrush(Fill); e.Graphics.FillPath(b, p);
            if (Stroke != Color.Transparent) { using var pen = new Pen(Stroke); e.Graphics.DrawPath(pen, p); }
        }
    }

    // ---- actions ----
    protected override bool CanConfirm() => false; // The async service result controls closing.
    private async Task Done(string message) { await _refresh(_details.Appointment.AppointmentId); using (UiMessages.UseOwner(Owner ?? this)) UiMessages.ShowSuccess(message); CloseAnimated(DialogResult.OK); }

    private async Task CompleteAsync()
    {
        var a = _details.Appointment;
        if (a.Status is not (AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn) || !RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Completed)) return;
        var result = await _appointments.UpdateAppointmentStatusAsync(_actor, a.AppointmentId, AppointmentStatus.Completed);
        if (!result.Success) UiMessages.ShowError(result); else await Done("Appointment completed.");
    }

    private async Task CheckInAsync()
    {
        var result = await _appointments.UpdateAppointmentStatusAsync(_actor, _details.Appointment.AppointmentId, AppointmentStatus.CheckedIn);
        if (!result.Success) UiMessages.ShowError(result); else await Done("Patient checked in.");
    }

    private async Task CancelAsync()
    {
        var a = _details.Appointment;
        if (a.Status != AppointmentStatus.Scheduled || !RoleAccess.CanChangeStatus(_actor, a, AppointmentStatus.Cancelled)) return;
        if (!UiMessages.Confirm("Are you sure you want to cancel this appointment?", "Cancel appointment")) return;
        var choice = _reason.SelectedItem?.ToString() ?? Reasons[0];
        var result = await _appointments.CancelAppointmentAsync(_actor, a.AppointmentId, choice == "No-show", choice);
        if (!result.Success) UiMessages.ShowError(result); else await Done("Appointment cancelled.");
    }
}