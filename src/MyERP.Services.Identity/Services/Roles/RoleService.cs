using MyERP.Services.Identity.DTOs.Roles;
using MyERP.Services.Identity.Models;
using MyERP.Services.Identity.Repositories;
using MyERP.Services.Identity.Exceptions;

namespace MyERP.Services.Identity.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly IUserRepository _userRepo;

        public RoleService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _userRepo.GetAllRolesAsync();
            
            return roles.Select(r => new RoleDto
            {
                RoleId = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                IsActive = r.IsActive,
                UserCount = r.Users?.Count ?? 0,
                PermissionCount = r.RolePermissions?.Count ?? 0,
                CreatedAt = DateTime.UtcNow // Entity doesn't have CreatedAt, maybe add later? ignoring for now.
            }).ToList();
        }

        public async Task<RoleDto> GetRoleByIdAsync(Guid roleId)
        {
            var r = await _userRepo.GetRoleByIdAsync(roleId);
            if (r == null) throw new NotFoundException("Role not found");

            return new RoleDto
            {
                RoleId = r.Id,
                RoleName = r.RoleName,
                Description = r.Description,
                IsActive = r.IsActive,
                UserCount = r.Users?.Count ?? 0,
                PermissionCount = r.RolePermissions?.Count ?? 0
            };
        }

        public async Task<Guid> CreateRoleAsync(RoleDto request)
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                RoleName = request.RoleName,
                Description = request.Description,
                IsActive = request.IsActive?? true
            };

            await _userRepo.CreateRoleAsync(role);
            return role.Id;
        }

        public async Task UpdateRoleAsync(RoleDto request)
        {
            var role = await _userRepo.GetRoleByIdAsync(request.RoleId);
            if (role == null) throw new NotFoundException("Role not found");

            role.RoleName = request.RoleName;
            role.Description = request.Description;
            role.IsActive = request.IsActive ?? role.IsActive; // Keep existing if null, or default to true/false logic as desired. Spec says default 1 on create. Update? Let's say keep existing.

            await _userRepo.UpdateRoleAsync(role);
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            // Optional: Check if role has users?
            await _userRepo.DeleteRoleAsync(roleId);
        }
    }
}
