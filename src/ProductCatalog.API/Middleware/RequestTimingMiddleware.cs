using System.Diagnostics;

namespace ProductCatalog.API.Middleware;

/// <summary>
/// Custom middleware that logs request timing information
/// Implemented manually without UseMiddleware helper
/// </summary>
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Request started: {Method} {Path} at {StartTime}",
            context.Request.Method,
            context.Request.Path,
            startTime);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var endTime = DateTime.UtcNow;

            _logger.LogInformation(
                "Request completed: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | Start: {StartTime} | End: {EndTime}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                startTime,
                endTime);
        }
    }
}
