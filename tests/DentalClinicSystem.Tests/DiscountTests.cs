using DentalClinicSystem.Forms;
using DentalClinicSystem.Helpers;
using DentalClinicSystem.Helpers.Design.Controls;
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
}
