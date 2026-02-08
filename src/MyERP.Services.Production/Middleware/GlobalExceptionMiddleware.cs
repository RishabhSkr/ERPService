/*
 * GlobalExceptionMiddleware
 * 
 * 📚 INDUSTRY vs NOOB:
 * 
 * ❌ NOOB: try-catch in every controller action
 * ✅ INDUSTRY:
 *    1. Centralized exception handling
 *    2. Consistent error response format
 *    3. Logging for debugging
 *    4. Hide internal details in production
 */

using System.Net;
using System.Text.Json;
using MyERP.Services.Production.Exceptions;

namespace MyERP.Services.Production.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
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
            // Log the error
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // Determine status code
            var statusCode = exception switch
            {
                AppException appEx => appEx.StatusCode,
                _ => (int)HttpStatusCode.InternalServerError
            };

            // Create response
            var response = new ApiResponse
            {
                Success = false,
                Message = exception.Message,
                Error = _env.IsDevelopment() ? exception.StackTrace : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }

    /// <summary>
    /// Standard API response wrapper
    /// 📝 Industry Practice: Consistent response format for all APIs
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message
            };
        }
    }
}
