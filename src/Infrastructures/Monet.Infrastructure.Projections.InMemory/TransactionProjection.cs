using Monet.Application.Ports;
using Monet.Application.Transactions;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Infrastructure.Projections.InMemory;

public class TransactionProjection : IProjection, IProvideTransactions
{
    private readonly List<TransactionReadModel> _transactions = [];

    public Task ApplyAsync(IDomainEvent domainEvent)
    {
        if (domainEvent is TransactionImported imported)
        {
            _transactions.Add(new TransactionReadModel
            {
                TransactionId = imported.AggregateId,
                Amount = imported.Amount,
                Date = imported.Date,
                Description = imported.Description,
                AccountNumber = imported.AccountNumber,
            });
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TransactionReadModel>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<TransactionReadModel>>(_transactions.AsReadOnly());
    }
}
