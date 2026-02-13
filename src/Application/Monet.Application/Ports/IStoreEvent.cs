using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Ports;


public interface IStoreEvent
{
    Task PublishAsync(IDomainEvent domainEvent);
    Task<bool> HasAsync(string aggregateId, CancellationToken cancellationToken);
    Task<int> GetCurrentVersionAsync(string aggregateId, CancellationToken cancellationToken);
}
