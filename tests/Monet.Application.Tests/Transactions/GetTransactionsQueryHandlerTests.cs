using FluentAssertions;
using Monet.Application.Ports;
using Monet.Application.Tests.Shared;
using Monet.Application.Transactions;
using Monet.Domain;

namespace Monet.Application.Tests.Transactions;

public class GetTransactionsQueryHandlerTests
{
    private readonly DeterministicAuthenticationGateway _authenticationGateway = new();
    private readonly StubTransactionProvider _transactionProvider = new();

    private GetTransactionsQueryHandler Handler => new(_transactionProvider, _authenticationGateway);

    [Fact]
    public async Task Given_AuthenticatedUser_When_Handle_Then_ShouldReturnTransactions()
    {
        _authenticationGateway.ConnectedUser(JohnDoe);
        _transactionProvider.Transactions.Add(new TransactionReadModel
        {
            TransactionId = "Transaction-TX001",
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Grocery shopping",
            AccountNumber = "ACC001",
        });

        var result = await Handler.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].TransactionId.Should().Be("Transaction-TX001");
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

    private class StubTransactionProvider : IProvideTransactions
    {
        public List<TransactionReadModel> Transactions { get; } = [];

        public Task<IReadOnlyList<TransactionReadModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<TransactionReadModel>>(Transactions.AsReadOnly());
        }
    }
}
