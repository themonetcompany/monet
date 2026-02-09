using Monet.Application.Ports;
using Monet.Domain.Shared;

namespace Monet.WebApp.Infrastructure;

public class InMemoryEventStore : IStoreEvent
{
    private readonly List<IDomainEvent> _events = [];

    public Task PublishAsync(IDomainEvent domainEvent)
    {
        _events.Add(domainEvent);
        return Task.CompletedTask;
    }

    public Task<bool> HasAsync(string aggregateId, CancellationToken cancellationToken)
    {
        return Task.FromResult(_events.Any(domainEvent => domainEvent.AggregateId == aggregateId));
    }
}
