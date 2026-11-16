using VishKat.Domain.Implementations;
using VishKat.Domain.Interfaces;

namespace VishKat.Domain.Services;

public class BeginnerPolicy : IBeginnerPolicy
{
    public bool CheckBeginnerPolicy(Vehicle vehicle)
    {
        switch (vehicle)
        {
            case IEnergyConsumer consumer when consumer.Consumption > 50:
            case {Ease: < 6}:
                return false;
            default:
                return true;
        }
    }
}