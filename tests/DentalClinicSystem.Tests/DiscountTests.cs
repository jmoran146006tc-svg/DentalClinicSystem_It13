using DentalClinicSystem.Models;
using DentalClinicSystem.Service;

namespace DentalClinicSystem.Tests;

public class DiscountTests
{
    [Theory]
    [InlineData(DiscountTypes.None, 0, true)] [InlineData(DiscountTypes.None, 1, false)]
    [InlineData(DiscountTypes.Senior, 20, true)] [InlineData(DiscountTypes.Pwd, 20, true)]
    [InlineData(DiscountTypes.Other, 0, true)] [InlineData(DiscountTypes.Other, 100, true)]
    [InlineData(DiscountTypes.Other, -1, false)] [InlineData(DiscountTypes.Other, 101, false)]
    [InlineData("Unknown", 20, false)]
    public void TreatmentDiscountRules(string type, int percent, bool expected) =>
        Assert.Equal(expected, Validator.Treatment(new() { DiscountType = type, DiscountPercent = percent, DatePerformed = RescheduleTests.Now }, RescheduleTests.Now).Success);
    [Fact]
    public void NetPreservesGrossCostAndSupportsDecimalPercent()
    {
        var treatment = new Treatment { Cost = 1000, DiscountType = DiscountTypes.Other, DiscountPercent = 12.50m };
        Assert.Equal(875m, treatment.Net); Assert.Equal(1000m, treatment.Cost);
        treatment.DiscountPercent = 100; Assert.Equal(0, treatment.Net);
        treatment.DiscountPercent = 0; Assert.Equal(1000, treatment.Net);
        Assert.False(Validator.Discount(DiscountTypes.Other, 12.345m).Success);
    }
    [Fact]
    public void ReportsExposeBilledPropertiesWhileKeepingTypes()
    {
        Assert.Equal(800, new RevenueDay(RescheduleTests.Now, 800).Billed);
        Assert.Equal(800, new TopTreatmentType("Cleaning", 1, 800).Billed);
        Assert.Equal(800, new DentistWorkload("Ana", 1, 1, 800).Billed);
    }
}
