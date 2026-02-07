namespace Monet.Domain.Shared;

public static class OptionExtensions
{
    public static async Task<TResult> MatchAsync<T, TResult>(
        this Option<T> option,
        Func<T, Task<TResult>> some,
        Func<TResult> none)
    {
        return option switch
        {
            Option<T>.Some(var value) => await some(value),
            Option<T>.None => none(),
            _ => throw new InvalidOperationException()
        };
    }
}