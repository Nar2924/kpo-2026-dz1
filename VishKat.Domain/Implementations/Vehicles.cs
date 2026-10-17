using VishKat.Domain.Interfaces;

namespace VishKat.Domain.Implementations;

public abstract class Vehicle : IInventoryObject
{
    public string Name { get; }
    public int Id { get; }
    public int Ease { get; }

    protected Vehicle(string name, int id, int  ease)
    {
        Name = name;
        Id = id;
        if (ease >= 1 && ease <= 10) 
        {
            Ease = ease;
        }
        else 
        {
            throw new ArgumentOutOfRangeException(nameof(ease), "Легкость должна быть в пределах от 1 до 10");
        }
    }
}

public class ElectricBike : Vehicle, IEnergyConsumer
{
    public int Consumption { get; }

    public ElectricBike(string name, int id, int ease, int consumption) : base(name, id, ease)
    {
        if (consumption >= 0) 
        {
            Consumption = consumption;
        } 
        else 
        {
            throw new ArgumentOutOfRangeException(nameof(consumption),
                "Потребление энергии не может быть отрицательным");
        }
    }
}

public class Bike : Vehicle
{
    public Bike(string name, int id, int ease) : base(name, id, ease) { }
}

public class ElectricalScooter : Vehicle, IEnergyConsumer
{
    public int Consumption { get; }
    public ElectricalScooter(string name, int id, int ease, int consumption) : base(name, id, ease) 
    {
        if (consumption >= 0) 
        {
            Consumption = consumption;
        } 
        else 
        {
            throw new ArgumentOutOfRangeException(nameof(consumption),
                "Потребление энергии не может быть отрицательным");
        }
    }
}