using VishKat.Domain.Implementations;
using VishKat.Domain.Services;
using VishKat.Infrastructure.Repositories;
using Xunit;

namespace VishKat.Tests;

[CollectionDefinition("ConsoleTests", DisableParallelization = true)]
public class ConsoleTestsCollection { }

[Collection("ConsoleTests")]
public class InfrastructureConsoleTests
{
    // --- Тесты для InputRep ---

    [Fact]
    public void GetLine_ReadsLineAndPrintsPrompt()
    {
        var inputRep = new InputRep();
        using var sr = new StringReader("Тестовая строка\n");
        using var sw = new StringWriter();
        Console.SetIn(sr);
        Console.SetOut(sw);

        string result = inputRep.GetLine("Введите имя: ");

        Assert.Equal("Тестовая строка", result);
        Assert.Equal("Введите имя: ", sw.ToString());
    }

    [Fact]
    public void GetNumber_ValidIntegerInput_ReturnsNumber()
    {
        var inputRep = new InputRep();
        using var sr = new StringReader("42\n");
        using var sw = new StringWriter();
        Console.SetIn(sr);
        Console.SetOut(sw);

        int? result = inputRep.GetNumber("Введите число: ");

        Assert.Equal(42, result);
        Assert.Equal("Введите число: ", sw.ToString());
    }

    [Fact]
    public void GetNumber_InvalidIntegerInput_ReturnsNullAndPrintsErrorMessage()
    {
        var inputRep = new InputRep();
        using var sr = new StringReader("не_число\n");
        using var sw = new StringWriter();
        Console.SetIn(sr);
        Console.SetOut(sw);

        int? result = inputRep.GetNumber("Введите число: ");

        Assert.Null(result);
        Assert.Contains("Неудачная попытка считывания числа", sw.ToString());
    }

    // --- Тесты для OutputRep ---

    [Fact]
    public void PrintMessage_WritesCorrectMessageToConsole()
    {
        var outputRep = new OutputRep();
        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintMessage("Привет, мир!");

        Assert.Equal($"Привет, мир!{Environment.NewLine}", sw.ToString());
    }

    [Fact]
    public void PrintAllInventory_EmptyInventory_PrintsParkingIsEmpty()
    {
        var outputRep = new OutputRep();
        var kick = new KickSharing(new ServiceCenter(), new BeginnerPolicy());
        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintAllInventory(kick);

        Assert.Equal($"Паркинг пуст.{Environment.NewLine}", sw.ToString());
    }

    [Fact]
    public void PrintAllInventory_WithItems_PrintsFormattedInventory()
    {
        var outputRep = new OutputRep();
        var kick = new KickSharing(new ServiceCenter(), new BeginnerPolicy());
        kick.Add(new Bike("Stels", 1, 5));
        kick.Add(new Helmet("Шлем", 2));

        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintAllInventory(kick);

        string output = sw.ToString();
        Assert.Contains("--- НАЧАЛО ВЫВОДА ИНВЕНТОРЯ ---", output);
        Assert.Contains("Name:  Stels, Id: 1.", output);
        Assert.Contains("Name:  Шлем, Id: 2.", output);
        Assert.Contains("--- КОНЕЦ ВЫВОДА ИНВЕНТОРЯ ---", output);
    }

    [Fact]
    public void PrintAllForBeginners_EmptyBeginnerList_PrintsParkingIsEmpty()
    {
        var outputRep = new OutputRep();
        var kick = new KickSharing(new ServiceCenter(), new BeginnerPolicy());
        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintAllForBeginners(kick);

        Assert.Equal($"Паркинг пуст.{Environment.NewLine}", sw.ToString());
    }

    [Fact]
    public void PrintAllForBeginners_WithBeginnerVehicles_PrintsFormattedBeginnerVehicles()
    {
        var outputRep = new OutputRep();
        var kick = new KickSharing(new ServiceCenter(), new BeginnerPolicy());
        kick.Add(new Bike("Детский велосипед", 10, 8));

        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintAllForBeginners(kick);

        string output = sw.ToString();
        Assert.Contains("--- НАЧАЛО ВЫВОДА ТРАНСПОРТА ДЛЯ НАЧИНАЮЩИХ ---", output);
        Assert.Contains("Название:  Детский велосипед, Id: 10, Уровень простоты: 8.", output);
        Assert.Contains("--- КОНЕЦ ВЫВОДА ТРАНСПОРТА ДЛЯ НАЧИНАЮЩИХ ---", output);
    }

    [Fact]
    public void PrintConsumption_PrintsTotalConsumptionValue()
    {
        var outputRep = new OutputRep();
        var kick = new KickSharing(new ServiceCenter(), new BeginnerPolicy());
        kick.Add(new PowerBank("PB", 1, 35));

        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintConsumption(kick);

        Assert.Equal($"35{Environment.NewLine}", sw.ToString());
    }

    [Fact]
    public void PrintCount_PrintsTotalCountValue()
    {
        var outputRep = new OutputRep();
        var kick = new KickSharing(new ServiceCenter(), new BeginnerPolicy());
        kick.Add(new Helmet("Шлем 1", 1));
        kick.Add(new Helmet("Шлем 2", 2));

        using var sw = new StringWriter();
        Console.SetOut(sw);

        outputRep.PrintCount(kick);

        Assert.Equal($"2{Environment.NewLine}", sw.ToString());
    }
}
