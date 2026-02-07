using FluentAssertions;
using Monet.Application.BankAccounts;
using Monet.Application.Ports;
using Monet.Application.Tests.Shared;
using Monet.Domain;

namespace Monet.Application.Tests.BankAccounts;

public class GetAccountBalancesQueryHandlerTests
{
    private readonly DeterministicAuthenticationGateway _authenticationGateway = new();
    private readonly StubAccountBalanceProvider _accountBalanceProvider = new();

    private GetAccountBalancesQueryHandler Handler => new(_accountBalanceProvider, _authenticationGateway);

    [Fact]
    public async Task Given_AuthenticatedUser_When_Handle_Then_ShouldReturnBalances()
    {
        _authenticationGateway.ConnectedUser(JohnDoe);
        _accountBalanceProvider.Balances.Add(new AccountBalanceReadModel
        {
            AccountNumber = "ACC001",
            Balance = 150.5m,
            Currency = "EUR",
        });

        var result = await Handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].AccountNumber.Should().Be("ACC001");
        result.Value[0].Balance.Should().Be(150.5m);
        result.Value[0].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Given_NoAuthenticatedUser_When_Handle_Then_ShouldReturnFailure()
    {
        var result = await Handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("NON_AUTHENTICATED_USER");
    }

    private ConnectedUser JohnDoe { get; } = new()
    {
        Id = Guid.NewGuid(),
        Email = "john.doe@acme.com"
    };

    private class StubAccountBalanceProvider : IProvideAccountBalances
    {
        public List<AccountBalanceReadModel> Balances { get; } = [];

        public Task<IReadOnlyList<AccountBalanceReadModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<AccountBalanceReadModel>>(Balances.AsReadOnly());
        }
    }
}
