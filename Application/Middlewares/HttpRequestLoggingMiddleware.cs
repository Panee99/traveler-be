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
        var path = context.Request.Path.Value;

        if (path != null)
        {
            if (_isApiPath(path))
                _logger.LogInformation(
                    "Method: {Method}, Path: {Path}",
                    context.Request.Method, context.Request.Path);
        }

        await next(context);
    }

    private static bool _isApiPath(string path)
    {
        return !path.StartsWith("/swagger") && !path.StartsWith("/favicon.ico");
    }
}