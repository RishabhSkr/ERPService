using MyERP.Services.Identity.DTOs.Roles;

namespace MyERP.Services.Identity.Services.Roles
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(Guid roleId);
        Task<Guid> CreateRoleAsync(RoleDto request);
        Task UpdateRoleAsync(RoleDto request);
        Task DeleteRoleAsync(Guid roleId);
    }
}
