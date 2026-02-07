using FluentAssertions;
using Monet.Application.Ports;
using Monet.Application.Tests.Shared;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Tests;

public class ProjectingEventStoreTests
{
    private readonly FakeEventStore _innerStore = new();

    [Fact]
    public async Task Given_Event_When_Publish_Then_ShouldStoreAndDispatchToProjections()
    {
        var spyProjection = new SpyProjection();
        var store = new ProjectingEventStore(_innerStore, [spyProjection]);

        var domainEvent = ATransactionImported();

        await store.PublishAsync(domainEvent);

        _innerStore.ShouldContain(domainEvent);
        spyProjection.AppliedEvents.Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public async Task Given_MultipleProjections_When_Publish_Then_ShouldDispatchToAll()
    {
        var projection1 = new SpyProjection();
        var projection2 = new SpyProjection();
        var store = new ProjectingEventStore(_innerStore, [projection1, projection2]);

        var domainEvent = ATransactionImported();

        await store.PublishAsync(domainEvent);

        projection1.AppliedEvents.Should().ContainSingle();
        projection2.AppliedEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task Given_NoProjections_When_Publish_Then_ShouldStoreWithoutError()
    {
        var store = new ProjectingEventStore(_innerStore, []);

        var domainEvent = ATransactionImported();

        await store.PublishAsync(domainEvent);

        _innerStore.ShouldContain(domainEvent);
    }

    [Fact]
    public async Task Given_AggregateId_When_Has_Then_ShouldDelegateToInnerStore()
    {
        var store = new ProjectingEventStore(_innerStore, [new SpyProjection()]);

        _innerStore.PublishedEvents.Add(ATransactionImported());

        var result = await store.HasAsync("Transaction-TX001", CancellationToken.None);

        result.Should().BeTrue();
    }

    private static TransactionImported ATransactionImported() => new()
    {
        AggregateId = "Transaction-TX001",
        Id = Guid.NewGuid(),
        Version = 1,
        Amount = new Amount(100, "EUR"),
        Date = new DateTimeOffset(2026, 02, 06, 0, 0, 0, TimeSpan.Zero),
        Description = "Test",
        AccountNumber = "ACC001",
        Timestamp = DateTimeOffset.UtcNow,
        PublisherId = Guid.NewGuid(),
    };

    private class SpyProjection : IProjection
    {
        public List<IDomainEvent> AppliedEvents { get; } = [];

        public Task ApplyAsync(IDomainEvent domainEvent)
        {
            AppliedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }
}
