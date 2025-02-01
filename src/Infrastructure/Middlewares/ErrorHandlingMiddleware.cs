using System.Text.Json;
using MoodTracker_back.Domain.Exceptions;

namespace MoodTracker_back.Infrastructure.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        switch (exception)
        {
            case ValidationException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;
            case NotFoundException ex:
                response.StatusCode = StatusCodes.Status404NotFound;
                break;
            case UnauthorizedAccessException ex:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                break;
            default:
                _logger.LogError(exception, "Erro não tratado");
                response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        var result = JsonSerializer.Serialize(new
        {
            StatusCode = response.StatusCode,
            Message = exception.Message
        });

        await response.WriteAsync(result);
    }
}

