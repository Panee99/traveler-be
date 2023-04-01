using System.Net;
using Application.Commons;
using Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Service.Models.Auth;
using Shared;
using Shared.Enums;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Application.Configurations.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly AccountRole[] _roles;

    public AuthorizeAttribute()
    {
        _roles = Array.Empty<AccountRole>();
    }

    public AuthorizeAttribute(params AccountRole[] roles)
    {
        _checkDuplicate(roles);
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // [AllowAnonymous]
        if (context.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>().Any()) return;

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
        return new ObjectResult(new ErrorResponsePayload
        {
            Timestamp = DateTimeHelper.VnNow(),
            Code = error.Code,
            Message = error.Message,
            Details = error.ErrorDetails
        })
        {
            StatusCode = StatusCode
        };
    }

    private void _checkDuplicate(AccountRole[] roles)
    {
        var hasDuplicates = roles.Length != roles.Distinct().Count();
        if (hasDuplicates)
            throw new ArgumentException("\"Authorize\" attribute parameter \"roles\" can not have duplicates");
    }
}