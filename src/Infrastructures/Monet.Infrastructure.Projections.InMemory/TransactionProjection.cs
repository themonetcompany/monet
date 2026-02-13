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
                FlowType = imported.FlowType.ToString(),
                CategoryId = null,
                CategoryName = null,
            });
        }
        else if (domainEvent is TransactionCategoryAssigned categoryAssigned)
        {
            var transactionIndex = _transactions.FindIndex(transaction => transaction.TransactionId == categoryAssigned.TransactionAggregateId);
            if (transactionIndex >= 0)
            {
                _transactions[transactionIndex] = _transactions[transactionIndex] with
                {
                    CategoryId = categoryAssigned.CategoryId,
                    CategoryName = categoryAssigned.CategoryName,
                };
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<TransactionReadModel>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyList<TransactionReadModel>>(_transactions.AsReadOnly());
    }

    public Task<TransactionReadModel?> GetByIdAsync(string transactionId)
    {
        return Task.FromResult(_transactions.FirstOrDefault(transaction => transaction.TransactionId == transactionId));
    }
}
