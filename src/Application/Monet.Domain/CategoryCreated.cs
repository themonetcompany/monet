using Monet.Domain.Shared;

namespace Monet.Domain;

public record CategoryCreated : DomainEvent
{
    public required string Name { get; init; }
    public required TransactionFlowType FlowType { get; init; }
}
