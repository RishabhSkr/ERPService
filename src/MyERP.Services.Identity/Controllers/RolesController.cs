using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyERP.Services.Identity.DTOs.Roles;
using MyERP.Services.Identity.Services.Roles;
using MyERP.Services.Identity.Exceptions;
using MyERP.Services.Identity.DTOs;

namespace MyERP.Services.Identity.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<List<RoleDto>>.Ok(roles));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(ApiResponse<RoleDto>.Ok(role));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto request)
        {
            var roleId = await _roleService.CreateRoleAsync(request);
            return Ok(ApiResponse<object>.Ok(new { RoleId = roleId }, "Role created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoleDto request)
        {
            if (id != request.RoleId) throw new AppException("ID mismatch");

            await _roleService.UpdateRoleAsync(request);
            return Ok(ApiResponse.Ok("Role updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _roleService.DeleteRoleAsync(id);
            return Ok(ApiResponse.Ok("Role deleted successfully"));
        }
    }
}
