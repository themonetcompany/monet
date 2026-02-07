namespace Monet.Domain;

public record ConnectedUser
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
}