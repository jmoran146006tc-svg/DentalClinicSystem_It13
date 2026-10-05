using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public sealed class RepositoryContractTests
{
    [Fact]
    public async Task PatientFakeHonorsDetachedReadsInactiveDuplicatesAndSoftWrites()
    {
        var repository = new MemoryPatientRepository(); var now = RescheduleTests.Now;
        await repository.AddAsync(new() { FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", DateOfBirth = now.AddYears(-25), CreatedAt = now });
        var patient = Assert.Single(await repository.GetAllAsync()); var id = patient.PatientId;
        patient.FirstName = "Changed locally"; Assert.Equal("Ana", (await repository.GetByIdAsync(id))?.FirstName);
        repository.SimulateInactivityEvent(id); Assert.Empty(await repository.GetAllAsync()); Assert.False(Assert.Single(await repository.GetAllIncludingInactiveAsync()).IsActive);
        patient.FirstName = "Ana"; patient.IsActive = true; await repository.UpdateAsync(patient); Assert.False((await repository.GetByIdAsync(id))?.IsActive);
        Assert.Single(await repository.FindByNameAndDateOfBirthAsync("ana", "santos", now.AddYears(-25)));
        var service = new PatientService(repository, new FixedTimeProvider(now));
        var duplicate = await service.AddPatientAsync(RescheduleTests.Admin, new() { FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", DateOfBirth = now.AddYears(-25) });
        Assert.Contains("inactive", duplicate.ErrorMessage); Assert.True((await service.ReactivatePatientAsync(RescheduleTests.Admin, id)).Success);
        Assert.Equal(id, Assert.Single(await repository.GetAllAsync()).PatientId);
    }
}
