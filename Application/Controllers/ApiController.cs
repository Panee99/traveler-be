using System.Net;
using Application.Commons;
using Application.Configurations.Auth;
using Microsoft.AspNetCore.Mvc;
using Service.Results;
using Shared;

namespace Application.Controllers;

[ApiController]
[Produces("application/json")]
[Authorize]
public class ApiController : ControllerBase
{
    protected AuthUser CurrentUser => (AuthUser) HttpContext.Items[AppConstants.UserContextKey]!;

    protected static IActionResult OnError(Error error)
    {
        var response = new ObjectResult(new ErrorResponsePayload()
        {
            Timestamp = DateTimeHelper.VnNow(),
            Code = error.Code,
            Message = error.Message,
            Details = error.ErrorDetails
        });

        switch (error.ErrorType)
        {
            case ErrorType.Unexpected:
                // Log.Warning("Unexpected error: {Code} - {Message}", error.Code, error.Message);
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
            case ErrorType.Validation:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case ErrorType.Conflict:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                break;
            case ErrorType.NotFound:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            case ErrorType.Authentication:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return response;
    }
}