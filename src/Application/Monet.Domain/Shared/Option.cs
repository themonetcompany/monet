namespace Monet.Domain.Shared;

public abstract record Option<T>
{
    public sealed record Some(T Value) : Option<T>;
    public sealed record None : Option<T>;

    public static Option<T> Of(T? value)
        => value is null ? new None() : new Some(value);
}