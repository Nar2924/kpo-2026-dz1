using VishKat.Domain.Interfaces;

namespace VishKat.Domain.Implementations;

public abstract class Thing : IInventoryObject 
{
    public string Name { get; }
    public int Id { get; }


    protected Thing(string name, int id) 
    {
        Name = name;
        Id = id;
    }
}

public class Helmet : Thing
{
    public Helmet(string name, int id) : base(name, id) { }
}

public class PowerBank: Thing, IEnergyConsumer 
{
    public int Consumption { get; }

    public PowerBank(string name, int id, int consumption) : base(name, id)
    {
        if (consumption >= 0)
        {
            Consumption = consumption;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(consumption),
                "Потребление энергии не может быть отрицательным.");
        }
    }
}

public class DocStation: Thing, IEnergyConsumer 
{
    public int Consumption { get; }

    public DocStation(string name, int id, int consumption) : base(name, id)
    {
        if (consumption >= 0)
        {
            Consumption = consumption;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(consumption),
                "Потребление энергии не может быть отрицательным.");
        }
    }
}