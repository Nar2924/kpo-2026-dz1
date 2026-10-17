using VishKat.Domain.Implementations;
using VishKat.Domain.Services;
using Xunit;

namespace VishKat.Tests;

public class PoliciesAndServicesTests
{
    // --- Тесты для BeginnerPolicy.CheckBeginnerPolicy ---

    [Fact]
    public void CheckBeginnerPolicy_NonEnergyConsumerWithHighEase_ReturnsTrue()
    {
        var policy = new BeginnerPolicy();
        var bike = new Bike("Обычный велосипед", 1, 6);

        Assert.True(policy.CheckBeginnerPolicy(bike));
    }

    [Fact]
    public void CheckBeginnerPolicy_EaseLessThan6_ReturnsFalse()
    {
        var policy = new BeginnerPolicy();
        var bike = new Bike("Сложный велосипед", 2, 5);

        Assert.False(policy.CheckBeginnerPolicy(bike));
    }

    [Fact]
    public void CheckBeginnerPolicy_ConsumptionGreaterThan50_ReturnsFalse()
    {
        var policy = new BeginnerPolicy();
        var scooter = new ElectricalScooter("Мощный самокат", 3, 8, 51);

        Assert.False(policy.CheckBeginnerPolicy(scooter));
    }

    [Fact]
    public void CheckBeginnerPolicy_Consumption50AndEase6_ReturnsTrue()
    {
        var policy = new BeginnerPolicy();
        var scooter = new ElectricalScooter("Подходящий самокат", 4, 6, 50);

        Assert.True(policy.CheckBeginnerPolicy(scooter));
    }

    // --- Тесты для ServiceCenter.CheckVehicle ---

    [Fact]
    public void CheckVehicle_ValidVehicle_ReturnsTrue()
    {
        var service = new ServiceCenter();
        var scooter = new ElectricalScooter("Обычный самокат", 1, 5, 100);

        Assert.True(service.CheckVehicle(scooter));
    }

    [Fact]
    public void CheckVehicle_ConsumptionGreaterThan100_ReturnsFalse()
    {
        var service = new ServiceCenter();
        var eBike = new ElectricBike("Слишком мощный байк", 2, 5, 101);

        Assert.False(service.CheckVehicle(eBike));
    }

    [Fact]
    public void CheckVehicle_KillerScooterName_ReturnsFalse()
    {
        var service = new ServiceCenter();
        var killer = new Bike("Самокат Самоубийца", 3, 8);

        Assert.False(service.CheckVehicle(killer));
    }
}
