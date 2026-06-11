using System.Net;
using System.Text.Json;
using FluentValidation;

namespace EnviroWatch.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var payload = JsonSerializer.Serialize(new
            {
                error = "Validation failed.",
                errors = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }),
                traceId = context.TraceIdentifier
            });

            await context.Response.WriteAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var payload = JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred.",
                traceId = context.TraceIdentifier
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
