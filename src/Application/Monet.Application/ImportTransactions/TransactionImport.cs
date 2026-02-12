namespace Monet.Application.ImportTransactions;

public record TransactionImport
{
    public Transaction[] Transactions { get; init; } = [];
    public Account[] Accounts { get; init; } = [];

    public record Transaction
    {
        public required string TransactionId { get; init; }
        public required decimal Amount { get; init; }
        public required DateTimeOffset Date { get; init; }
        public required string Description { get; init; }
        public required string AccountNumber { get; init; }
        public required string Currency { get; init; }
    }

    public record Account
    {
        public required string AccountNumber { get; init; }
        public required string Name { get; init; }
        public required Balance[] Balances { get; init; } = [];

        public record Balance
        {
            public required DateTimeOffset Date { get; init; }
            public required decimal Amount { get; init; }
            public required string Currency { get; init; }
        }
    }
}