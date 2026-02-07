using FluentAssertions;
using Monet.Application.ImportTransactions;
using Monet.Application.Tests.Shared;
using Monet.Domain;

namespace Monet.Application.Tests.ImportTransactions;

public class ImportTransactionTests
{
    private readonly FakeEventStore _eventStore = new();
    private readonly DeterministicGuidGenerator _guidGenerator = new();
    private readonly DeterministicAuthenticationGateway _authenticationGateway = new();
    private readonly DeterministicClock _clock = new();
    private TransactionImportHandler Handler => new(_eventStore, _authenticationGateway, _guidGenerator, _clock);

    [Fact]
    public async Task Given_ImportWithoutTransaction_When_Import_Then_ShouldReturnSummaryWithZeroTransactions()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        var import = new TransactionImport();
        var result = await Handler.HandleAsync(import, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(0);
    }

    [Fact]
    public async Task Given_ImportWithTransaction_When_Import_Then_ShouldReturnSummaryWithOneTransaction()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(1);
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION001",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportAlreadyImportedTransaction_When_Import_Then_ShouldIgnoreTransactionAndReturnSummaryWithOneIgnoredTransaction()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        _eventStore.PublishedEvents.Add(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION001",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(0);
        result.Value.IgnoredTransactions.Should().Be(1);
    }

    [Fact]
    public async Task Given_ImportWithTwoTransactions_When_Import_Then_ShouldReturnSummaryWithTwoTransactions()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 14, 54, 12, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                },
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION002",
                    Amount = new decimal(54.5),
                    Currency = "EUR",
                    Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
                    Description = "My Second Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(2);
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION001",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 14, 54, 12, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION002",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(new decimal(54.5), "EUR"),
            Date = new DateTimeOffset(2026, 02, 5, 08, 40, 23, TimeSpan.Zero),
            Description = "My Second Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportTransactionWithNonExistantAccount_When_Import_Then_ShouldReturnSummaryWithOneTransaction()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("ACCOUNT_NOT_FOUND");
        _eventStore.PublishedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_NoConnectedUser_When_Import_Then_ShouldReturnNonAuthenticatedUserError()
    {
        _clock.SetDate(2026, 02, 06);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("NON_AUTHENTICATED_USER");
    }

    [Fact]
    public async Task Given_ImportWithNewAndAlreadyImportedTransaction_When_Import_Then_ShouldReturnSummaryWithOneImportedAndOneIgnored()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        _eventStore.PublishedEvents.Add(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION001",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                },
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION002",
                    Amount = new decimal(54.5),
                    Currency = "EUR",
                    Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
                    Description = "My Second Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(1);
        result.Value.IgnoredTransactions.Should().Be(1);
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION002",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(new decimal(54.5), "EUR"),
            Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
            Description = "My Second Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithSecondTransactionOnNonExistentAccount_When_Import_Then_ShouldFailAndFirstTransactionIsAlreadyPublished()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                },
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION002",
                    Amount = new decimal(54.5),
                    Currency = "EUR",
                    Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
                    Description = "My Second Transaction",
                    AccountNumber = "ACCOUNT002"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Code.Should().Be("ACCOUNT_NOT_FOUND");
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION001",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithTransactionsOnDifferentAccounts_When_Import_Then_ShouldImportAll()
    {
        _clock.SetDate(2026, 02, 06);
        _guidGenerator.SetNext(Guid.NewGuid());
        _authenticationGateway.ConnectedUser(JohnDoe);

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT001",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        _eventStore.PublishedEvents.Add(new AccountCreated
        {
            AggregateId = "Account-ACCOUNT002",
            Id = Guid.NewGuid(),
            Version = 1,
            AccountNumber = "ACCOUNT002",
            Name = "My Second Account",
            Timestamp = _clock.Now.AddDays(-1),
            PublisherId = Guid.NewGuid(),
        });

        var import = new TransactionImport
        {
            Transactions =
            [
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT001"
                },
                new TransactionImport.Transaction
                {
                    TransactionId  = "TRANSACTION002",
                    Amount = new decimal(54.5),
                    Currency = "EUR",
                    Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
                    Description = "My Second Transaction",
                    AccountNumber = "ACCOUNT002"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(2);
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION001",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-TRANSACTION002",
            Id = _guidGenerator.New(),
            Version = 1,
            Amount = new Amount(new decimal(54.5), "EUR"),
            Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
            Description = "My Second Transaction",
            AccountNumber = "ACCOUNT002",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    private ConnectedUser JohnDoe { get; } = new()
    {
        Id = Guid.NewGuid(),
        Email = "john.doe@acme.com"
    };
}
