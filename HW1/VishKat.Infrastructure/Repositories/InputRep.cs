using VishKat.Infrastructure.Interfaces;

namespace VishKat.Infrastructure.Repositories;

public class InputRep : IInputRep
{
    public string GetLine(string phrase)
    {
        Console.Write(phrase);
        return Console.ReadLine() ?? string.Empty;
    }

    public int? GetNumber(string phrase)
    {
        Console.Write(phrase);
        if (int.TryParse(Console.ReadLine(), out int number))
        {
            return number;
        }
        Console.WriteLine("Неудачная попытка считывания числа");
        return null;
    }
}