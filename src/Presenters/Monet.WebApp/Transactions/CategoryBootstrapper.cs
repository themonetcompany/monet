using Monet.Application.Ports;
using Monet.Domain;

namespace Monet.WebApp.Transactions;

public class CategoryBootstrapper(IStoreEvent eventStore, IClock clock, IGenerateGuid guidGenerator)
{
    public async Task BootstrapAsync(CancellationToken cancellationToken)
    {
        foreach (var category in BootstrapCategories)
        {
            if (await eventStore.HasAsync(category.AggregateId, cancellationToken))
            {
                continue;
            }

            var currentVersion = await eventStore.GetCurrentVersionAsync(category.AggregateId, cancellationToken);
            await eventStore.PublishAsync(new CategoryCreated
            {
                AggregateId = category.AggregateId,
                Id = guidGenerator.New(),
                Version = currentVersion + 1,
                Name = category.Name,
                FlowType = category.FlowType,
                Timestamp = clock.Now,
                PublisherId = Guid.Empty,
            });
        }
    }

    private static IReadOnlyList<BootstrapCategory> BootstrapCategories { get; } =
    [
        new("Category-Expense-Alimentation", "Alimentation", TransactionFlowType.Expense),
        new("Category-Expense-Logement", "Logement", TransactionFlowType.Expense),
        new("Category-Expense-Transport", "Transport", TransactionFlowType.Expense),
        new("Category-Expense-Sante", "Santé", TransactionFlowType.Expense),
        new("Category-Expense-Loisirs", "Loisirs", TransactionFlowType.Expense),
        new("Category-Expense-Factures", "Factures", TransactionFlowType.Expense),
        new("Category-Expense-Education", "Éducation", TransactionFlowType.Expense),
        new("Category-Expense-Impots", "Impôts", TransactionFlowType.Expense),
        new("Category-Expense-Autres", "Autres", TransactionFlowType.Expense),
        new("Category-Income-Salaire", "Salaire", TransactionFlowType.Income),
        new("Category-Income-Freelance", "Freelance", TransactionFlowType.Income),
        new("Category-Income-Remboursement", "Remboursement", TransactionFlowType.Income),
        new("Category-Income-Aides", "Aides", TransactionFlowType.Income),
        new("Category-Income-Placement", "Placement", TransactionFlowType.Income),
        new("Category-Income-Autres", "Autres", TransactionFlowType.Income),
    ];

    private sealed record BootstrapCategory(string AggregateId, string Name, TransactionFlowType FlowType);
}
