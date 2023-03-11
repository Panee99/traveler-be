using System.Net;
using Application.Commons;
using Microsoft.AspNetCore.Mvc;
using Service.Results;

namespace Application.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    private readonly ILogger<ApiController> _logger;

    public ApiController(ILogger<ApiController> logger)
    {
        _logger = logger;
    }

    protected IActionResult OnError(Error error)
    {
        var response = new ObjectResult(new ErrorResponsePayload()
        {
            Timestamp = DateTime.Now,
            Code = error.Code,
            Message = error.Message,
            Details = error.ErrorDetails
        });

        switch (error.ErrorType)
        {
            case ErrorType.Unexpected:
                _logger.LogError("Unexpected error: {Code} - {Message}", error.Code, error.Message);
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
        }
        return response;
    }
}