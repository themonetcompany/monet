using Monet.Application.Ports;
using Monet.Domain.Shared;

namespace Monet.Application;

public class ProjectingEventStore(IStoreEvent innerStore, IProjection[] projections) : IStoreEvent
{
    public async Task PublishAsync(IDomainEvent domainEvent)
    {
        await innerStore.PublishAsync(domainEvent);

        foreach (var projection in projections)
        {
            await projection.ApplyAsync(domainEvent);
        }
    }

    public Task<bool> HasAsync(string aggregateId, CancellationToken cancellationToken)
    {
        return innerStore.HasAsync(aggregateId, cancellationToken);
    }

    public Task<int> GetCurrentVersionAsync(string aggregateId, CancellationToken cancellationToken)
    {
        return innerStore.GetCurrentVersionAsync(aggregateId, cancellationToken);
    }
}
