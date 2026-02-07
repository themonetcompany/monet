using Monet.Application.Transactions;

namespace Monet.Application.Ports;

public interface IProvideTransactions
{
    Task<IReadOnlyList<TransactionReadModel>> GetAllAsync();
}
