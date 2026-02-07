using Monet.Domain.Shared;

namespace Monet.Domain;

public record AccountCreated : DomainEvent
{
    public required string AccountNumber { get; init; }
    public required string Name { get; init; }
}