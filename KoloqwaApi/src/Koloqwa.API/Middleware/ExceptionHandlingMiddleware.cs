using System.Net;
using System.Text.Json;
using Koloqwa.Application.Common.Exceptions;
using Koloqwa.Domain.Exceptions;

namespace Koloqwa.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve =>
                (HttpStatusCode.BadRequest, "Validation failed",
                 (object)ve.Errors),

            NotFoundException ne =>
                (HttpStatusCode.NotFound, ne.Message, (object?)null),

            ConflictException ce =>
                (HttpStatusCode.Conflict, ce.Message, (object?)null),

            UnauthorizedException ue =>
                (HttpStatusCode.Unauthorized, ue.Message, (object?)null),

            DomainException de =>
                (HttpStatusCode.BadRequest, de.Message, (object?)null),

            _ =>
                (HttpStatusCode.InternalServerError,
                 "An unexpected error occurred.", (object?)null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Success = false,
            Message = title,
            Errors = errors,
            TraceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
