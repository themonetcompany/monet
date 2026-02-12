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

    [Fact]
    public async Task Given_DeclaredBalanceWithoutTransaction_When_GetAll_Then_ShouldReturnDeclaredBalance()
    {
        await _projection.ApplyAsync(DeclaredBalance("ACC001", 500m, "EUR", day: 10));

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
        result[0].AccountNumber.Should().Be("ACC001");
        result[0].Balance.Should().Be(500m);
        result[0].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Given_DeclaredBalanceAndTransactionAfterCutoff_When_GetAll_Then_ShouldAddTransactionToDeclaredBalance()
    {
        await _projection.ApplyAsync(DeclaredBalance("ACC001", 500m, "EUR", day: 10));
        await _projection.ApplyAsync(Transaction("ACC001", 25m, "EUR", day: 11, id: "TX-001"));

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
        result[0].Balance.Should().Be(525m);
        result[0].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Given_DeclaredBalanceAndTransactionsBeforeOrAtCutoff_When_GetAll_Then_ShouldIgnoreThoseTransactions()
    {
        await _projection.ApplyAsync(Transaction("ACC001", 30m, "EUR", day: 9, id: "TX-001"));
        await _projection.ApplyAsync(Transaction("ACC001", 40m, "EUR", day: 10, id: "TX-002"));
        await _projection.ApplyAsync(DeclaredBalance("ACC001", 500m, "EUR", day: 10));

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
        result[0].Balance.Should().Be(500m);
    }

    [Fact]
    public async Task Given_TransactionsAndThenMoreRecentDeclaredBalance_When_GetAll_Then_ShouldRecalculateFromDeclaredBalance()
    {
        await _projection.ApplyAsync(Transaction("ACC001", 100m, "EUR", day: 8, id: "TX-001"));
        await _projection.ApplyAsync(Transaction("ACC001", 75m, "EUR", day: 12, id: "TX-002"));
        await _projection.ApplyAsync(DeclaredBalance("ACC001", 1_000m, "EUR", day: 10));

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
        result[0].Balance.Should().Be(1_075m);
        result[0].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Given_MultipleDeclaredBalances_When_GetAll_Then_MostRecentDateShouldWin()
    {
        await _projection.ApplyAsync(DeclaredBalance("ACC001", 300m, "EUR", day: 8));
        await _projection.ApplyAsync(DeclaredBalance("ACC001", 700m, "EUR", day: 12));
        await _projection.ApplyAsync(Transaction("ACC001", 25m, "EUR", day: 13, id: "TX-001"));

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
        result[0].Balance.Should().Be(725m);
        result[0].Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Given_SameEventsInDifferentOrder_When_GetAll_Then_ShouldReturnSameBalance()
    {
        var projectionA = new AccountBalanceProjection();
        var projectionB = new AccountBalanceProjection();

        var declaredOld = DeclaredBalance("ACC001", 100m, "EUR", day: 5);
        var declaredRecent = DeclaredBalance("ACC001", 200m, "EUR", day: 10);
        var transactionBeforeCutoff = Transaction("ACC001", 10m, "EUR", day: 9, id: "TX-001");
        var transactionAfterCutoff = Transaction("ACC001", 50m, "EUR", day: 12, id: "TX-002");

        await projectionA.ApplyAsync(declaredOld);
        await projectionA.ApplyAsync(transactionBeforeCutoff);
        await projectionA.ApplyAsync(declaredRecent);
        await projectionA.ApplyAsync(transactionAfterCutoff);

        await projectionB.ApplyAsync(transactionAfterCutoff);
        await projectionB.ApplyAsync(declaredRecent);
        await projectionB.ApplyAsync(transactionBeforeCutoff);
        await projectionB.ApplyAsync(declaredOld);

        var resultA = await projectionA.GetAllAsync();
        var resultB = await projectionB.GetAllAsync();

        resultA.Should().ContainSingle();
        resultB.Should().ContainSingle();
        resultA[0].Balance.Should().Be(250m);
        resultB[0].Balance.Should().Be(250m);
        resultA[0].Currency.Should().Be("EUR");
        resultB[0].Currency.Should().Be("EUR");
    }

    private static TransactionImported Transaction(
        string accountNumber,
        decimal amount,
        string currency,
        int day,
        string id)
    {
        return new TransactionImported
        {
            AggregateId = $"Transaction-{id}",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(amount, currency),
            Date = new DateTimeOffset(2026, 02, day, 0, 0, 0, TimeSpan.Zero),
            Description = $"Transaction {id}",
            AccountNumber = accountNumber,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid()
        };
    }

    private static DeclaredBalance DeclaredBalance(
        string accountNumber,
        decimal amount,
        string currency,
        int day)
    {
        return new DeclaredBalance
        {
            AggregateId = $"Account-{accountNumber}",
            Id = Guid.NewGuid(),
            Version = 1,
            Balance = new Amount(amount, currency),
            Date = new DateTimeOffset(2026, 02, day, 0, 0, 0, TimeSpan.Zero),
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid()
        };
    }
}
