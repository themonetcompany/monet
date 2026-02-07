namespace Monet.Application.BankAccounts;

public record AccountBalanceReadModel
{
    public required string AccountNumber { get; init; }
    public required decimal Balance { get; init; }
    public required string Currency { get; init; }
}
