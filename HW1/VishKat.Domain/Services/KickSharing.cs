using VishKat.Domain.Implementations;
using VishKat.Domain.Interfaces;

namespace VishKat.Domain.Services;

public class KickSharing : IKickSharing
{
    private readonly List<Vehicle> _vehicles = new List<Vehicle>();
    private readonly List<Thing> _things = new List<Thing>();
    private readonly IServiceCenter _checkingSystem;
    private readonly IBeginnerPolicy _beginnerPolicy;

    public KickSharing(IServiceCenter service, IBeginnerPolicy beginnerPolicy)
    {
        _checkingSystem = service;
        _beginnerPolicy = beginnerPolicy;
    }
    
    public bool Add(IInventoryObject? obj)
    {
        if (obj is null) {return false;}
        foreach (var tmp in _vehicles)
        {
            if (obj.Id == tmp.Id) { return false; }
        }

        foreach (var tmp in _things)
        {
            if (obj.Id == tmp.Id) { return false; }
        }
        
        switch (obj)
        {
            case Vehicle vehicle when _checkingSystem.CheckVehicle(vehicle):
                _vehicles.Add(vehicle);
                return true;
            case Thing thing:
                _things.Add(thing);
                return true;
        }
        return false;
    }

    public bool Remove(IInventoryObject obj)
    {
        switch (obj)
        {
            case Vehicle vehicle when _vehicles.Count > 0 && _vehicles.Contains(vehicle):
                return _vehicles.Remove(vehicle);

            case Thing thing when _things.Count > 0:
                return _things.Remove(thing);
        }
        return false;
    }

    public int GetConsumption()
    {
        int result = 0;
        foreach (var vehicle in _vehicles)
        {
            if (vehicle is IEnergyConsumer consumer)
            {
                result += consumer.Consumption;
            }
        }

        foreach (var thing in _things)
        {
            if (thing is IEnergyConsumer consumer)
            {
                result += consumer.Consumption;
            }
        }
        return result;
    }

    public int GetCount()
    {
        return _vehicles.Count + _things.Count;
    }

    public List<Vehicle> GetBeginnerVehicles()
    {
        List<Vehicle> allowed = new List<Vehicle>();
        foreach (var vehicle in _vehicles)
        {
            if (_beginnerPolicy.CheckBeginnerPolicy(vehicle))
            {
                allowed.Add(vehicle);
            }
        }
        return allowed;
    }

    public List<IInventoryObject> GetAllInventory()
    {
        List<IInventoryObject> allInventory = new List<IInventoryObject>();
        allInventory.AddRange(_vehicles);
        allInventory.AddRange(_things);
        return allInventory;
    }
}