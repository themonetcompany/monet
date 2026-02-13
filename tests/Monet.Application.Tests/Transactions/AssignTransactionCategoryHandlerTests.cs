using FluentAssertions;
using Monet.Application.Ports;
using Monet.Application.Tests.Shared;
using Monet.Application.Transactions;
using Monet.Domain;

namespace Monet.Application.Tests.Transactions;

public class AssignTransactionCategoryHandlerTests
{
    private readonly FakeEventStore _eventStore = new();
    private readonly StubTransactionProvider _transactionProvider = new();
    private readonly StubCategoryProvider _categoryProvider = new();
    private readonly DeterministicAuthenticationGateway _authenticationGateway = new();
    private readonly DeterministicGuidGenerator _guidGenerator = new();
    private readonly DeterministicClock _clock = new();

    private AssignTransactionCategoryHandler Handler =>
        new(_eventStore, _transactionProvider, _categoryProvider, _authenticationGateway, _guidGenerator, _clock);

    [Fact]
    public async Task Given_ValidExpenseCategory_When_Handle_Then_ShouldPublishAssignmentEvent()
    {
        _clock.SetDate(2026, 02, 12);
        var eventId = Guid.NewGuid();
        _guidGenerator.SetNext(eventId);
        _authenticationGateway.ConnectedUser(JohnDoe);

        _transactionProvider.Transactions.Add(new TransactionReadModel
        {
            TransactionId = "Transaction-ACC001-TX001",
            Amount = new Amount(-100, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Groceries",
            AccountNumber = "ACC001",
            FlowType = "Expense",
            CategoryId = null,
            CategoryName = null,
        });
        _categoryProvider.Categories.Add(new CategoryReadModel
        {
            CategoryId = "Category-Expense-Alimentation",
            Name = "Alimentation",
            FlowType = "Expense",
        });
        _eventStore.PublishedEvents.Add(ImportedTransaction("Transaction-ACC001-TX001", TransactionFlowType.Expense, version: 1));

        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", "Category-Expense-Alimentation", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _eventStore.ShouldContain(new TransactionCategoryAssigned
        {
            AggregateId = "Transaction-ACC001-TX001",
            TransactionAggregateId = "Transaction-ACC001-TX001",
            CategoryId = "Category-Expense-Alimentation",
            CategoryName = "Alimentation",
            Id = eventId,
            Version = 2,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_NeutralTransactionAndCategory_When_Handle_Then_ShouldFail()
    {
        _authenticationGateway.ConnectedUser(JohnDoe);
        _transactionProvider.Transactions.Add(new TransactionReadModel
        {
            TransactionId = "Transaction-ACC001-TX001",
            Amount = new Amount(0, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Neutral",
            AccountNumber = "ACC001",
            FlowType = "Neutral",
            CategoryId = null,
            CategoryName = null,
        });

        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", "Category-Expense-Alimentation", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("CATEGORY_FORBIDDEN_FOR_NEUTRAL_FLOW");
    }

    [Fact]
    public async Task Given_UnknownCategory_When_Handle_Then_ShouldFail()
    {
        _authenticationGateway.ConnectedUser(JohnDoe);
        _transactionProvider.Transactions.Add(new TransactionReadModel
        {
            TransactionId = "Transaction-ACC001-TX001",
            Amount = new Amount(-10, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Unknown category",
            AccountNumber = "ACC001",
            FlowType = "Expense",
            CategoryId = null,
            CategoryName = null,
        });

        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", "Category-Expense-Unknown", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("INVALID_TRANSACTION_CATEGORY");
    }

    [Fact]
    public async Task Given_MismatchedFlowType_When_Handle_Then_ShouldFail()
    {
        _authenticationGateway.ConnectedUser(JohnDoe);
        _transactionProvider.Transactions.Add(new TransactionReadModel
        {
            TransactionId = "Transaction-ACC001-TX001",
            Amount = new Amount(-10, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Mismatch",
            AccountNumber = "ACC001",
            FlowType = "Expense",
            CategoryId = null,
            CategoryName = null,
        });
        _categoryProvider.Categories.Add(new CategoryReadModel
        {
            CategoryId = "Category-Income-Salaire",
            Name = "Salaire",
            FlowType = "Income",
        });

        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", "Category-Income-Salaire", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("CATEGORY_NOT_ALLOWED_FOR_FLOW");
    }

    [Fact]
    public async Task Given_MissingTransaction_When_Handle_Then_ShouldFail()
    {
        _authenticationGateway.ConnectedUser(JohnDoe);

        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", "Category-Expense-Alimentation", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("TRANSACTION_NOT_FOUND");
    }

    [Fact]
    public async Task Given_NonAuthenticatedUser_When_Handle_Then_ShouldFail()
    {
        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", "Category-Expense-Alimentation", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("NON_AUTHENTICATED_USER");
    }

    [Fact]
    public async Task Given_NullCategoryId_When_Handle_Then_ShouldClearCategory()
    {
        _clock.SetDate(2026, 02, 12);
        var eventId = Guid.NewGuid();
        _guidGenerator.SetNext(eventId);
        _authenticationGateway.ConnectedUser(JohnDoe);
        _transactionProvider.Transactions.Add(new TransactionReadModel
        {
            TransactionId = "Transaction-ACC001-TX001",
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Salary",
            AccountNumber = "ACC001",
            FlowType = "Income",
            CategoryId = "Category-Income-Salaire",
            CategoryName = "Salaire",
        });

        var result = await Handler.HandleAsync("Transaction-ACC001-TX001", null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _eventStore.ShouldContain(new TransactionCategoryAssigned
        {
            AggregateId = "Transaction-ACC001-TX001",
            TransactionAggregateId = "Transaction-ACC001-TX001",
            CategoryId = null,
            CategoryName = null,
            Id = eventId,
            Version = 1,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    private ConnectedUser JohnDoe { get; } = new()
    {
        Id = Guid.NewGuid(),
        Email = "john.doe@acme.com",
    };

    private static TransactionImported ImportedTransaction(string transactionId, TransactionFlowType flowType, int version)
    {
        return new TransactionImported
        {
            AggregateId = transactionId,
            Id = Guid.NewGuid(),
            Version = version,
            Amount = new Amount(42, "EUR"),
            Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
            Description = "Imported",
            AccountNumber = "ACC001",
            FlowType = flowType,
            Timestamp = DateTimeOffset.UtcNow,
            PublisherId = Guid.NewGuid(),
        };
    }

    private class StubTransactionProvider : IProvideTransactions
    {
        public List<TransactionReadModel> Transactions { get; } = [];

        public Task<IReadOnlyList<TransactionReadModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<TransactionReadModel>>(Transactions.AsReadOnly());
        }

        public Task<TransactionReadModel?> GetByIdAsync(string transactionId)
        {
            return Task.FromResult(Transactions.FirstOrDefault(transaction => transaction.TransactionId == transactionId));
        }
    }

    private class StubCategoryProvider : IProvideTransactionCategories
    {
        public List<CategoryReadModel> Categories { get; } = [];

        public Task<IReadOnlyList<CategoryReadModel>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<CategoryReadModel>>(Categories.AsReadOnly());
        }

        public Task<CategoryReadModel?> GetByIdAsync(string categoryId)
        {
            return Task.FromResult(Categories.FirstOrDefault(category => category.CategoryId == categoryId));
        }
    }
}
