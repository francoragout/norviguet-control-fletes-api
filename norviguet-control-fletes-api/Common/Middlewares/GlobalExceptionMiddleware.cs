using Microsoft.AspNetCore.Mvc;
using norviguet_control_fletes_api.Common.Middlewares;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace norviguet_control_fletes_api.Common.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, title) = ex switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Resource not found"),
            ConflictException => (HttpStatusCode.Conflict, "Business rule violation"),
            ValidationException => (HttpStatusCode.BadRequest, "Validation error"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            _ => (HttpStatusCode.InternalServerError, "Internal server error")
        };

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = env.IsDevelopment() ? ex.Message : null
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status.Value;

        return context.Response.WriteAsJsonAsync(problem);
    }
}
