using Monet.Domain.Shared;

namespace Monet.Application.Ports;

public interface IProjection
{
    Task ApplyAsync(IDomainEvent domainEvent);
}
