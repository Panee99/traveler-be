namespace Shared.ResultExtensions;

public class Result
{
    protected static readonly Error NoError =
        Error.Custom(ErrorType.Unexpected, "General.NoError", "Success result has no error.");

    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, NoError);
    }

    public static implicit operator Result(Error error)
    {
        return new Result(false, error);
    }

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onError)
    {
        return IsSuccess ? onSuccess() : onError(Error);
    }
}