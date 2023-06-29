using System.Net;
using Application.Commons;
using Microsoft.AspNetCore.Mvc;
using Service.Models.Auth;
using Shared;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Application.Controllers;

[ApiController]
[Produces("application/json")]
public class ApiController : ControllerBase
{
    // Must use with [Authorize]
    protected AuthUser CurrentUser => (AuthUser)HttpContext.Items[AppConstants.UserContextKey]!;

    protected static IActionResult OnError(Error error)
    {
        return new ObjectResult(new ErrorResponsePayload
        {
            Timestamp = DateTimeHelper.VnNow(),
            Code = error.Code,
            Message = error.Message,
            Details = error.ErrorDetails
        })
        {
            StatusCode = error.ErrorType switch
            {
                // Log.Warning("Unexpected error: {Code} - {Message}", error.Code, error.Message);
                ErrorType.Unexpected => (int)HttpStatusCode.InternalServerError,
                ErrorType.Validation => (int)HttpStatusCode.BadRequest,
                ErrorType.Conflict => (int)HttpStatusCode.Conflict,
                ErrorType.NotFound => (int)HttpStatusCode.NotFound,
                ErrorType.Authentication => (int)HttpStatusCode.Unauthorized,
                ErrorType.Authorization => (int)HttpStatusCode.Forbidden,
                _ => throw new ArgumentOutOfRangeException()
            }
        };
    }
}