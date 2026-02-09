using Monet.Application.Ports;

namespace Monet.WebApp.Infrastructure;

public class GuidGenerator : IGenerateGuid
{
    public Guid New()
    {
        return Guid.NewGuid();
    }
}
