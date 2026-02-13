using Monet.Application.Ports;
using Monet.Domain.Shared;

namespace Monet.Application.Transactions;

public class GetTransactionCategoriesQueryHandler(
    IProvideTransactionCategories categoryProvider,
    IAuthenticationGateway authenticationGateway)
{
    public async Task<IResult<IReadOnlyList<CategoryReadModel>>> HandleAsync(CancellationToken cancellationToken)
    {
        return await authenticationGateway.GetConnectedUser()
            .MatchAsync(
                async _ => Result<IReadOnlyList<CategoryReadModel>>.Success(
                    await categoryProvider.GetAllAsync()),
                () => Result<IReadOnlyList<CategoryReadModel>>.Failure("NON_AUTHENTICATED_USER"));
    }
}
