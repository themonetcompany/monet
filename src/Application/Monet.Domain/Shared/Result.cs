#nullable disable
namespace Monet.Domain.Shared;

public interface IResult<out T>
{
    bool IsSuccess { get; }
    T Value { get; }
    string Code { get; }
}

public record Result<T> : IResult<T>
{
    public bool IsSuccess { get; set; }
    public T Value { get; init; }
    public string Code { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string code)
    {
        IsSuccess = false;
        Code = code;
    }

    public static IResult<T> Failure(string code)
    {
        return new Result<T>(code);
    }

    public static IResult<T> Success(T value)
    {
        return new Result<T>(value);
    }
}