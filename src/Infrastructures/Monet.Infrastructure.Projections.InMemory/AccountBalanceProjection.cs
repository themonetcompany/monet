using Monet.Application.BankAccounts;
using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Infrastructure.Projections.InMemory;

public class AccountBalanceProjection : IProjection, IProvideAccountBalances
{
    private readonly Dictionary<string, (decimal Balance, string Currency)> _balances = new();

    public Task ApplyAsync(IDomainEvent domainEvent)
    {
        if (domainEvent is TransactionImported imported)
        {
            var key = imported.AccountNumber;
            if (_balances.TryGetValue(key, out var current))
            {
                _balances[key] = (current.Balance + imported.Amount.Value, current.Currency);
            }
            else
            {
                _balances[key] = (imported.Amount.Value, imported.Amount.Currency);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AccountBalanceReadModel>> GetAllAsync()
    {
        var result = _balances.Select(kvp => new AccountBalanceReadModel
        {
            AccountNumber = kvp.Key,
            Balance = kvp.Value.Balance,
            Currency = kvp.Value.Currency,
        }).ToList();

        return Task.FromResult<IReadOnlyList<AccountBalanceReadModel>>(result.AsReadOnly());
    }
}
