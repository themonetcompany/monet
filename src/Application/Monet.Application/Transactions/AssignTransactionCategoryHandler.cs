using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Transactions;

public class AssignTransactionCategoryHandler(
    IStoreEvent eventStore,
    IProvideTransactions transactionProvider,
    IProvideTransactionCategories categoryProvider,
    IAuthenticationGateway authenticationGateway,
    IGenerateGuid guidGenerator,
    IClock clock)
{
    public async Task<IResult<bool>> HandleAsync(string transactionId, string? categoryId, CancellationToken cancellationToken)
    {
        return await authenticationGateway.GetConnectedUser()
            .MatchAsync(async connectedUser =>
            {
                var transaction = await transactionProvider.GetByIdAsync(transactionId);
                if (transaction is null)
                {
                    return Result<bool>.Failure("TRANSACTION_NOT_FOUND");
                }

                if (string.IsNullOrWhiteSpace(categoryId))
                {
                    return await PublishCategoryAssignmentAsync(
                        transactionId,
                        categoryId: null,
                        categoryName: null,
                        connectedUser.Id,
                        cancellationToken);
                }

                if (IsFlowType(transaction.FlowType, TransactionFlowType.Neutral))
                {
                    return Result<bool>.Failure("CATEGORY_FORBIDDEN_FOR_NEUTRAL_FLOW");
                }

                var category = await categoryProvider.GetByIdAsync(categoryId);
                if (category is null)
                {
                    return Result<bool>.Failure("INVALID_TRANSACTION_CATEGORY");
                }

                if (!string.Equals(category.FlowType, transaction.FlowType, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<bool>.Failure("CATEGORY_NOT_ALLOWED_FOR_FLOW");
                }

                return await PublishCategoryAssignmentAsync(
                    transactionId,
                    categoryId: category.CategoryId,
                    categoryName: category.Name,
                    connectedUser.Id,
                    cancellationToken);
            }, () => Result<bool>.Failure("NON_AUTHENTICATED_USER"));
    }

    private async Task<IResult<bool>> PublishCategoryAssignmentAsync(
        string transactionId,
        string? categoryId,
        string? categoryName,
        Guid publisherId,
        CancellationToken cancellationToken)
    {
        var currentVersion = await eventStore.GetCurrentVersionAsync(transactionId, cancellationToken);

        await eventStore.PublishAsync(new TransactionCategoryAssigned
        {
            AggregateId = transactionId,
            TransactionAggregateId = transactionId,
            CategoryId = categoryId,
            CategoryName = categoryName,
            Id = guidGenerator.New(),
            Version = currentVersion + 1,
            Timestamp = clock.Now,
            PublisherId = publisherId,
        });

        return Result<bool>.Success(true);
    }

    private static bool IsFlowType(string flowType, TransactionFlowType expectedFlowType)
    {
        return string.Equals(flowType, expectedFlowType.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
