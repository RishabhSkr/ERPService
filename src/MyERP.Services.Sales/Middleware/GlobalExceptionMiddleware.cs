using System.Text.Json;
using MyERP.Services.Sales.DTOs;
using MyERP.Services.Sales.Exceptions;

namespace MyERP.Services.Sales.Middleware
{
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                ValidationException validationEx => new
                {
                    success = false,
                    message = validationEx.Message,
                    errors = validationEx.Errors,
                    statusCode = validationEx.StatusCode
                },
                AppException appEx => new
                {
                    success = false,
                    message = appEx.Message,
                    errors = (Dictionary<string, string[]>?)null,
                    statusCode = appEx.StatusCode
                },
                _ => new
                {
                    success = false,
                    message = "An unexpected error occurred",
                    errors = (Dictionary<string, string[]>?)null,
                    statusCode = 500
                }
            };

            context.Response.StatusCode = response.statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
