namespace DentalClinicSystem.Helpers;

public sealed class KeyboardShortcuts : IDisposable, IMessageFilter
{
    private const int KeyDownMessage = 0x100;
    private readonly Control _owner;
    private readonly Dictionary<Keys, Action> _actions = [];
    public KeyboardShortcuts(Control owner) { _owner = owner; Application.AddMessageFilter(this); owner.Disposed += OwnerDisposed; }
    public void Register(Keys keys, Action action) => _actions[keys] = action;
    public bool PreFilterMessage(ref Message message)
    {
        if (message.Msg != KeyDownMessage || _owner.IsDisposed || !_owner.Visible || !_owner.Enabled || !_owner.ContainsFocus) return false;
        if (!_actions.TryGetValue((Keys)(int)message.WParam | Control.ModifierKeys, out var action)) return false;
        action(); return true;
    }
    private void OwnerDisposed(object? sender, EventArgs e) => Dispose();
    public void Dispose() { Application.RemoveMessageFilter(this); _owner.Disposed -= OwnerDisposed; }
}
