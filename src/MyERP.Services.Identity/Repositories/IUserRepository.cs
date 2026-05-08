using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.DTOs.Roles;
using MyERP.Services.Identity.Models;

namespace MyERP.Services.Identity.Repositories
{
    public interface IUserRepository
    {
        // User Methods
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsAsync(string email);
        Task CreateAsync(User user);
        
        // New CRUD Methods
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid userId);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid userId);

        // Role & Permissions Methods
        Task<string?> GetRoleNameAsync(Guid roleId);
        Task<List<ModuleDto>> GetAccessibleModulesAsync(Guid roleId);
        
        // New Role CRUD
        Task<List<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(Guid roleId);
        Task CreateRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(Guid roleId);

        // Permission Methods
        Task<List<RolePermission>> GetPermissionsByRoleIdAsync(Guid roleId);
        Task<RolePermission?> GetPermissionAsync(Guid roleId, Guid moduleId, string permissionName);
        Task<RolePermission?> GetPermissionByIdAsync(Guid rolePermissionId);
        Task GrantPermissionAsync(RolePermission permission);
        Task RemovePermissionAsync(RolePermission permission);
        Task UpdatePermissionAsync(RolePermission permission);
        Task<List<Module>> GetAllModulesAsync();
        Task ApproveUserAsync(Guid userId, Guid roleId);
        Task SuspendUserAsync(Guid userId);
        Task CreateModuleAsync(Module module);

        // Refresh Token Methods (New)
        Task SaveRefreshTokenAsync(RefreshToken token);
        Task<bool> HasPermissionAsync(Guid roleId, string path, string method);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken token);
    }
}