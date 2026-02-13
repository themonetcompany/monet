using FluentAssertions;
using Monet.Domain;

namespace Monet.Infrastructure.Projections.InMemory.Tests;

public class CategoryProjectionTests
{
    private readonly CategoryProjection _projection = new();

    [Fact]
    public async Task Given_NoEvents_When_GetAll_Then_ShouldReturnEmpty()
    {
        var result = await _projection.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_CategoryCreated_When_GetAll_Then_ShouldContainCategory()
    {
        await _projection.ApplyAsync(new CategoryCreated
        {
            AggregateId = "Category-Expense-Alimentation",
            Id = Guid.NewGuid(),
            Version = 1,
            Name = "Alimentation",
            FlowType = TransactionFlowType.Expense,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
        result[0].CategoryId.Should().Be("Category-Expense-Alimentation");
        result[0].Name.Should().Be("Alimentation");
        result[0].FlowType.Should().Be("Expense");
    }

    [Fact]
    public async Task Given_SameCategoryCreatedTwice_When_GetAll_Then_ShouldKeepSingleEntry()
    {
        await _projection.ApplyAsync(new CategoryCreated
        {
            AggregateId = "Category-Income-Salaire",
            Id = Guid.NewGuid(),
            Version = 1,
            Name = "Salaire",
            FlowType = TransactionFlowType.Income,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await _projection.ApplyAsync(new CategoryCreated
        {
            AggregateId = "Category-Income-Salaire",
            Id = Guid.NewGuid(),
            Version = 2,
            Name = "Salaire",
            FlowType = TransactionFlowType.Income,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().ContainSingle();
    }

    [Fact]
    public async Task Given_NonCategoryEvent_When_Apply_Then_ShouldIgnore()
    {
        await _projection.ApplyAsync(new AccountCreated
        {
            AggregateId = "Account-ACC001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACC001",
            Name = "Main",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        var result = await _projection.GetAllAsync();

        result.Should().BeEmpty();
    }
}
