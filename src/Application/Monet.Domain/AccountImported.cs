using Monet.Domain.Shared;

namespace Monet.Domain;

public record AccountImported : DomainEvent
{
    public required string AccountNumber { get; init; }
    public required string Name { get; init; }
}