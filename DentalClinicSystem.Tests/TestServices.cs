using System.Reflection;
using DentalClinicSystem.Service;
using DentalClinicSystem.Interfaces;

namespace DentalClinicSystem.Tests;

public class TestServices : DispatchProxy
{
    public Dictionary<string, object> Results { get; } = [];
    public Dictionary<string, int> Calls { get; } = [];
    public Dictionary<string, object?[]> Arguments { get; } = [];
    public static T Create<T>(params (string Method, object Result)[] results) where T : class
    {
        var proxy = Create<T, TestServices>();
        foreach (var (method, result) in results) ((TestServices)(object)proxy).Results[method] = result;
        return proxy;
    }
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var method = targetMethod!; Calls[method.Name] = Calls.GetValueOrDefault(method.Name) + 1;
        Arguments[method.Name] = args ?? [];
        if (Results.TryGetValue(method.Name, out var configured)) return configured;
        var resultType = method.ReturnType.GetGenericArguments()[0];
        object? value;
        if (resultType == typeof(bool)) value = true;
        else if (resultType == typeof(ServiceResult)) value = ServiceResult.Ok();
        else
        {
            var dataType = resultType.GetGenericArguments()[0];
            var data = dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                ? Array.CreateInstance(dataType.GetGenericArguments()[0], 0) : Activator.CreateInstance(dataType);
            value = resultType.GetMethod("Ok", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!.Invoke(null, [data]);
        }
        return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, [value]);
    }
    public static AppServices Empty => new(Create<IAuthService>(), Create<IPatientService>(), Create<IDentistService>(), Create<IAppointmentService>(), Create<ITreatmentService>(), Create<ITreatmentTypeService>(), Create<IUserService>(), Create<IReportService>(), Create<IPatientHistoryService>());
}
