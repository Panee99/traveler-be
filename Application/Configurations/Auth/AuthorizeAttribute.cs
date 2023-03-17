using System.Net;
using Application.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Service.Results;
using Shared;
using Shared.Enums;

namespace Application.Configurations.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _roles;

    public AuthorizeAttribute()
    {
        _roles = Array.Empty<UserRole>();
    }

    public AuthorizeAttribute(UserRole role)
    {
        _roles = new[] { role };
    }

    public AuthorizeAttribute(UserRole[] Roles)
    {
        _roles = Roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (AuthUser?)context.HttpContext.Items[AppConstants.UserContextKey];

        // Unauthorized
        if (user is null)
        {
            context.Result = _generateErrorResponse(Error.Authentication(), (int)HttpStatusCode.Unauthorized);
            return;
        }

        // Allow
        if (_roles.Length == 0) return;
        if (_roles.Contains(user.Role)) return;

        // Forbidden
        context.Result = _generateErrorResponse(Error.Authorization(), (int)HttpStatusCode.Forbidden);
    }

    // PRIVATE
    private ObjectResult _generateErrorResponse(Error error, int StatusCode)
    {
        return new ObjectResult(new ErrorResponsePayload()
        {
            Timestamp = DateTimeHelper.VnNow(),
            Code = error.Code,
            Message = error.Message,
            Details = error.ErrorDetails
        })
        {
            StatusCode = StatusCode,
        };
    }
}