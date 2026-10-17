using VishKat.Domain.Interfaces;

namespace VishKat.Infrastructure.Interfaces;

public interface IOutputRep
{
    void PrintMessage(string message);
    void PrintAllInventory(IKickSharing kick);
    void PrintAllForBeginners(IKickSharing kick);
    public void PrintConsumption(IKickSharing kick);
    public void PrintCount(IKickSharing kick);
}