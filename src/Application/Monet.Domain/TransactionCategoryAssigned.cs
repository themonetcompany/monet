using Monet.Domain.Shared;

namespace Monet.Domain;

public record TransactionCategoryAssigned : DomainEvent
{
    public required string TransactionAggregateId { get; init; }
    public string? CategoryId { get; init; }
    public string? CategoryName { get; init; }
}
