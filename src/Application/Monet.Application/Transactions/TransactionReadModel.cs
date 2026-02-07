using Monet.Domain;

namespace Monet.Application.Transactions;

public record TransactionReadModel
{
    public required string TransactionId { get; init; }
    public required Amount Amount { get; init; }
    public required DateTimeOffset Date { get; init; }
    public required string Description { get; init; }
    public required string AccountNumber { get; init; }
}
