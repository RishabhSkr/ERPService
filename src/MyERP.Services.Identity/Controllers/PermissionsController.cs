using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Identity.DTOs; // For ApiResponse
using MyERP.Services.Identity.DTOs.Permissions;
using MyERP.Services.Identity.Services.Permissions;

namespace MyERP.Services.Identity.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRole(Guid roleId)
        {
            var permissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId);
            return Ok(ApiResponse<List<RolePermissionDto>>.Ok(permissions));
        }

        [HttpPost("grant")]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto request)
        {
             await _permissionService.GrantPermissionAsync(request);
             return Ok(ApiResponse.Ok("Permission granted successfully"));
        }

        [HttpDelete("revoke/{id}")]
        public async Task<IActionResult> RevokePermission(Guid id)
        {
            await _permissionService.RevokePermissionAsync(id);
            return Ok(ApiResponse.Ok("Permission revoked successfully"));
        }
    }
}
