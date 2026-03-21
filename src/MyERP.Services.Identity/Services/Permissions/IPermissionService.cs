using MyERP.Services.Identity.DTOs.Permissions;

namespace MyERP.Services.Identity.Services.Permissions
{
    public interface IPermissionService
    {
        Task<List<RolePermissionDto>> GetRolePermissionsByRoleIdAsync(Guid roleId);
        Task GrantPermissionAsync(GrantPermissionDto request);
        Task RevokePermissionAsync(Guid rolePermissionId);
        Task<bool> CheckPermissionAsync(string roleName, string endpoint, string httpMethod);
    }
}
