using Monet.Application.BankAccounts;
using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Infrastructure.Projections.InMemory;

public class AccountBalanceProjection : IProjection, IProvideAccountBalances
{
    private const string AccountAggregateIdPrefix = "Account-";
    private readonly Dictionary<string, AccountState> _accountStates = new(StringComparer.OrdinalIgnoreCase);

    public Task ApplyAsync(IDomainEvent domainEvent)
    {
        if (domainEvent is TransactionImported imported)
        {
            var state = GetOrCreateState(imported.AccountNumber);
            state.Transactions.Add(new ImportedTransaction(imported.Date, imported.Amount.Value, imported.Amount.Currency));
        }

        if (domainEvent is DeclaredBalance declaredBalance)
        {
            var accountNumber = ExtractAccountNumber(declaredBalance.AggregateId);
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return Task.CompletedTask;
            }

            var state = GetOrCreateState(accountNumber);
            if (state.LatestDeclaredBalance is null || declaredBalance.Date >= state.LatestDeclaredBalance.Date)
            {
                state.LatestDeclaredBalance = new DeclaredBalanceSnapshot(
                    declaredBalance.Date,
                    declaredBalance.Balance.Value,
                    declaredBalance.Balance.Currency
                );
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AccountBalanceReadModel>> GetAllAsync()
    {
        var result = _accountStates
            .Select(entry => CreateReadModel(entry.Key, entry.Value))
            .Where(model => model is not null)
            .Select(model => model!)
            .OrderBy(model => model.AccountNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Task.FromResult<IReadOnlyList<AccountBalanceReadModel>>(result.AsReadOnly());
    }

    private AccountState GetOrCreateState(string accountNumber)
    {
        if (_accountStates.TryGetValue(accountNumber, out var state))
        {
            return state;
        }

        var createdState = new AccountState();
        _accountStates[accountNumber] = createdState;
        return createdState;
    }

    private static AccountBalanceReadModel? CreateReadModel(string accountNumber, AccountState state)
    {
        if (state.LatestDeclaredBalance is { } declaredBalance)
        {
            var transactionsAfterDeclaredBalance = state.Transactions
                .Where(transaction => transaction.Date > declaredBalance.Date)
                .Sum(transaction => transaction.Amount);

            return new AccountBalanceReadModel
            {
                AccountNumber = accountNumber,
                Balance = declaredBalance.Amount + transactionsAfterDeclaredBalance,
                Currency = declaredBalance.Currency
            };
        }

        if (state.Transactions.Count == 0)
        {
            return null;
        }

        var firstCurrency = state.Transactions[0].Currency;
        var balance = state.Transactions.Sum(transaction => transaction.Amount);

        return new AccountBalanceReadModel
        {
            AccountNumber = accountNumber,
            Balance = balance,
            Currency = firstCurrency
        };
    }

    private static string? ExtractAccountNumber(string aggregateId)
    {
        if (!aggregateId.StartsWith(AccountAggregateIdPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var accountNumber = aggregateId[AccountAggregateIdPrefix.Length..];
        return string.IsNullOrWhiteSpace(accountNumber) ? null : accountNumber;
    }

    private sealed class AccountState
    {
        public List<ImportedTransaction> Transactions { get; } = [];
        public DeclaredBalanceSnapshot? LatestDeclaredBalance { get; set; }
    }

    private sealed record ImportedTransaction(DateTimeOffset Date, decimal Amount, string Currency);

    private sealed record DeclaredBalanceSnapshot(DateTimeOffset Date, decimal Amount, string Currency);
}
