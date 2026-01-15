using System.Net;
using System.Text.Json;
using MyERP.Services.Inventory.DTOs;
using MyERP.Services.Inventory.Exceptions;

namespace MyERP.Services.Inventory.Middleware
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
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ApiResponse();
            
            switch (exception)
            {
                case InsufficientStockException insufficientEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new ApiResponse
                    {
                        Success = false,
                        Message = insufficientEx.Message,
                        StatusCode = insufficientEx.StatusCode,
                        Data = new { insufficientMaterials = insufficientEx.InsufficientMaterials }
                    };
                    break;

                case NotFoundException notFoundEx:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = ApiResponse.Fail(notFoundEx.Message, notFoundEx.StatusCode);
                    break;

                case ConflictException conflictEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response = ApiResponse.Fail(conflictEx.Message, conflictEx.StatusCode);
                    break;

                case BadRequestException badRequestEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = ApiResponse.Fail(badRequestEx.Message, badRequestEx.StatusCode);
                    break;

                case AppException appEx:
                    context.Response.StatusCode = appEx.StatusCode;
                    response = ApiResponse.Fail(appEx.Message, appEx.StatusCode);
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception occurred");
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = ApiResponse.Fail("An error occurred while processing your request", 500);
                    break;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
