using Monet.Application.Ports;
using Monet.Domain.Shared;

namespace Monet.Application.Transactions;

public class GetTransactionsQueryHandler(IProvideTransactions transactionProvider, IAuthenticationGateway authenticationGateway)
{
    public async Task<IResult<IReadOnlyList<TransactionReadModel>>> HandleAsync(CancellationToken cancellationToken)
    {
        return await authenticationGateway.GetConnectedUser()
            .MatchAsync(
                async _ => Result<IReadOnlyList<TransactionReadModel>>.Success(
                    await transactionProvider.GetAllAsync()),
                () => Result<IReadOnlyList<TransactionReadModel>>.Failure("NON_AUTHENTICATED_USER"));
    }
}
