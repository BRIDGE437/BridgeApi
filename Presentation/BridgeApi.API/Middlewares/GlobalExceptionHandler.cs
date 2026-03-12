using BridgeApi.API.Models;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using AuthenticationException = BridgeApi.Application.Exceptions.AuthenticationException;

namespace BridgeApi.API.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            _logger.LogWarning(
                validationException,
                "Validation failed for {Path}. Errors: {@Errors}",
                httpContext.Request.Path,
                errors);

            var response = new ValidationErrorResponse(errors);

            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(response),
                cancellationToken);

            return true;
        }

        if (exception is AuthenticationException authException)
        {
            _logger.LogWarning(
                authException,
                "Authentication failed for {Path}: {Message}",
                httpContext.Request.Path,
                authException.Message);

            var response = new BadRequestErrorResponse(authException.Message);

            httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(response),
                cancellationToken);

            return true;
        }

        if (exception is UnauthorizedAccessException unauthorizedException)
        {
            _logger.LogWarning(
                unauthorizedException,
                "Unauthorized access for {Path}: {Message}",
                httpContext.Request.Path,
                unauthorizedException.Message);

            var forbiddenResponse = new BadRequestErrorResponse(unauthorizedException.Message);

            httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(forbiddenResponse),
                cancellationToken);

            return true;
        }

        if (exception is DbUpdateException dbEx && dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == "23503")
        {
            var response = new BadRequestErrorResponse("Referenced entity not found. One or more referenced entities do not exist.");

            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            await httpContext.Response.WriteAsync(
                JsonSerializer.Serialize(response),
                cancellationToken);

            return true;
        }

        _logger.LogError(exception, "Unhandled exception");

        var internalResponse = new InternalErrorResponse(
            Title: "An error occurred",
            Status: (int)HttpStatusCode.InternalServerError);

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(internalResponse),
            cancellationToken);

        return true;
    }
}
