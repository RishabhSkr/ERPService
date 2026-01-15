using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            return Ok(permissions);
        }

        [HttpPost("grant")]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto request)
        {
             try
             {
                 await _permissionService.GrantPermissionAsync(request);
                 return Ok("Permission granted");
             }
             catch (InvalidOperationException ex)
             {
                 return Conflict(ex.Message);
             }
        }

        [HttpDelete("revoke/{id}")]
        public async Task<IActionResult> RevokePermission(Guid id)
        {
            await _permissionService.RevokePermissionAsync(id);
            return NoContent();
        }
    }
}
