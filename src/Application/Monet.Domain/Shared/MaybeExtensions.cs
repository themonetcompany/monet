namespace Monet.Domain.Shared;

public static class MaybeExtensions
{
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Maybe<T> maybe,
        Func<T, Task<TResult>> some,
        Func<TResult> none)
    {
        return maybe switch
        {
            Maybe<T>.Some(var value) => await some(value),
            Maybe<T>.None => none(),
            _ => throw new InvalidOperationException()
        };
    }
}