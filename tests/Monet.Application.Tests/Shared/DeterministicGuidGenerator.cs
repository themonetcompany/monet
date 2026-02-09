using Monet.Application.Ports;

namespace Monet.Application.Tests.Shared;

public class DeterministicGuidGenerator : IGenerateGuid
{
    private Queue<Guid> _guids = new();

    public void SetNext(params Guid[] guids)
    {
        foreach (var guid in guids)
        {
            _guids.Enqueue(guid);
        }
    }

    public Guid New()
    {
        return _guids.Dequeue();
    }
}