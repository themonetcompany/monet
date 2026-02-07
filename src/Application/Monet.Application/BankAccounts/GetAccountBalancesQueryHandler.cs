using Monet.Application.Ports;
using Monet.Domain.Shared;

namespace Monet.Application.BankAccounts;

public class GetAccountBalancesQueryHandler(IProvideAccountBalances accountBalanceProvider, IAuthenticationGateway authenticationGateway)
{
    public async Task<IResult<IReadOnlyList<AccountBalanceReadModel>>> HandleAsync(CancellationToken cancellationToken)
    {
        return await authenticationGateway.GetConnectedUser()
            .MatchAsync(
                async _ => Result<IReadOnlyList<AccountBalanceReadModel>>.Success(
                    await accountBalanceProvider.GetAllAsync()),
                () => Result<IReadOnlyList<AccountBalanceReadModel>>.Failure("NON_AUTHENTICATED_USER"));
    }
}
