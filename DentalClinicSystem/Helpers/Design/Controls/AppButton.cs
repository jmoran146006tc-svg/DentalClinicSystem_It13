namespace DentalClinicSystem.Helpers.Design.Controls;

public enum ButtonVariant { Primary, Secondary, Danger, Ghost }
public enum ButtonSize { Regular, Compact }

public sealed class AppButton : Button
{
    [System.ComponentModel.DefaultValue("Working…")]
    public string BusyText { get; set; } = "Working…";
    [System.ComponentModel.DefaultValue(false)]
    public bool IsBusy { get => Loading; set => ButtonStyler.SetBusy(this, value); }
    public AppButton(string text, ButtonVariant variant = ButtonVariant.Primary, IconKind? icon = null, ButtonSize size = ButtonSize.Regular)
    {
        Text = text; Height = size == ButtonSize.Compact ? Metrics.CompactHeight : Metrics.ControlHeight;
        Width = Metrics.FormWidth / 2; Margin = new Padding(Space.Xs); AccessibleName = text;
        ButtonStyler.Attach(this, variant, icon);
    }
}

public static class ButtonStyler
{
    private sealed class BusyState
    {
        public string? Text { get; set; }
        public bool Attached { get; set; }
        public int MinimumWidth { get; set; }
    }
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Button, BusyState> Busy = new();
    public static void Attach(Button button, ButtonVariant variant, IconKind? icon = null, bool preserveFont = false)
    {
        Theme.MarkPrimitive(button);
        var state = Busy.GetOrCreateValue(button);
        if (!state.Attached)
        {
            state.Attached = true; state.MinimumWidth = button.MinimumSize.Width;
            button.TextChanged += (_, _) => Measure(button, state);
            button.FontChanged += (_, _) => Measure(button, state);
            button.DpiChangedAfterParent += (_, _) => Measure(button, state);
        }
        button.Radius = Metrics.ControlRadius; button.WaveSize = Metrics.FocusRing;
        button.Type = variant == ButtonVariant.Primary ? AntdUI.TTypeMini.Primary : variant == ButtonVariant.Danger ? AntdUI.TTypeMini.Error : AntdUI.TTypeMini.Default;
        button.Ghost = variant == ButtonVariant.Ghost;
        button.BorderWidth = variant == ButtonVariant.Secondary ? Metrics.Border : 0;
        button.DefaultBack = Palette.Surface; button.DefaultBorderColor = Palette.LineStrong;
        button.BackColor = button.ForeColor = null;
        button.BackHover = variant == ButtonVariant.Ghost ? Palette.BrandSoft : null;
        if (!preserveFont) button.Font = Typography.Label;
        button.IconSvg = icon.HasValue ? AntdTheme.Svg(icon.Value) : null;
        button.Cursor = Cursors.Hand; button.TabStop = true;
        button.LoadingRespondClick = false; button.AccessibleRole = AccessibleRole.PushButton;
        button.AccessibleName ??= button.Text; button.AutoEllipsis = false;
        Measure(button, state);
    }
    private static void Measure(Button button, BusyState state)
    {
        if (button.IsDisposed || button.Loading) return;
        var busyLabel = button is AppButton app ? app.BusyText : button.Name == "btnLogin" ? "Signing in…" : "Saving…";
        var textWidth = TextRenderer.MeasureText(button.Text, button.Font).Width;
        var busyWidth = TextRenderer.MeasureText(busyLabel, button.Font).Width + Metrics.Scale(button, Metrics.IconSize + Space.Sm);
        var width = Math.Max(textWidth + (button.HasIcon ? Metrics.Scale(button, Metrics.IconSize + Space.Sm) : 0), busyWidth) + Metrics.Scale(button, Space.Lg * 2);
        button.MinimumSize = new(Math.Max(state.MinimumWidth, width), button.MinimumSize.Height);
    }
    public static void SetBusy(Button button, bool busy)
    {
        if (button.Loading == busy) return;
        var state = Busy.GetOrCreateValue(button);
        if (busy)
        {
            state.Text = button.Text;
            button.Loading = true;
            button.Text = button is AppButton app ? app.BusyText : button.Name == "btnLogin" ? "Signing in…" : "Saving…";
        }
        else if (state.Text is not null) { button.Text = state.Text; state.Text = null; }
        button.Loading = busy;
    }
    public static bool IsBusy(Button button) => button.Loading;
}
