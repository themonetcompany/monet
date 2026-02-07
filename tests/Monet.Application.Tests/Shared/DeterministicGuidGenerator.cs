using Monet.Application.Ports;

namespace Monet.Application.Tests.Shared;

public class DeterministicGuidGenerator : IGenerateGuid
{
    private Guid? _next;

    public Guid SetNext(Guid guid)
    {
        _next = guid;
        return guid;
    }

    public Guid New()
    {
        return _next ?? throw new InvalidOperationException("No next GUID set");
    }
}