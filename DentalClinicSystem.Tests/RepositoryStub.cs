using System.Reflection;

namespace DentalClinicSystem.Tests;

// Repository fakes are separate from the service proxy used by UI fixtures.
public class RepositoryStub : DispatchProxy
{
    public Dictionary<string, Func<object?[], object?>> Handlers { get; } = [];
    public Dictionary<string, int> Calls { get; } = [];
    public Dictionary<string, object?[]> Arguments { get; } = [];
    public static T Create<T>(params (string Name, Func<object?[], object?> Handler)[] handlers) where T : class
    {
        var instance = Create<T, RepositoryStub>();
        foreach (var (name, handler) in handlers) Of(instance).Handlers[name] = handler;
        return instance;
    }
    public static RepositoryStub Of(object repository) => (RepositoryStub)repository;
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var method = targetMethod!; args ??= [];
        Calls[method.Name] = Calls.GetValueOrDefault(method.Name) + 1; Arguments[method.Name] = args;
        if (Handlers.TryGetValue(method.Name, out var handler)) return handler(args);
        if (method.ReturnType == typeof(Task)) return Task.CompletedTask;
        var type = method.ReturnType.GenericTypeArguments[0];
        object? value = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
            ? Array.CreateInstance(type.GenericTypeArguments[0], 0) : type.IsValueType ? Activator.CreateInstance(type) : null;
        return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(type).Invoke(null, [value]);
    }
}
