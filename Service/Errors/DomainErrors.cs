using Service.Results;

namespace Service.Errors;

public static class DomainErrors
{
    public static class Traveler
    {
        public static readonly Error IdToken =
            Error.Custom(ErrorType.Validation, "Traveler.IdToken", "Verify idToken failed.");
    }
}