using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Monet.Application.Ports;
using Monet.Domain;

namespace Monet.WebApp.Tests.Transactions;

public class TransactionCategoryEndpointsTests
{
    [Fact]
    public async Task Given_ApplicationStarted_When_GetCategories_Then_ShouldReturnDefaultCatalog()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/transactions/categories", cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>(cancellationToken);
        categories.Should().NotBeNull();
        categories.Should().HaveCount(15);
        categories.Should().Contain(category => category.CategoryId == "Category-Expense-Alimentation");
        categories.Should().Contain(category => category.CategoryId == "Category-Income-Salaire");
    }

    [Fact]
    public async Task Given_UnknownTransaction_When_AssignCategory_Then_ShouldReturnNotFound()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            "/api/transactions/Transaction-Unknown/category",
            new AssignCategoryRequest("Category-Expense-Alimentation"),
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await ReadJsonAsync(response, cancellationToken);
        body.RootElement.GetProperty("code").GetString().Should().Be("TRANSACTION_NOT_FOUND");
    }

    [Fact]
    public async Task Given_NeutralTransaction_When_AssignCategory_Then_ShouldReturnBadRequest()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new WebApplicationFactory<Program>();
        await SeedTransactionAsync(factory, "Transaction-ACC001-NEUTRAL", TransactionFlowType.Neutral, amount: 0, cancellationToken);
        using var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            "/api/transactions/Transaction-ACC001-NEUTRAL/category",
            new AssignCategoryRequest("Category-Expense-Alimentation"),
            cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await ReadJsonAsync(response, cancellationToken);
        body.RootElement.GetProperty("code").GetString().Should().Be("CATEGORY_FORBIDDEN_FOR_NEUTRAL_FLOW");
    }

    [Fact]
    public async Task Given_ExpenseTransaction_When_AssignCategory_Then_ShouldPersistCategoryOnTransaction()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new WebApplicationFactory<Program>();
        await SeedTransactionAsync(factory, "Transaction-ACC001-EXPENSE", TransactionFlowType.Expense, amount: -55, cancellationToken);
        using var client = factory.CreateClient();

        var updateResponse = await client.PutAsJsonAsync(
            "/api/transactions/Transaction-ACC001-EXPENSE/category",
            new AssignCategoryRequest("Category-Expense-Alimentation"),
            cancellationToken);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var transactionsResponse = await client.GetAsync("/api/transactions", cancellationToken);
        transactionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var transactions = await transactionsResponse.Content.ReadFromJsonAsync<List<TransactionResponse>>(cancellationToken);
        transactions.Should().NotBeNull();

        var updated = transactions!.Single(transaction => transaction.TransactionId == "Transaction-ACC001-EXPENSE");
        updated.CategoryId.Should().Be("Category-Expense-Alimentation");
        updated.CategoryName.Should().Be("Alimentation");
    }

    private static async Task SeedTransactionAsync(
        WebApplicationFactory<Program> factory,
        string transactionId,
        TransactionFlowType flowType,
        decimal amount,
        CancellationToken cancellationToken)
    {
        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IStoreEvent>();

        await store.PublishAsync(new AccountCreated
        {
            AggregateId = "Account-ACC001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACC001",
            Name = "Primary account",
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });

        await store.PublishAsync(new TransactionImported
        {
            AggregateId = transactionId,
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(amount, "EUR"),
            Date = new DateTimeOffset(2026, 02, 12, 0, 0, 0, TimeSpan.Zero),
            Description = "Seeded transaction",
            AccountNumber = "ACC001",
            FlowType = flowType,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        });
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonDocument.Parse(content);
    }

    private sealed record AssignCategoryRequest(string? CategoryId);

    private sealed record CategoryResponse
    {
        public required string CategoryId { get; init; }
        public required string Name { get; init; }
        public required string FlowType { get; init; }
    }

    private sealed record TransactionResponse
    {
        public required string TransactionId { get; init; }
        public string? CategoryId { get; init; }
        public string? CategoryName { get; init; }
    }
}
