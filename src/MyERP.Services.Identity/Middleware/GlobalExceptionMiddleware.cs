using System.Net;
using System.Text.Json;
using MyERP.Services.Identity.DTOs;
using MyERP.Services.Identity.Exceptions;

namespace MyERP.Services.Identity.Middleware
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

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                await HandleExceptionAsync(context, error);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred.");

            var response = context.Response;
            response.ContentType = "application/json";

            var responseModel = new ApiResponse<string> { Success = false, Data = null };

            switch (exception)
            {
                case AppException e:
                    // Custom application error
                    response.StatusCode = e.StatusCode;
                    responseModel.Message = e.Message;
                    responseModel.StatusCode = e.StatusCode;
                    break;

                case KeyNotFoundException e:
                    // Not found error
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    responseModel.Message = e.Message;
                    responseModel.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                
                case UnauthorizedAccessException e:
                     response.StatusCode = (int)HttpStatusCode.Unauthorized;
                     responseModel.Message = "Unauthorized Access";
                     responseModel.StatusCode = (int)HttpStatusCode.Unauthorized;
                     break;

                default:
                    // Unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseModel.Message = "Internal Server Error: " + exception.Message; // Dev mode: show message. Prod: Hide.
                    responseModel.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var result = JsonSerializer.Serialize(responseModel, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await response.WriteAsync(result);
        }
    }
}
