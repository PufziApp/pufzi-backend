using System.Net;
using System.Text.Json;

namespace Pufzi.Api.Middleware;

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
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogWarning(
                "Unauthorized request. Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.Unauthorized,
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            _logger.LogWarning(
                "Resource not found. Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.NotFound,
                exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(
                "Invalid operation. Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.BadRequest,
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "A apărut o eroare internă. Încearcă din nou.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            statusCode = (int)statusCode,
            message
        };

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}