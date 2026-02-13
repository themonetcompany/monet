namespace Monet.Application.Transactions;

public record CategoryReadModel
{
    public required string CategoryId { get; init; }
    public required string Name { get; init; }
    public required string FlowType { get; init; }
}
