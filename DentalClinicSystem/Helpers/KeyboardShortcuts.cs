namespace DentalClinicSystem.Helpers;

public sealed class KeyboardShortcuts : IDisposable, IMessageFilter
{
    private const int KeyDownMessage = 0x100;
    private readonly Control _owner;
    private readonly Dictionary<Keys, Action> _actions = [];
    private IEnumerable<Control> _formInputs = [];
    private Action? _submit;
    public KeyboardShortcuts(Control owner) { _owner = owner; Application.AddMessageFilter(this); owner.Disposed += OwnerDisposed; }
    public void Register(Keys keys, Action action) => _actions[keys] = action;
    public void RegisterEnterNavigation(IEnumerable<Control> inputs, Action submit) { _formInputs = inputs; _submit = submit; }
    public bool PreFilterMessage(ref Message message)
    {
        if (message.Msg != KeyDownMessage || _owner.IsDisposed || !_owner.Visible || !_owner.Enabled || !_owner.ContainsFocus) return false;
        var keys = (Keys)(int)message.WParam | Control.ModifierKeys;
        if (keys == Keys.Enter)
        {
            var inputs = _formInputs.Where(input => input.Visible && input.Enabled).ToArray();
            var index = Array.FindIndex(inputs, input => input.ContainsFocus);
            if (index >= 0 && inputs[index] is not TextBox { Multiline: true } && inputs[index] is not ComboBox { ExpandDrop: true })
            {
                if (index + 1 < inputs.Length) inputs[index + 1].Focus(); else _submit?.Invoke();
                return true;
            }
        }
        if (!_actions.TryGetValue(keys, out var action)) return false;
        action(); return true;
    }
    private void OwnerDisposed(object? sender, EventArgs e) => Dispose();
    public void Dispose() { Application.RemoveMessageFilter(this); _owner.Disposed -= OwnerDisposed; }
}
