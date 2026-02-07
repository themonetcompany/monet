using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.ImportTransactions;

public class TransactionImportHandler(IStoreEvent eventStore, IAuthenticationGateway authenticationGateway, IGenerateGuid guidGenerator, IClock clock)
{
    public async Task<IResult<TransactionImportResult>> HandleAsync(TransactionImport import, CancellationToken cancellationToken)
    {
        return await authenticationGateway.GetConnectedUser()
            .MatchAsync(async connectedUser =>
            {
                var importedTransactionCount = 0;
                var ignoredTransactionCount = 0;

                foreach (var transaction in import.Transactions)
                {
                    if (!await eventStore.HasAsync($"Account-{transaction.AccountNumber}", cancellationToken))
                        return Result<TransactionImportResult>.Failure("ACCOUNT_NOT_FOUND");

                    var transactionAggregateId = $"Transaction-{transaction.TransactionId}";
                    if (await eventStore.HasAsync(transactionAggregateId, cancellationToken))
                    {
                        ignoredTransactionCount++;
                        continue;
                    }

                    await eventStore.PublishAsync(new TransactionImported
                    {
                        AggregateId = transactionAggregateId,
                        Id = guidGenerator.New(),
                        Version = 1,
                        Amount = new Amount(transaction.Amount, transaction.Currency),
                        Date = transaction.Date,
                        Description = transaction.Description,
                        AccountNumber = transaction.AccountNumber,
                        Timestamp = clock.Now,
                        PublisherId = connectedUser.Id,
                    });
                    importedTransactionCount++;
                }

                return Result<TransactionImportResult>.Success(new TransactionImportResult
                {
                    ImportedTransactions = importedTransactionCount,
                    IgnoredTransactions = ignoredTransactionCount,
                });
            },  () => Result<TransactionImportResult>.Failure("NON_AUTHENTICATED_USER"));


    }
}





