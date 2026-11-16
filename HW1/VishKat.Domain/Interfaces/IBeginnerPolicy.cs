using VishKat.Domain.Implementations;

namespace VishKat.Domain.Interfaces;

public interface IBeginnerPolicy
{
    bool CheckBeginnerPolicy(Vehicle obj);
}