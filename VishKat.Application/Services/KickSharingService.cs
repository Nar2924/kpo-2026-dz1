using VishKat.Application.Interfaces;
using VishKat.Domain.Interfaces;
using VishKat.Domain.Services;

namespace VishKat.Application.Services;

public class KickSharingService : IKickSharingService
{

    readonly IServiceCenter _service;
    readonly IBeginnerPolicy  _policy;
    public KickSharingService(IBeginnerPolicy  policy, IServiceCenter serviceCenter)
    {
       _policy = policy;
       _service = serviceCenter;
    }

    public KickSharing CreateKickSharing()
    {
        var res = new KickSharing(_service, _policy);

        return res;
    }
}