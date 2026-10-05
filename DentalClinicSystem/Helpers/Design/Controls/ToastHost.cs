using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace DentalClinicSystem.Helpers.Design.Controls;

public sealed class ToastHost : IDisposable
{
    private static readonly ConditionalWeakTable<Form, ToastHost> Hosts = new();
    private readonly Form _owner;
    private readonly ConcurrentDictionary<string, byte> _ids = new();
    private ToastHost(Form owner)
    {
        _owner = owner; owner.Disposed += OwnerDisposed; owner.VisibleChanged += OwnerVisibility;
    }
    public static void Show(Form owner, string text, Semantic semantic) => Hosts.GetValue(owner, form => new ToastHost(form)).Notify(text, semantic);
    private void Notify(string text, Semantic semantic)
    {
        if (_owner.IsDisposed || !_owner.Visible) return;
        var id = Guid.NewGuid().ToString("N"); _ids.TryAdd(id, 0);
        var icon = semantic switch
        {
            Semantic.Success => AntdUI.TType.Success, Semantic.Warning => AntdUI.TType.Warn,
            Semantic.Danger => AntdUI.TType.Error, _ => AntdUI.TType.Info
        };
        var config = new AntdUI.Notification.Config(_owner, "Dental clinic", text, icon, AntdUI.TAlignFrom.TR, Typography.Body, 6)
        {
            ID = id, Radius = Metrics.CardRadius, EnableSound = false, ShowInWindow = true,
            OnClose = () => _ids.TryRemove(id, out _)
        };
        AntdUI.Notification.open(config);
    }
    private void OwnerDisposed(object? sender, EventArgs e) => Dispose();
    private void OwnerVisibility(object? sender, EventArgs e) { if (!_owner.Visible) CloseAll(); }
    private void CloseAll()
    {
        foreach (var id in _ids.Keys) AntdUI.Notification.close_id(id);
        _ids.Clear();
    }
    public void Dispose()
    {
        CloseAll(); _owner.Disposed -= OwnerDisposed; _owner.VisibleChanged -= OwnerVisibility;
    }
}
