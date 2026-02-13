using FluentAssertions;
using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;
using Monet.WebApp.Transactions;

namespace Monet.WebApp.Tests.Transactions;

public class CategoryBootstrapperTests
{
    [Fact]
    public async Task Given_EmptyStore_When_Bootstrap_Then_ShouldCreateDefaultCategories()
    {
        var store = new RecordingEventStore();
        var bootstrapper = new CategoryBootstrapper(store, new FixedClock(), new DeterministicGuidGenerator());

        await bootstrapper.BootstrapAsync(CancellationToken.None);

        var createdCategories = store.Events.OfType<CategoryCreated>().ToList();
        createdCategories.Should().HaveCount(15);
        createdCategories.Should().Contain(category => category.AggregateId == "Category-Expense-Alimentation");
        createdCategories.Should().Contain(category => category.AggregateId == "Category-Income-Salaire");
    }

    [Fact]
    public async Task Given_AlreadyBootstrappedStore_When_Bootstrap_Then_ShouldNotCreateDuplicates()
    {
        var store = new RecordingEventStore();
        var bootstrapper = new CategoryBootstrapper(store, new FixedClock(), new DeterministicGuidGenerator());

        await bootstrapper.BootstrapAsync(CancellationToken.None);
        await bootstrapper.BootstrapAsync(CancellationToken.None);

        var createdCategories = store.Events.OfType<CategoryCreated>().ToList();
        createdCategories.Should().HaveCount(15);
        createdCategories.Select(category => category.AggregateId).Distinct().Should().HaveCount(15);
    }

    private sealed class RecordingEventStore : IStoreEvent
    {
        public List<IDomainEvent> Events { get; } = [];

        public Task PublishAsync(IDomainEvent domainEvent)
        {
            Events.Add(domainEvent);
            return Task.CompletedTask;
        }

        public Task<bool> HasAsync(string aggregateId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Events.Any(domainEvent => domainEvent.AggregateId == aggregateId));
        }

        public Task<int> GetCurrentVersionAsync(string aggregateId, CancellationToken cancellationToken)
        {
            var currentVersion = Events
                .Where(domainEvent => domainEvent.AggregateId == aggregateId)
                .Select(domainEvent => domainEvent.Version)
                .DefaultIfEmpty(0)
                .Max();

            return Task.FromResult(currentVersion);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset Now => new(2026, 02, 12, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class DeterministicGuidGenerator : IGenerateGuid
    {
        private int _counter;

        public Guid New()
        {
            _counter++;
            return Guid.Parse($"00000000-0000-0000-0000-{_counter:000000000000}");
        }
    }
}
