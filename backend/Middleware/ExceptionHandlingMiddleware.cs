using System.Net;
using Microsoft.AspNetCore.Mvc;

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
                await WriteProblemDetailsAsync(context, ex);
            }
        }

        private async Task WriteProblemDetailsAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = MapException(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = $"https://tools.ietf.org/html/rfc9110#section-15.{StatusCodeSection(statusCode)}",
                Instance = context.Request.Path,
            };

            if (_environment.IsDevelopment())
            {
                problemDetails.Detail = exception.ToString();
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static (int StatusCode, string Title) MapException(Exception exception) => exception switch
        {
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid request"),
            InvalidOperationException => ((int)HttpStatusCode.Conflict, "Operation could not be completed"),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred"),
        };

        private static string StatusCodeSection(int statusCode) => statusCode switch
        {
            400 => "5.1",
            401 => "5.2",
            404 => "5.5",
            409 => "5.10",
            _ => "6.1",
        };
    }
}
