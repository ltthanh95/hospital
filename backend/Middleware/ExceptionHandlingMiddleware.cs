using System.Net;
using backend.Exceptions;
using backend.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace backend.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteApiResponseAsync(context, ex);
            }
        }

        private async Task WriteApiResponseAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = MapException(exception);
            var message = _environment.IsDevelopment() ? $"{title}: {exception.Message}" : title;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message, statusCode));
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            ForbiddenAccessException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid request"),
            DbUpdateException => ((int)HttpStatusCode.Conflict, "Operation could not be completed because related records exist"),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, "Operation could not be completed"),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred"),
        };
    }
}
