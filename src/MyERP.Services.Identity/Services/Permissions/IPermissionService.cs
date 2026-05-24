using MyERP.Services.Identity.DTOs.Permissions;
using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.Models;

namespace MyERP.Services.Identity.Services.Permissions
{
    public interface IPermissionService
    {
        Task<List<ModuleDto>> GetAllModulesAsync();
        Task CreateModuleAsync(Module module);
        Task<List<RolePermissionDto>> GetRolePermissionsByRoleIdAsync(Guid roleId);
        Task GrantPermissionAsync(GrantPermissionDto request);
        Task RevokePermissionAsync(Guid rolePermissionId);
        Task<bool> CheckPermissionAsync(string roleName, string endpoint, string httpMethod);
    }
}
