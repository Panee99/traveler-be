namespace Service.Results;

public enum ErrorType
{
    Failure, // Exceptions
    Unexpected,
    Validation,
    Conflict,
    NotFound,
}