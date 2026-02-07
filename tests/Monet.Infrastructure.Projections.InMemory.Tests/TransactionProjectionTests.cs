using FluentAssertions;
using Monet.Domain;

namespace Monet.Infrastructure.Projections.InMemory.Tests;

public class TransactionProjectionTests
{
    private readonly TransactionProjection _projection = new();

    [Fact]
    public async Task Given_NoEvents_When_GetAll_Then_ShouldReturnEmpty()
    {
        var result = await _projection.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_TransactionImported_When_GetAll_Then_ShouldReturnTransaction()
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
        result[0].TransactionId.Should().Be("Transaction-TX001");
        result[0].Amount.Should().Be(new Amount(100, "EUR"));
        result[0].Date.Should().Be(new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero));
        result[0].Description.Should().Be("Grocery shopping");
        result[0].AccountNumber.Should().Be("ACC001");
    }

    [Fact]
    public async Task Given_MultipleTransactionsImported_When_GetAll_Then_ShouldReturnAll()
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

        result.Should().HaveCount(2);
        result[0].TransactionId.Should().Be("Transaction-TX001");
        result[1].TransactionId.Should().Be("Transaction-TX002");
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
