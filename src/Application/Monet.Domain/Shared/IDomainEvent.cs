namespace Monet.Domain.Shared;

public interface IDomainEvent
{
    string AggregateId { get; }
    Guid Id { get; }
    int Version { get; }
    DateTimeOffset Timestamp { get; }
    Guid PublisherId { get; }
}