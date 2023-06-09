using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Data.EFCore;
using Data.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Models.Auth;
using Shared;
using Shared.Settings;

namespace Application.Middlewares;

public class HttpRequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<HttpRequestLoggingMiddleware> _logger;

    public HttpRequestLoggingMiddleware(ILogger<HttpRequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _logger.LogInformation("Method: {Method}, Path: {Path}", context.Request.Method, context.Request.Path);
        await next(context);
    }
}