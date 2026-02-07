using Monet.Application.BankAccounts;

namespace Monet.Application.Ports;

public interface IProvideAccountBalances
{
    Task<IReadOnlyList<AccountBalanceReadModel>> GetAllAsync();
}
