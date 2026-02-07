using FluentAssertions;
using Monet.Application.Ports;
using Monet.Domain;
using Monet.Domain.Shared;

namespace Monet.Application.Tests.Shared;

public class FakeEventStore : IStoreEvent
{
    public List<IDomainEvent> PublishedEvents { get; } = [];
    public Task PublishAsync(IDomainEvent domainEvent)
    {
        PublishedEvents.Add(domainEvent);
        return Task.CompletedTask;
    }

    public Task<bool> HasAsync(string s, CancellationToken cancellationToken)
    {
        return Task.FromResult(PublishedEvents.Any(e => e.AggregateId == s));
    }

    public void ShouldContain(IDomainEvent domainEvent)
    {
        PublishedEvents.Should().Contain(domainEvent);
    }
}