namespace Shared.ResultExtensions;

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value, bool isSuccess, Error error) : base(
        isSuccess, error)
    {
        _value = value;
    }

    private Result(bool isSuccess, Error error) : base(
        isSuccess, error)
    {
    }

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Error result have no value");

    public static implicit operator Result<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, NoError);
    }

    public static implicit operator Result<TValue>(Error error)
    {
        return new Result<TValue>(false, error);
    }

    public TResult Match<TResult>(Func<TValue, TResult> onValue, Func<Error, TResult> onError)
    {
        return IsSuccess ? onValue(_value!) : onError(Error);
    }
}