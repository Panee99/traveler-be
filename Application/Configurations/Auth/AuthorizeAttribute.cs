using System.Net;
using Application.Commons;
using Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Service.Models.Auth;
using Shared;
using Shared.Helpers;
using Shared.ResultExtensions;

namespace Application.Configurations.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole[] _roles;

    public AuthorizeAttribute()
    {
        _roles = Array.Empty<UserRole>();
    }

    public AuthorizeAttribute(params UserRole[] roles)
    {
        _checkDuplicate(roles);
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext filterContext)
    {
        // [AllowAnonymous]
        if (filterContext.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>().Any()) return;


        var user = (AuthUser?)filterContext.HttpContext.Items[AppConstants.UserContextKey];
        {
            // Unauthorized
            if (user is null)
            {
                filterContext.Result = _generateErrorResponse(Error.Authentication(), (int)HttpStatusCode.Unauthorized);
                return;
            }

            // Allow
            if (_roles.Length == 0) return;
            if (_roles.Contains(user.Role)) return;
        }

        // Forbidden
        filterContext.Result = _generateErrorResponse(Error.Authorization(), (int)HttpStatusCode.Forbidden);
    }

    // PRIVATE
    private ObjectResult _generateErrorResponse(Error error, int statusCode)
    {
        return new ObjectResult(new ErrorResponsePayload
        {
            Timestamp = DateTimeHelper.VnNow(),
            Code = error.Code,
            Message = error.Message,
            Details = error.ErrorDetails
        })
        {
            StatusCode = statusCode
        };
    }

    private void _checkDuplicate(UserRole[] roles)
    {
        var hasDuplicates = roles.Length != roles.Distinct().Count();
        if (hasDuplicates)
            throw new ArgumentException("\"Authorize\" attribute parameter \"roles\" can not have duplicates");
    }
}