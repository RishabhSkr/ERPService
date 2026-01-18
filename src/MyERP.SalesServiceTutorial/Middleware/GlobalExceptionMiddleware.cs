using System.Net;
using MyERP.SalesServiceTutorial.Common.Responses;
using MyERP.SalesServiceTutorial.Common.Exceptions;

namespace MyERP.SalesServiceTutorial.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    
    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(ApiResponse.Fail("Something went wrong"));
        }
    }
}