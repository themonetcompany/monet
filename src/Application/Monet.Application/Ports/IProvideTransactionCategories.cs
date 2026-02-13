using Monet.Application.Transactions;

namespace Monet.Application.Ports;

public interface IProvideTransactionCategories
{
    Task<IReadOnlyList<CategoryReadModel>> GetAllAsync();
    Task<CategoryReadModel?> GetByIdAsync(string categoryId);
}
