using Serilog;
using Shared;

namespace Application.Configurations.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;

    static ErrorLoggingMiddleware()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Month,
                outputTemplate:
                "{Properties:j}{NewLine}{Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public ErrorLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error Logging Middleware");
            Console.WriteLine($"Time {DateTimeHelper.VnNow()}");
            throw;
        }
    }
}