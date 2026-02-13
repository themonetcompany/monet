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
        var transactionId = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId);
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithAccountAndTransaction_When_Import_Then_ShouldReturnSummaryWithOneTransactionAndOneAccount()
    {
        _clock.SetDate(2026, 02, 06);
        var expectedAccountId = Guid.NewGuid();
        var expectedBalanceId = Guid.NewGuid();
        var expectedTransactionId = Guid.NewGuid();
        _guidGenerator.SetNext(expectedAccountId, expectedBalanceId, expectedTransactionId);
        _authenticationGateway.ConnectedUser(JohnDoe);

        var import = new TransactionImport
        {
            Accounts =
            [
                new TransactionImport.Account
                {
                    AccountNumber = "ACCOUNT001",
                    Name = "My Account",
                    Balances = [
                        new TransactionImport.Account.Balance
                        {
                            Date = new DateTimeOffset(2023, 02, 05, 00, 00, 00, TimeSpan.Zero),
                            Amount = 150,
                            Currency = "EUR",
                        }
                    ]
                }
            ],
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
        result.Value.ImportedAccounts.Should().Be(1);
        result.Value.ImportedTransactions.Should().Be(1);
        _eventStore.ShouldContain(new AccountImported
        {
            AggregateId = "Account-ACCOUNT001",
            Id = expectedAccountId,
            Version = 1,
            AccountNumber = "ACCOUNT001",
            Name = "My Account",
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new DeclaredBalance
        {
            AggregateId = "Account-ACCOUNT001",
            Id = expectedBalanceId,
            Version = 2,
            Date = new DateTimeOffset(2023, 02, 05, 00, 00, 00, TimeSpan.Zero),
            Balance = new Amount(150, "EUR"),
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = expectedTransactionId,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
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
        var transactionId1 = Guid.NewGuid();
        var transactionId2 = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId1, transactionId2);
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId1,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 14, 54, 12, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION002",
            Id = transactionId2,
            Version = 1,
            Amount = new Amount(new decimal(54.5), "EUR"),
            Date = new DateTimeOffset(2026, 02, 5, 08, 40, 23, TimeSpan.Zero),
            Description = "My Second Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
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
        var transactionId = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId);
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = Guid.NewGuid(),
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION002",
            Id = transactionId,
            Version = 1,
            Amount = new Amount(new decimal(54.5), "EUR"),
            Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
            Description = "My Second Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithSecondTransactionOnNonExistentAccount_When_Import_Then_ShouldFailAndFirstTransactionIsAlreadyPublished()
    {
        _clock.SetDate(2026, 02, 06);
        var transactionId = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId);
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithTransactionsOnDifferentAccounts_When_Import_Then_ShouldImportAll()
    {
        _clock.SetDate(2026, 02, 06);
        var transactionId1 = Guid.NewGuid();
        var transactionId2 = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId1, transactionId2);
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
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId1,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT002-TRANSACTION002",
            Id = transactionId2,
            Version = 1,
            Amount = new Amount(new decimal(54.5), "EUR"),
            Date = new DateTimeOffset(2026, 02, 05, 08, 40, 23, TimeSpan.Zero),
            Description = "My Second Transaction",
            AccountNumber = "ACCOUNT002",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithSameTransactionForTwoDifferentAccounts_When_Import_Then_ShouldImportAll()
    {
        _clock.SetDate(2026, 02, 06);
        var transactionId1 = Guid.NewGuid();
        var transactionId2 = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId1, transactionId2);
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
                    TransactionId  = "TRANSACTION001",
                    Amount = 100,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My First Transaction",
                    AccountNumber = "ACCOUNT002"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ImportedTransactions.Should().Be(2);
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId1,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT002-TRANSACTION001",
            Id = transactionId2,
            Version = 1,
            Amount = new Amount(100, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My First Transaction",
            AccountNumber = "ACCOUNT002",
            FlowType = TransactionFlowType.Income,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithNegativeAmount_When_Import_Then_ShouldSetExpenseFlowType()
    {
        _clock.SetDate(2026, 02, 06);
        var transactionId = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId);
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
                    Amount = -42,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My Expense Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId,
            Version = 1,
            Amount = new Amount(-42, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My Expense Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Expense,
            Timestamp = _clock.Now,
            PublisherId = JohnDoe.Id,
        });
    }

    [Fact]
    public async Task Given_ImportWithZeroAmount_When_Import_Then_ShouldSetNeutralFlowType()
    {
        _clock.SetDate(2026, 02, 06);
        var transactionId = Guid.NewGuid();
        _guidGenerator.SetNext(transactionId);
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
                    Amount = 0,
                    Currency = "EUR",
                    Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
                    Description = "My Neutral Transaction",
                    AccountNumber = "ACCOUNT001"
                }
            ]
        };

        var result = await Handler.HandleAsync(import, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _eventStore.ShouldContain(new TransactionImported
        {
            AggregateId = "Transaction-ACCOUNT001-TRANSACTION001",
            Id = transactionId,
            Version = 1,
            Amount = new Amount(0, "EUR"),
            Date = new DateTimeOffset(2023, 02, 04, 00, 00, 00, TimeSpan.Zero),
            Description = "My Neutral Transaction",
            AccountNumber = "ACCOUNT001",
            FlowType = TransactionFlowType.Neutral,
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
