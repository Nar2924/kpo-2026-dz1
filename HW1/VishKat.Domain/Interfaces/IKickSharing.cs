using VishKat.Domain.Implementations;

namespace VishKat.Domain.Interfaces;

public interface IKickSharing
{
    bool Add(IInventoryObject? obj);
    bool Remove(IInventoryObject obj);
    int GetConsumption();
    int GetCount();
    List<Vehicle> GetBeginnerVehicles();
    List<IInventoryObject> GetAllInventory();

}