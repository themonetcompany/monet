namespace Monet.Application.ImportTransactions;

public record TransactionImportResult
{
    public required  int ImportedTransactions { get; init; }
    public required int IgnoredTransactions { get; init; }
}