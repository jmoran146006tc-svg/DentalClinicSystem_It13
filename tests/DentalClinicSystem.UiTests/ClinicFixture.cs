using System.Reflection;
using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.UiTests;

// No credentials, database connections, or production patient records.
public class ServiceStub : DispatchProxy
{
    public Dictionary<string, object> Results { get; } = [];
    public Dictionary<string, object?[]> Calls { get; } = [];
    public static T For<T>(params (string Method, object Result)[] results) where T : class
    {
        var service = Create<T, ServiceStub>();
        foreach (var (method, result) in results) ((ServiceStub)(object)service).Results[method] = result;
        return service;
    }
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var method = targetMethod!; Calls[method.Name] = args ?? [];
        if (Results.TryGetValue(method.Name, out var result)) return result;
        var resultType = method.ReturnType.GetGenericArguments()[0];
        if (resultType == typeof(bool)) return Task.FromResult(true);
        if (resultType == typeof(ServiceResult)) return Task.FromResult(ServiceResult.Ok());
        var dataType = resultType.GetGenericArguments()[0];
        var data = dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
            ? Array.CreateInstance(dataType.GetGenericArguments()[0], 0) : null;
        var response = resultType.GetMethod("Ok", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!.Invoke(null, [data]);
        return typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, [response]);
    }
}

internal static class ClinicFixture
{
    public static User Actor(string role = Roles.Admin) => new() { UserId = 1, Username = "review", Role = role, DentistId = 2 };
    public static AppServices Services(bool filled = true)
    {
        var patient = new Patient { PatientId = 3, FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", DateOfBirth = new(1998, 4, 12), Email = "ana@example.test" };
        var dentist = new Dentist { DentistId = 2, FirstName = "Miguel", LastName = "Reyes", Specialization = "General dentistry", ContactNumber = "09181234567", LicenseNumber = "PRC-12345" };
        var appointment = new Appointment { AppointmentId = 4, PatientId = 3, DentistId = 2, AppointmentDateTime = DateTime.Today.AddDays(1).AddHours(10), Status = AppointmentStatus.Completed, Reason = "Consultation & Check up" };
        var patients = Task.FromResult(ServiceResult<IReadOnlyList<Patient>>.Ok(filled ? [patient] : []));
        return new(
            ServiceStub.For<IAuthService>(),
            ServiceStub.For<IPatientService>((nameof(IPatientService.GetAllPatientsAsync), patients), (nameof(IPatientService.GetAllIncludingInactiveAsync), patients)),
            ServiceStub.For<IDentistService>((nameof(IDentistService.GetAllDentistsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Dentist>>.Ok(filled ? [dentist] : [])))),
            ServiceStub.For<IAppointmentService>((nameof(IAppointmentService.GetAllAppointmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Appointment>>.Ok(filled ? [appointment] : []))), (nameof(IAppointmentService.GetDetailsAsync), Task.FromResult(ServiceResult<AppointmentDetails>.Ok(new(appointment, patient, dentist))))),
            ServiceStub.For<ITreatmentService>((nameof(ITreatmentService.GetAllTreatmentsAsync), Task.FromResult(ServiceResult<IReadOnlyList<Treatment>>.Ok(filled ? [new() { TreatmentId = 7, AppointmentId = 4, TreatmentTypeId = 8, Cost = 1250.5m, DatePerformed = DateTime.Today, ToothNumber = "11" }] : [])))),
            ServiceStub.For<ITreatmentTypeService>((nameof(ITreatmentTypeService.GetAllTreatmentTypesAsync), Task.FromResult(ServiceResult<IReadOnlyList<TreatmentType>>.Ok(filled ? [new() { TreatmentTypeId = 8, Name = "Cleaning", DefaultCost = 1250 }] : [])))),
            ServiceStub.For<IUserService>((nameof(IUserService.GetAllUsersAsync), Task.FromResult(ServiceResult<IReadOnlyList<User>>.Ok(filled ? [new() { UserId = 1, Username = "admin", Role = Roles.Admin, PasswordHash = "must never appear" }] : [])))),
            ServiceStub.For<IReportService>(), ServiceStub.For<IPatientHistoryService>());
    }
}
