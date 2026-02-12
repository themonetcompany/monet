using Monet.Domain.Shared;

namespace Monet.Domain;

public record DeclaredBalance : DomainEvent
{
    public required Amount Balance { get; init; }
    public required DateTimeOffset Date { get; init; }
}