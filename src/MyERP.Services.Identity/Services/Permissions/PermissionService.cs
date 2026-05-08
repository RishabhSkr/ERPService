using MyERP.Services.Identity.DTOs.Permissions;
using MyERP.Services.Identity.DTOs.Auth;
using MyERP.Services.Identity.Models;
using MyERP.Services.Identity.Repositories;
using MyERP.Services.Identity.Exceptions;

namespace MyERP.Services.Identity.Services.Permissions
{
    public class PermissionService : IPermissionService
    {
        private readonly IUserRepository _userRepo;

        public PermissionService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<ModuleDto>> GetAllModulesAsync()
        {
            var modules = await _userRepo.GetAllModulesAsync();
            return modules.Select(m => new ModuleDto
            {
                ModuleId = m.Id,
                ModuleName = m.ModuleName,
                ModuleCode = m.ModuleCode,
                DisplayOrder = m.DisplayOrder,
                HasAccess = true
            }).ToList();
        }
        
        public async Task CreateModuleAsync(Module module)
        {
            // Directly use context through repo or add a new method
            await _userRepo.CreateModuleAsync(module);
        }

        public async Task<bool> CheckPermissionAsync(string roleName, string endpoint, string httpMethod)
        {
            // SuperAdmin bypass - has access to everything
            if (roleName.Equals(SystemConstants.RoleSuperAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            
            var roles = await _userRepo.GetAllRolesAsync();
            var role = roles.FirstOrDefault(r => r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null) return false;
            
            var permissions = await _userRepo.GetPermissionsByRoleIdAsync(role.Id);
            
            return permissions.Any(p => 
                p.ApiEndpoint.Equals(endpoint, StringComparison.OrdinalIgnoreCase) &&
                p.HttpMethod.Equals(httpMethod, StringComparison.OrdinalIgnoreCase) &&
                p.IsGranted);
        }

        public async Task<List<RolePermissionDto>> GetRolePermissionsByRoleIdAsync(Guid roleId)
        {
            var permissions = await _userRepo.GetPermissionsByRoleIdAsync(roleId);
            
            return permissions.Select(p => new RolePermissionDto
            {
                RolePermissionId = p.RolePermissionId,
                ModuleId = p.ModuleId,
                ModuleName = p.Module?.ModuleName ?? "Unknown",
                PermissionName = p.PermissionName,
                ApiEndpoint = p.ApiEndpoint,
                HttpMethod = p.HttpMethod,
                IsGranted = p.IsGranted 
            }).ToList();
        }

        public async Task GrantPermissionAsync(GrantPermissionDto request)
        {
            // Check if already exists (even if revoked)
            var existing = await _userRepo.GetPermissionAsync(request.RoleId, request.ModuleId, request.PermissionName);
            if (existing != null)
            {
                // Re-activate existing permission
                existing.IsGranted = true;
                existing.ApiEndpoint = request.ApiEndpoint;
                existing.HttpMethod = request.HttpMethod;
                await _userRepo.UpdatePermissionAsync(existing);
                return; // Don't create new if we just updated existing
            }

            var permission = new RolePermission
            {
                RolePermissionId = Guid.NewGuid(),
                RoleId = request.RoleId,
                ModuleId = request.ModuleId,
                PermissionName = request.PermissionName,
                ApiEndpoint = request.ApiEndpoint,
                HttpMethod = request.HttpMethod,
                IsGranted = request.IsGranted
            };

            await _userRepo.GrantPermissionAsync(permission);
        }

        public async Task RevokePermissionAsync(Guid rolePermissionId)
        {
             var permission = await _userRepo.GetPermissionByIdAsync(rolePermissionId);
             if (permission != null)
             {
                 await _userRepo.RemovePermissionAsync(permission);
             }
        }
    }
}
