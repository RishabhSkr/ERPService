using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Identity.DTOs; // For ApiResponse
using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.DTOs.Permissions;
using MyERP.Services.Identity.Services.Permissions;
using MyERP.Services.Identity.Models;

namespace MyERP.Services.Identity.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }


        [HttpGet("modules")]
        [Authorize]
        public async Task<IActionResult> GetAllModules()
        {
            var modules = await _permissionService.GetAllModulesAsync();
            return Ok(ApiResponse<List<ModuleDto>>.Ok(modules));
        }

        [HttpPost("modules")]
        [Authorize]
        public async Task<IActionResult> CreateModule([FromBody] ModuleDto request)
        {
            var module = new Module
            {
                Id = Guid.NewGuid(),
                ModuleName = request.ModuleName,
                ModuleCode = request.ModuleCode,
                DisplayOrder = request.DisplayOrder,
                IsActive = true
            };
            // Direct save (simple approach)
            await _permissionService.CreateModuleAsync(module);
            return Ok(ApiResponse.Ok("Module created successfully"));
        }


        [HttpGet("role/{roleId}")]
        [Authorize]
        public async Task<IActionResult> GetByRole(Guid roleId)
        {
            var permissions = await _permissionService.GetRolePermissionsByRoleIdAsync(roleId);
            return Ok(ApiResponse<List<RolePermissionDto>>.Ok(permissions));
        }

        [HttpPost("grant")]
        [Authorize]
        public async Task<IActionResult> GrantPermission([FromBody] GrantPermissionDto request)
        {
             await _permissionService.GrantPermissionAsync(request);
             return Ok(ApiResponse.Ok("Permission granted successfully"));
        }

        [HttpPost("check")]
        public async Task<IActionResult> CheckPermission([FromBody] CheckPermissionDto request)
        {
            var hasAccess = await _permissionService.CheckPermissionAsync(
                request.RoleName, 
                request.Endpoint, 
                request.HttpMethod);
            
            return Ok(ApiResponse<CheckPermissionResultDto>.Ok(
                new CheckPermissionResultDto { HasAccess = hasAccess }));
        }

        [HttpDelete("revoke/{id}")]
        [Authorize]
        public async Task<IActionResult> RevokePermission(Guid id)
        {
            await _permissionService.RevokePermissionAsync(id);
            return Ok(ApiResponse.Ok("Permission revoked successfully"));
        }
    }
}
