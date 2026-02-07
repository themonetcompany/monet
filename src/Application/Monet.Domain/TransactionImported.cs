using Monet.Domain.Shared;

namespace Monet.Domain;

public record TransactionImported : DomainEvent
{
    public required Amount Amount { get; init; }
    public required DateTimeOffset Date { get; init; }
    public required string Description { get; init; }
    public required string AccountNumber { get; set; }
}