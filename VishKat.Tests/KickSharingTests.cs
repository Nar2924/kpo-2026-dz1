using Moq;
using VishKat.Domain.Implementations;
using VishKat.Domain.Interfaces;
using VishKat.Domain.Services;

namespace VishKat.Tests;

public class KickSharingTests
{
    
    // --- Тесты для Add --- 
    
    [Fact]
public void Add_VehiclePassesServiceCheck_AddedSuccessfully()
{
    // Arrange
    // 1. Создаем моки (заглушки) для зависимостей парка
    var mockService = new Mock<IServiceCenter>();
    var mockPolicy = new Mock<IBeginnerPolicy>();

    // 2. Настраиваем поведение: на ЛЮБОЙ объект типа Vehicle метод CheckVehicle вернет true
    mockService.Setup(s => s.CheckVehicle(It.IsAny<Vehicle>())).Returns(true);

    // 3. Передаем готовые объекты моков (свойство .Object) в тестируемый класс
    var park = new KickSharing(mockService.Object, mockPolicy.Object);
    var bike = new Bike("Stels", 1, 8); 

    // Act
    var result = park.Add(bike); 

    // Assert
    Assert.True(result, "сервисный центр одобрил велосипед");
    
    // велосипед должен был сохраниться в списке
    Assert.Equal(1, park.GetCount()); 
}

[Fact]
public void Add_VehicleFailsServiceCheck_NotAdded()
{
    // Arrange
    var mockService = new Mock<IServiceCenter>();
    var mockPolicy = new Mock<IBeginnerPolicy>();

    // Настраиваем поведение: сервисный центр бракует любой транспорт
    mockService.Setup(s => s.CheckVehicle(It.IsAny<Vehicle>())).Returns(false);

    var park = new KickSharing(mockService.Object, mockPolicy.Object);
    var badScooter = new ElectricalScooter("Kugoo", 2, 5, 200); 

    // Act
    var result = park.Add(badScooter); 

    // Assert
    Assert.False(result, "техосмотр не пройден");
    
    // сломанный транспорт не добавляется в баланс
    Assert.Equal(0, park.GetCount()); 
}

// --- Тест для GetBeginnerVehicles ---

[Fact]
public void GetBeginnerVehicles_FiltersCorrectly_BasedOnPolicy()
{
    // Arrange
    var mockService = new Mock<IServiceCenter>();
    var mockPolicy = new Mock<IBeginnerPolicy>();

    // Разрешаем добавлять любой транспорт
    mockService.Setup(s => s.CheckVehicle(It.IsAny<Vehicle>())).Returns(true);
    var park = new KickSharing(mockService.Object, mockPolicy.Object);

    var easyBike = new Bike("Easy", 1, 9); 
    var hardScooter = new ElectricalScooter("Hard", 2, 3, 100); 
    park.Add(easyBike);
    park.Add(hardScooter);

    // Настраиваем точечное поведение для конкретных объектов:
    // Политика одобряет easyBike, но бракует hardScooter
    mockPolicy.Setup(p => p.CheckBeginnerPolicy(easyBike)).Returns(true);
    mockPolicy.Setup(p => p.CheckBeginnerPolicy(hardScooter)).Returns(false);

    // Act
    var beginnerVehicles = park.GetBeginnerVehicles(); 

    // Assert
    // политика пропустила только один транспорт
    Assert.Single(beginnerVehicles);
    
    // политика вернула true для easyBike
    Assert.Contains(easyBike, beginnerVehicles); 
    
    // политика вернула false для hardScooter
    Assert.DoesNotContain(hardScooter, beginnerVehicles); 
}
    
    // --- Тесты для Remove ---

     private KickSharing CreateKickSharing()
     {
         return new KickSharing(new ServiceCenter(), new BeginnerPolicy());
     }

     
    [Fact]
    public void Remove_ExistingVehicle_ReturnsTrueAndRemovesItem()
    {
        var kick = CreateKickSharing();
        var bike = new Bike("Велосипед", 1, 7);
        kick.Add(bike);

        bool removed = kick.Remove(bike);

        Assert.True(removed);
        Assert.Equal(0, kick.GetCount());
    }

    [Fact]
    public void Remove_ExistingThing_ReturnsTrueAndRemovesItem()
    {
        var kick = CreateKickSharing();
        var helmet = new Helmet("Шлем", 2);
        kick.Add(helmet);

        bool removed = kick.Remove(helmet);

        Assert.True(removed);
        Assert.Equal(0, kick.GetCount());
    }

    [Fact]
    public void Remove_NonExistingObjects_ReturnsFalse()
    {
        var kick = CreateKickSharing();
        var bike = new Bike("Велосипед", 1, 7);
        var helmet = new Helmet("Шлем", 2);

        Assert.False(kick.Remove(bike));
        Assert.False(kick.Remove(helmet));
    }

    // --- Тесты для GetConsumption ---

    [Fact]
    public void GetConsumption_EmptyInventory_ReturnsZero()
    {
        var kick = CreateKickSharing();

        Assert.Equal(0, kick.GetConsumption());
    }

    [Fact]
    public void GetConsumption_MixedInventory_ReturnsTotalEnergyConsumption()
    {
        var kick = CreateKickSharing();
        kick.Add(new Bike("Велосипед", 1, 5)); // Не потребляет энергию
        kick.Add(new Helmet("Шлем", 2)); // Не потребляет энергию
        kick.Add(new ElectricalScooter("Самокат", 3, 6, 40)); // 40
        kick.Add(new ElectricBike("Электровелосипед", 4, 6, 50)); // 50
        kick.Add(new PowerBank("Повербанк", 5, 10)); // 10
        kick.Add(new DocStation("Док-станция", 6, 150)); // 150

        int totalConsumption = kick.GetConsumption();

        Assert.Equal(250, totalConsumption);
    }

    // --- Тесты для GetCount ---

    [Fact]
    public void GetCount_ReturnsTotalCountOfVehiclesAndThings()
    {
        var kick = CreateKickSharing();
        Assert.Equal(0, kick.GetCount());

        kick.Add(new Bike("Велосипед", 1, 5));
        kick.Add(new Helmet("Шлем", 2));
        kick.Add(new PowerBank("Повербанк", 3, 15));

        Assert.Equal(3, kick.GetCount());
    }

    // --- Тесты для GetAllInventory ---

    [Fact]
    public void GetAllInventory_ReturnsAllAddedVehiclesAndThings()
    {
        var kick = CreateKickSharing();
        var bike = new Bike("Велосипед", 1, 5);
        var helmet = new Helmet("Шлем", 2);

    }
}
