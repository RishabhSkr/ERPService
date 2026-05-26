using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MyERP.Services.Sales.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PermissionHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<PermissionHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        try
        {
            // 1. Get Role from token claims
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            
            if (roleClaim == null || userIdClaim == null)
            {
                _logger.LogWarning("No role or user claim found in token");
                context.Fail();
                return;
            }

            // 2. Get current request path and method
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                context.Fail();
                return;
            }

            var endpoint = httpContext.Request.Path.Value ?? "";
            var method = httpContext.Request.Method;

            _logger.LogInformation("ðŸ” Checking permission: Role={RoleName} Endpoint={Endpoint} Method={Method}", 
                roleClaim.Value, endpoint, method);

            // 3. Call Identity Service to check permission
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10); // Render cold start timeout
            var identityUrl = _configuration["Services:IdentityService:BaseUrl"] ?? "http://localhost:5205";

            _logger.LogInformation("ðŸ“¡ Calling Identity Service at: {IdentityUrl}", identityUrl);

            var checkRequest = new
            {
                RoleName = roleClaim.Value,
                Endpoint = endpoint,
                HttpMethod = method
            };

            var json = JsonSerializer.Serialize(checkRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{identityUrl}/api/permissions/check", content);
            Console.WriteLine("Response Log Checking Permission:", response.StatusCode);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PermissionCheckResponse>(responseBody, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Data?.HasAccess == true)
                {
                    _logger.LogInformation("âœ… Permission GRANTED for {Endpoint}", endpoint);
                    context.Succeed(requirement);
                }
                else
                {
                    _logger.LogWarning("âŒ Permission DENIED for {Endpoint}", endpoint);
                    context.Fail();
                }
            }
            else
            {
                _logger.LogError("âŒ Identity Service returned {StatusCode}", response.StatusCode);
                context.Fail();
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "âš ï¸ Identity Service unreachable at configured URL. Check Services__IdentityService__BaseUrl env var on Render.");
            context.Fail();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "âš ï¸ Identity Service timeout (cold start?). Will retry on next request.");
            context.Fail();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Permission check failed");
            context.Fail();
        }
    }
}

// Response DTOs for deserializing Identity response
public class PermissionCheckResponse
{
    public bool Success { get; set; }
    public PermissionCheckData? Data { get; set; }
}

public class PermissionCheckData
{
    public bool HasAccess { get; set; }
}
