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
            FlowType = TransactionFlowType.Income,
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
        result[0].FlowType.Should().Be("Income");
        result[0].CategoryId.Should().BeNull();
        result[0].CategoryName.Should().BeNull();
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
            FlowType = TransactionFlowType.Income,
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
            FlowType = TransactionFlowType.Income,
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

    [Fact]
    public async Task Given_CategoryAssigned_When_GetById_Then_ShouldReturnTransactionWithCategory()
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
            FlowType = TransactionFlowType.Expense,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await _projection.ApplyAsync(new TransactionCategoryAssigned
        {
            AggregateId = "Transaction-TX001",
            TransactionAggregateId = "Transaction-TX001",
            CategoryId = "Category-Expense-Alimentation",
            CategoryName = "Alimentation",
            Id = Guid.NewGuid(),
            Version = 2,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetByIdAsync("Transaction-TX001");

        result.Should().NotBeNull();
        result!.CategoryId.Should().Be("Category-Expense-Alimentation");
        result.CategoryName.Should().Be("Alimentation");
    }

    [Fact]
    public async Task Given_CategoryCleared_When_GetById_Then_ShouldReturnTransactionWithoutCategory()
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
            FlowType = TransactionFlowType.Expense,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await _projection.ApplyAsync(new TransactionCategoryAssigned
        {
            AggregateId = "Transaction-TX001",
            TransactionAggregateId = "Transaction-TX001",
            CategoryId = "Category-Expense-Alimentation",
            CategoryName = "Alimentation",
            Id = Guid.NewGuid(),
            Version = 2,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await _projection.ApplyAsync(new TransactionCategoryAssigned
        {
            AggregateId = "Transaction-TX001",
            TransactionAggregateId = "Transaction-TX001",
            CategoryId = null,
            CategoryName = null,
            Id = Guid.NewGuid(),
            Version = 3,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetByIdAsync("Transaction-TX001");

        result.Should().NotBeNull();
        result!.CategoryId.Should().BeNull();
        result.CategoryName.Should().BeNull();
    }
}
