using VishKat.Domain.Implementations;
using VishKat.Domain.Interfaces;

namespace VishKat.Domain.Services;

public class ServiceCenter : IServiceCenter
{
    public bool CheckVehicle(Vehicle vehicle)
    {
        if (vehicle is IEnergyConsumer consumer && consumer.Consumption > 100)
        {
            return false;
        }

        if (vehicle.Name == "Самокат Самоубийца")
        {
            return false;
        }
        return true;
    }
    
}