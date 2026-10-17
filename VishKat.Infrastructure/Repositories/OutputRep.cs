using VishKat.Domain.Interfaces;
using VishKat.Infrastructure.Interfaces;

namespace VishKat.Infrastructure.Repositories;

public class OutputRep : IOutputRep
{
    public void PrintMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void PrintAllInventory(IKickSharing kick)
    {
        if(kick.GetAllInventory().Count == 0) 
        {
            Console.WriteLine("Паркинг пуст.");
            return;
        }
        Console.WriteLine("--- НАЧАЛО ВЫВОДА ИНВЕНТОРЯ ---");
        foreach (var elem in kick.GetAllInventory())
        {
            Console.WriteLine($"Name:  {elem.Name}, Id: {elem.Id}.");
        }
        Console.WriteLine("--- КОНЕЦ ВЫВОДА ИНВЕНТОРЯ ---");
    }

    public void PrintAllForBeginners(IKickSharing kick)
    {
        if(kick.GetBeginnerVehicles().Count == 0) 
        {
            Console.WriteLine("Паркинг пуст.");
            return;
        }
        Console.WriteLine("--- НАЧАЛО ВЫВОДА ТРАНСПОРТА ДЛЯ НАЧИНАЮЩИХ ---");
        foreach (var elem in kick.GetBeginnerVehicles())
        {
            Console.WriteLine($"Название:  {elem.Name}, Id: {elem.Id}, Уровень простоты: {elem.Ease}.");
        }
        Console.WriteLine("--- КОНЕЦ ВЫВОДА ТРАНСПОРТА ДЛЯ НАЧИНАЮЩИХ ---");
    }

    public void PrintConsumption(IKickSharing kick)
    {
        Console.WriteLine(kick.GetConsumption());
    }

    public void PrintCount(IKickSharing kick)
    {
        Console.WriteLine(kick.GetCount());
    }
}