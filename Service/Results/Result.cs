namespace Service.Results;

public class Result
{
    public bool IsSuccess { get; }

    public Error Error { get; }

    protected static readonly Error NoError = new(ErrorType.Unexpected, "General.NoError",
        "Success result has no error.");

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, NoError);

    public static implicit operator Result(Error error) => new(false, error);
    
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onError) =>
        IsSuccess ? onSuccess() : onError(Error);
}