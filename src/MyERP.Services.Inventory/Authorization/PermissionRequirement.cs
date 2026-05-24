using Microsoft.AspNetCore.Authorization;
namespace MyERP.Services.Inventory.Authorization;
public class PermissionRequirement : IAuthorizationRequirement
{
    // Empty class - just a marker that tells ASP.NET to use our handler
}
