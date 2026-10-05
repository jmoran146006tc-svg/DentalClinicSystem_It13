namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class InlineAlert : AntdUI.Alert
{
    public InlineAlert()
    {
        Theme.MarkPrimitive(this);
        Height = Metrics.AlertHeight; Dock = DockStyle.Top;
        Font = Typography.Body; Radius = Metrics.ControlRadius; CloseIcon = true;
        TabStop = true; AccessibleRole = AccessibleRole.Alert;
        CloseChanged += (_, _) => { Dismiss(); return false; };
    }
    public void ShowMessage(string message, Semantic semantic = Semantic.Danger)
    {
        Text = message;
        Icon = semantic switch
        {
            Semantic.Success => AntdUI.TType.Success, Semantic.Warning => AntdUI.TType.Warn,
            Semantic.Info => AntdUI.TType.Info, _ => AntdUI.TType.Error
        };
        Height = Metrics.AlertHeight; Visible = true;
    }
    public void Dismiss() => Visible = false;
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Escape or Keys.Space or Keys.Enter) { Dismiss(); e.SuppressKeyPress = true; }
        base.OnKeyDown(e);
    }
}
