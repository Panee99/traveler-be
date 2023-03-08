namespace Service.Results;

public class Result<TValue> : Result where TValue : class
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Error result have no value");

    public static implicit operator Result<TValue>(TValue value) => new(value, true, NoError);

    public static implicit operator Result<TValue>(Error error) => new(null, false, error);

    private Result(TValue? value, bool isSuccess, Error error) : base(
        isSuccess, error)
    {
        _value = value;
    }

    public TResult Match<TResult>(Func<TValue, TResult> onValue, Func<Error, TResult> onError) =>
        IsSuccess ? onValue(_value!) : onError(Error);
}