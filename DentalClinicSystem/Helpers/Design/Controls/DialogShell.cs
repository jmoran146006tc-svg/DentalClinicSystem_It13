using DentalClinicSystem.Models;
using DentalClinicSystem.Helpers.Design.Motion;
using DentalClinicSystem.Helpers.Native;
using MotionSystem = DentalClinicSystem.Helpers.Design.Motion.Motion;

namespace DentalClinicSystem.Helpers.Design.Controls;

public class DialogShell : Form
{
    private bool _closing;
    private bool _accepted;
    public Panel Body { get; } = new() { Dock = DockStyle.Fill, BackColor = Palette.Surface, Padding = new Padding(Space.Xl) };
    public FlowLayoutPanel Footer { get; } = new() { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = Metrics.ControlHeight + Space.Xl, Padding = new Padding(Space.Sm), BackColor = Palette.Surface };
    public AppButton ConfirmButton { get; }
    public AppButton DismissButton { get; }
    public DialogShell(string title, string confirmText = "Confirm", Size? size = null)
    {
        Theme.MarkPrimitive(this);
        Text = title; StartPosition = FormStartPosition.CenterParent; FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false; MaximizeBox = false; ShowInTaskbar = false; BackColor = Palette.Surface;
        Size = size ?? new(Metrics.DialogWidth, Metrics.DialogHeight); MinimumSize = Size; Font = Typography.Body;
        ConfirmButton = new AppButton(confirmText); DismissButton = new AppButton("Cancel", ButtonVariant.Secondary) { DialogResult = DialogResult.Cancel };
        ConfirmButton.Click += async (_, _) =>
        {
            if (ConfirmButton.IsBusy || _accepted) return;
            DismissButton.Enabled = false;
            try
            {
                await UiAction.RunAsync(this, async () =>
                {
                    if (await ConfirmAsync() && !IsDisposed) { _accepted = true; CloseAnimated(DialogResult.OK); }
                }, ConfirmButton);
            }
            finally { if (!IsDisposed) { ConfirmButton.IsBusy = false; ConfirmButton.Enabled = !_accepted; DismissButton.Enabled = !_accepted; } }
        };
        Footer.Controls.Add(ConfirmButton); Footer.Controls.Add(DismissButton);
        Controls.Add(Body); Controls.Add(Footer);
        Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = Metrics.ControlHeight + Space.Xl, Padding = new Padding(Space.Xl, Space.Md, 0, 0), Font = Typography.Heading, ForeColor = Palette.Ink900 });
        AcceptButton = ConfirmButton; CancelButton = DismissButton; WindowChrome.Apply(this);
    }
    protected virtual bool CanConfirm() => true;
    protected virtual Task<bool> ConfirmAsync() => Task.FromResult(CanConfirm());
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e); var destination = Top;
        MotionSystem.Animator.Run(this, "dialog-enter", 0, 1, MotionSystem.Fast, Easing.EaseOutCubic, t => Opacity = Math.Clamp(t, 0, 1));
        MotionSystem.Animator.Run(this, "dialog-settle", 0, 1, MotionSystem.Base, Easing.EaseOutCubic, t => Top = destination + (int)(Metrics.Settle * (1 - t)));
    }
    public void CloseAnimated(DialogResult result = DialogResult.Cancel) { DialogResult = result; Close(); }
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (ConfirmButton.IsBusy && DialogResult != DialogResult.OK && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; return; }
        if (!_closing && MotionSystem.Enabled && e.CloseReason == CloseReason.UserClosing)
        {
            var result = DialogResult == DialogResult.None ? DialogResult.Cancel : DialogResult;
            e.Cancel = true; DialogResult = DialogResult.None;
            MotionSystem.Animator.Cancel(this, "dialog-enter"); MotionSystem.Animator.Cancel(this, "dialog-settle");
            MotionSystem.Animator.Run(this, "dialog-exit", (float)Opacity, 0, MotionSystem.Fast, Easing.EaseOutCubic,
                t => Opacity = Math.Clamp(t, 0, 1), () => { _closing = true; DialogResult = result; Close(); });
        }
        base.OnFormClosing(e);
    }
    protected override void Dispose(bool disposing) { if (disposing) MotionSystem.Animator.Cancel(this); base.Dispose(disposing); }
}

public sealed class ConfirmDialog : DialogShell
{
    public ConfirmDialog(string message, string title = "Please confirm") : base(title)
    {
        Body.Controls.Add(new Label { Text = message, Dock = DockStyle.Fill, Font = Typography.Body, ForeColor = Palette.Ink700 });
    }
}

public sealed class ReasonDialog : DialogShell
{
    private readonly ComboBox _reasons = new();
    private readonly TextBox _details = new() { MaxLength = Service.FieldLimits.Reason };
    private readonly FormField _other;
    public string Reason => _reasons.Text == CancellationReasons.Other ? _details.Text.Trim() : _reasons.Text;
    public bool IsNoShow => _reasons.Text == CancellationReasons.NoShow;
    public ReasonDialog() : base("Cancel appointment", "Confirm cancellation")
    {
        Height = Metrics.DialogHeight + Metrics.FieldHeight;
        _reasons.Items.AddRange(CancellationReasons.All); _reasons.SelectedIndex = 0;
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        var choices = FieldBox.Wrap(_reasons, "Reason", FieldKind.Choice); choices.Dock = DockStyle.Top;
        _other = FieldBox.Wrap(_details, "Details for Other"); _other.Dock = DockStyle.Top; _other.Visible = false;
        _reasons.SelectedIndexChanged += (_, _) => _other.Visible = _reasons.Text == CancellationReasons.Other;
        fields.Controls.Add(choices, 0, 0); fields.Controls.Add(_other, 0, 1); Body.Controls.Add(fields);
    }
    protected override bool CanConfirm()
    {
        if (Reason.Length > 0) return true;
        _other.SetError("Enter a reason for the cancellation."); _details.Focus(); return false;
    }
}
