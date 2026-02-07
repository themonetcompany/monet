namespace Monet.Domain.Shared;

public abstract record DomainEvent : IDomainEvent
{
    public required string AggregateId { get; init; }
    public required Guid Id { get; init; }
    public required int Version { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required Guid PublisherId { get; init; }
}