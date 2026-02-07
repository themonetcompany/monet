using FluentAssertions;
using Monet.Domain;

namespace Monet.Infrastructure.Projections.InMemory.Tests;

public class AccountBalanceProjectionTests
{
    private readonly AccountBalanceProjection _projection = new();

    [Fact]
    public async Task Given_NoEvents_When_GetAll_Then_ShouldReturnEmpty()
    {
        var result = await _projection.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_OneTransactionImported_When_GetAll_Then_ShouldReturnBalanceEqualToAmount()
    {
        await _projection.ApplyAsync(new TransactionImported
        {
            AggregateId = "Transaction-TX001",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Grocery shopping",
            AccountNumber = "ACC001",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().HaveCount(1);
        result[0].AccountNumber.Should().Be("ACC001");
        result[0].Balance.Should().Be(100);
        result[0].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Given_TwoTransactionsOnSameAccount_When_GetAll_Then_ShouldReturnSummedBalance()
    {
        await _projection.ApplyAsync(new TransactionImported
        {
            AggregateId = "Transaction-TX001",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "First",
            AccountNumber = "ACC001",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await _projection.ApplyAsync(new TransactionImported
        {
            AggregateId = "Transaction-TX002",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(50.5m, "EUR"),
            Date = new DateTimeOffset(2026, 02, 07, 0, 0, 0, TimeSpan.Zero),
            Description = "Second",
            AccountNumber = "ACC001",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().HaveCount(1);
        result[0].AccountNumber.Should().Be("ACC001");
        result[0].Balance.Should().Be(150.5m);
    }

    [Fact]
    public async Task Given_TransactionsOnDifferentAccounts_When_GetAll_Then_ShouldReturnSeparateBalances()
    {
        await _projection.ApplyAsync(new TransactionImported
        {
            AggregateId = "Transaction-TX001",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "First",
            AccountNumber = "ACC001",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await _projection.ApplyAsync(new TransactionImported
        {
            AggregateId = "Transaction-TX002",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(200, "EUR"),
            Date = new DateTimeOffset(2026, 02, 07, 0, 0, 0, TimeSpan.Zero),
            Description = "Second",
            AccountNumber = "ACC002",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(r => r.AccountNumber == "ACC001" && r.Balance == 100);
        result.Should().Contain(r => r.AccountNumber == "ACC002" && r.Balance == 200);
    }

    [Fact]
    public async Task Given_NonTransactionEvent_When_Apply_Then_ShouldBeIgnored()
    {
        await _projection.ApplyAsync(new AccountCreated
        {
            AggregateId = "Account-ACC001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACC001",
            Name = "My Account",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().BeEmpty();
    }
}
