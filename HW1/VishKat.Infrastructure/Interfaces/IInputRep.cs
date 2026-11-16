namespace VishKat.Infrastructure.Interfaces;

public interface IInputRep
{
    string GetLine(string phrase);
    int? GetNumber(string phrase);
}