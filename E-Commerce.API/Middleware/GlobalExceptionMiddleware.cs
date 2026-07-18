using System.Net;
using System.Text.Json;
using ECommerce.API.Extensions;
using ECommerce.API.Helpers;
using ECommerce.Application.Common.Exceptions;
using FluentValidation;

namespace ECommerce.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        var (statusCode, message, errors) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Message, (IEnumerable<string>?)null),
            ValidationException validationEx => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed.",
                validationEx.Errors.Select(e => e.ErrorMessage)),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.", (IEnumerable<string>?)null)
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred.");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(message, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
