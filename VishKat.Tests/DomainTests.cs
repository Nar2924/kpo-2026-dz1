using VishKat.Domain.Implementations;
using Xunit;

namespace VishKat.Tests;

public class DomainTests
{
    // --- Тесты для Thing и наследников ---

    [Fact]
    public void Helmet_Constructor_SetsPropertiesCorrectly()
    {
        var helmet = new Helmet("Шлем защитный", 1);

        Assert.Equal("Шлем защитный", helmet.Name);
        Assert.Equal(1, helmet.Id);
    }

    [Fact]
    public void PowerBank_ValidConsumption_SetsPropertiesCorrectly()
    {
        var pb = new PowerBank("PowerBank Xiaomi", 2, 15);

        Assert.Equal("PowerBank Xiaomi", pb.Name);
        Assert.Equal(2, pb.Id);
        Assert.Equal(15, pb.Consumption);
    }

    [Fact]
    public void PowerBank_NegativeConsumption_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PowerBank("PB", 2, -1));
    }

    [Fact]
    public void DocStation_ValidConsumption_SetsPropertiesCorrectly()
    {
        var station = new DocStation("Станция зарядки", 3, 200);

        Assert.Equal("Станция зарядки", station.Name);
        Assert.Equal(3, station.Id);
        Assert.Equal(200, station.Consumption);
    }

    [Fact]
    public void DocStation_NegativeConsumption_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocStation("Станция", 3, -5));
    }

    // --- Тесты для Vehicle и наследников ---

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Bike_ValidEase_SetsPropertiesCorrectly(int ease)
    {
        var bike = new Bike("Горный велосипед", 10, ease);

        Assert.Equal("Горный велосипед", bike.Name);
        Assert.Equal(10, bike.Id);
        Assert.Equal(ease, bike.Ease);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(11)]
    public void Vehicle_InvalidEase_ThrowsArgumentOutOfRangeException(int invalidEase)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Bike("Велосипед", 10, invalidEase));
    }

    [Fact]
    public void ElectricBike_ValidParameters_SetsPropertiesCorrectly()
    {
        var eBike = new ElectricBike("Электровелосипед", 11, 7, 45);

        Assert.Equal("Электровелосипед", eBike.Name);
        Assert.Equal(11, eBike.Id);
        Assert.Equal(7, eBike.Ease);
        Assert.Equal(45, eBike.Consumption);
    }

    [Fact]
    public void ElectricBike_NegativeConsumption_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ElectricBike("Электровелосипед", 11, 7, -10));
    }

    [Fact]
    public void ElectricalScooter_ValidParameters_SetsPropertiesCorrectly()
    {
        var scooter = new ElectricalScooter("Ninebot", 12, 8, 30);

        Assert.Equal("Ninebot", scooter.Name);
        Assert.Equal(12, scooter.Id);
        Assert.Equal(8, scooter.Ease);
        Assert.Equal(30, scooter.Consumption);
    }

    [Fact]
    public void ElectricalScooter_NegativeConsumption_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ElectricalScooter("Ninebot", 12, 8, -1));
    }
}
