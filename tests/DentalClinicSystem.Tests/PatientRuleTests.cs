using DentalClinicSystem.Interfaces;
using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class PatientRuleTests
{
    private static readonly DateTime Now = RescheduleTests.Now;
    private static Patient Patient(DateTime dob) => new() { PatientId = 1, FirstName = "Ana", LastName = "Santos", ContactNumber = "09171234567", DateOfBirth = dob };
}
