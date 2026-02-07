namespace Monet.Domain.Shared;

public abstract record Maybe<T>
{
    public sealed record Some(T Value) : Maybe<T>;
    public sealed record None : Maybe<T>;

    public static Maybe<T> Of(T? value)
        => value is null ? new None() : new Some(value);
}