namespace Shared.ResultExtensions;

public class Error
{
    public readonly ICollection<string> ErrorDetails;

    private Error(ErrorType errorType, string code, string message, ICollection<string>? errorDetails = null)
    {
        ErrorType = errorType;
        Code = code;
        Message = message;
        ErrorDetails = errorDetails ?? new List<string>();
    }

    public ErrorType ErrorType { get; }

    public string Code { get; }

    public string Message { get; }


    public static Error Custom(ErrorType errorType, string code, string message,
        params string[] errorDetails)
    {
        return new Error(errorType, code, message, errorDetails);
    }

    public static Error Validation(params string[] errorDetails)
    {
        return new Error(ErrorType.Validation, "General.Validation", "A validation error has occurred.", errorDetails);
    }

    public static Error Conflict(params string[] errorDetails)
    {
        return new Error(ErrorType.Conflict, "General.Conflict", "A conflict error has occurred.", errorDetails);
    }

    public static Error NotFound(params string[] errorDetails)
    {
        return new Error(ErrorType.NotFound, "General.NotFound", "A 'Not Found' error has occurred.", errorDetails);
    }

    public static Error Unexpected(params string[] errorDetails)
    {
        return new Error(ErrorType.Unexpected, "General.Unexpected", "A unexpected error has occurred.", errorDetails);
    }

    public static Error Authentication(params string[] errorDetails)
    {
        return new Error(ErrorType.Authentication, "General.Authentication", "Unauthorized", errorDetails);
    }

    public static Error Authorization(params string[] errorDetails)
    {
        return new Error(ErrorType.Authorization, "General.Authorization", "Forbidden", errorDetails);
    }
}