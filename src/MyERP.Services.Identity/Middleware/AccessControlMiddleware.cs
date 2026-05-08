using System.Security.Claims;
using MyERP.Services.Identity.Repositories;

namespace MyERP.Services.Identity.Middleware
{
    public class AccessControlMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessControlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Bypass logic (e.g. Public endpoints, Swagger)
            var path = context.Request.Path.Value?.ToLower() ?? "";
            
            // Allow Auth endpoints, Permission check, and Public assets
            if (path.StartsWith("/api/auth") || 
                path.StartsWith("/api/permissions/check") ||  // For inter-service permission checks
                path.StartsWith("/swagger") || 
                path == "/")
            {
                await _next(context);
                return;
            }

            // 2. Check Authentication
            if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                // If endpoint requires auth (which logic handles elsewhere usually, but let's be safe), 
                // we can let standard [Authorize] handle 401. 
                // But if we want to enforce here:
                // await _next(context); return; 
                // Let's assume standard [Authorize] is used on controllers to ensure Identity is populated.
                
                // If endpoints are NOT marked [Authorize], this middleware runs even for anonymous.
                // WE SHOULD BE CAREFUL.
                // If usage is mixed, we might block public endpoints if not whitelisted above.
                // Assuming all /api/ endpoints (except auth) require protection.
                
                if (path.StartsWith("/api/"))
                {
                     context.Response.StatusCode = 401; // Unauthorized
                     await context.Response.WriteAsync("Unauthorized");
                     return;
                }
                
                await _next(context);
                return;
            }

            // 3. Resolve Scoped Service (Repositories)
            // Middleware is singleton/transient, but DbContext is Scoped.
            // We cannot inject Repository in Constructor. We must use context.RequestServices.
            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                context.Response.StatusCode = 401;
                return;
            }

            var user = await userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                return;
            }

            // 4. SuperAdmin Bypass — full access to everything
            if (user.Role?.RoleName == SystemConstants.RoleSuperAdmin)
            {
                await _next(context);
                return;
            }

            // 5. Check Permissions
            // path: "/api/users/123", method: "DELETE"
            // permission: endpoint "/api/users", method "DELETE"
            
            var method = context.Request.Method;
            bool hasPermission = await userRepo.HasPermissionAsync(user.RoleId, path, method);

            if (!hasPermission)
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsJsonAsync(new { Message = "Access Denied: Insufficient Permissions" });
                return;
            }

            await _next(context);
        }
    }
}
